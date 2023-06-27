using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.Rcon
{
    public class RconClient : IDisposable
    {
        public ListCommandsParser ListCommandsParser { get; set; } = new ();
        public ListSquadsParser ListSquadsParser { get; set; } = new ();
        public ListLayersParser ListLayersParser { get; set; } = new ();
        public ListLevelsParser ListLevelsParser { get; set; } = new ();
        public ListPlayersParser ListPlayersParser { get; set; } = new ();

        private readonly ChatMessageParser _chatMessageParser = new();
        private readonly SquadCreatedMessageParser _squadCreatedMessageParser = new();

        public bool IsStarted => _workerThread != null;
        
        private readonly IPEndPoint _endPoint;
        private readonly string _password;
        
        private int _packetIdCounter = 3;

        private DateTime _lastReceivedPing = DateTime.UtcNow;

        private readonly ConcurrentDictionary<int, RconClientCommandResult> _pendingCommandResults = new ();

        private readonly ConcurrentQueue<Packet[]> _packageWriteQueue = new ();

        private Thread? _workerThread;
        private CancellationTokenSource? _threadCancellationTokenSource;
        
        public event Action? Connected;
        public event Action<Packet>? PacketReceived; 
        public event Action<ChatMessage>? ChatMessageReceived;
        public event Action<SquadCreatedMessage>? SquadCreatedMessageReceived;
        public event Action<byte[]>? BytesReceived;
        public event Action? PingReceived; 

        public RconClient(
            IPEndPoint endPoint,
            string password
        )
        {
            _endPoint = endPoint;
            _password = password;
        }

        private void ThreadHandler(object _)
        {
            var cancellationTokenSource = _threadCancellationTokenSource;
            if (cancellationTokenSource == null)
            {
                throw new Exception("No cancellation token source provided for the thread");
            }

            var nextPing = DateTime.UtcNow;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                using var socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(_endPoint);
                Authenticate(socket);
                OnConnected();

                var buffer = new byte[4096 + 7]; // maximum packet size + 7 bytes of broken package body
                var actualBufferLength = 0;

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var dataAvailable = socket.Available;
                    if (dataAvailable > 0)
                    {
                        var dataToRead = Math.Min(buffer.Length - actualBufferLength, dataAvailable);

                        var bytesRead = socket.Receive(buffer, actualBufferLength, dataToRead, SocketFlags.None);
                        if (bytesRead != dataToRead)
                        {
                            throw new Exception("some bad bytes came in");
                        }

                        OnBytesReceived(buffer[actualBufferLength..(actualBufferLength + dataToRead)]);

                        actualBufferLength += bytesRead;
                    }

                    if (actualBufferLength > 0)
                    {
                        var packetSize = Packet.ParseSize(buffer[..4]);

                        if (packetSize <= actualBufferLength)
                        {
                            ShiftBytesLeft(buffer, 4);
                            actualBufferLength -= 4;
                            
                            Packet packet;

                            // check for broken package:
                            // The Squad server sends an invalid packet on an appending empty exec command packet.
                            // It has to be filtered out here, because it is not needed and makes no sense regarding the Source Rcon protocol.
                            if (packetSize == 10)
                            {
                                var maybeBrokenBuffer = buffer[..17];
                                packet = Packet.Parse(maybeBrokenBuffer);
                                if (packet.IsBroken)
                                {
                                    ShiftBytesLeft(buffer, 17);
                                    actualBufferLength -= 17;
                                }
                                else
                                {
                                    packet = Packet.Parse(buffer[..10]);
                                    ShiftBytesLeft(buffer, 10);
                                    actualBufferLength -= 10;

                                    try
                                    {
                                        ProcessPacket(packet);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }
                            }
                            else
                            {
                                packet = Packet.Parse(buffer[..packetSize]);
                                ShiftBytesLeft(buffer, packetSize);
                                actualBufferLength -= packetSize;
                                try
                                {
                                    ProcessPacket(packet);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                        }
                    }

                    while (_packageWriteQueue.TryDequeue(out var packetGroup))
                    {
                        Send(socket, packetGroup);
                    }

                    // ping s.t. the connection is not destroyed
                    if (nextPing < DateTime.UtcNow)
                    {
                        Send(socket, new Packet(3, PacketType.ServerDataExecCommand, "", false, Encoding.UTF8));
                        nextPing = DateTime.UtcNow.AddMinutes(1);
                    }

                    if (DateTime.UtcNow - _lastReceivedPing > TimeSpan.FromMinutes(2))
                    {
                        break;
                    }

                    Thread.Yield();
                }
                
                foreach (var result in _pendingCommandResults)
                {
                    result.Value.Cancel();
                }
            
                _pendingCommandResults.Clear();
                _packageWriteQueue.Clear();
                _packetIdCounter = 3;

                Thread.Yield(); // yield the thread before starting a new connection
            }
        }

        private static void ShiftBytesLeft(
            byte[] bytes,
            int shiftLength
        )
        {
            if (shiftLength >= bytes.Length)
            {
                throw new Exception("invalid shift length given");
            }
            
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = i + shiftLength >= bytes.Length ? (byte)0 : bytes[i + shiftLength];
            }
        }
        
        private void Authenticate(Socket socket)
        {
            var authPacketId = GetNextPacketId();
            var requestPacket = new Packet(
                authPacketId,
                PacketType.ServerDataAuth,
                _password
            );
            Send(socket, requestPacket);

            // we have to receive the first packet, though we do not need its contents.
            ReceivePacket(socket);
            var packet = ReceivePacket(socket);

            if (packet.Id == -1)
            {
                throw new Exception("invalid authentication password");
            }
        }

        private static Packet ReceivePacket(Socket socket)
        {
            var sizeBuffer = new byte[4];
            var bytesRead = socket.Receive(sizeBuffer);
            if (bytesRead != 4)
            {
                throw new Exception("invalid bytes read");
            }

            var packetSize = Packet.ParseSize(sizeBuffer);
            var packetBuffer = new byte[packetSize];
            bytesRead = socket.Receive(packetBuffer);
            if (bytesRead != packetSize)
            {
                throw new Exception("invalid bytes read");
            }

            return Packet.Parse(packetBuffer);
        }

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }
            
            _threadCancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(ThreadHandler)
            {
                IsBackground = true
            };
            _workerThread.Start();
        }
        
        public void Stop()
        {
            foreach (var result in _pendingCommandResults)
            {
                result.Value.Cancel();
            }
            
            _pendingCommandResults.Clear();
            _packageWriteQueue.Clear();
            _packetIdCounter = 3;
            
            _threadCancellationTokenSource?.Cancel();
            _workerThread?.Join();
            _threadCancellationTokenSource?.Dispose();

            _threadCancellationTokenSource = null;
            _workerThread = null;
        }

        public async Task<byte[]> WriteCommandAsync(
            string command,
            CancellationToken cancellationToken
        )
        {
            var packetId = GetNextPacketId();
            var commandPacket = new Packet(
                packetId,
                PacketType.ServerDataExecCommand,
                command,
                false,
                Encoding.UTF8
            );

            var emptyPacket = new Packet(
                packetId,
                PacketType.ServerDataExecCommand,
                Array.Empty<byte>()
            );

            var commandResult = new RconClientCommandResult();

            _pendingCommandResults[packetId] = commandResult;
            WritePackets(commandPacket, emptyPacket);

            var packets = await commandResult.Result;
            var packetsWithoutEnd = packets.ToArray()[..^1];
            
            return packetsWithoutEnd.SelectMany(x => x.Body).ToArray();
        }

        protected void WritePackets(
            params Packet[] packets
        )
        {
            _packageWriteQueue.Enqueue(packets);
        }

        protected int GetNextPacketId()
        {
            return Interlocked.Increment(ref _packetIdCounter);
        }
        
        protected virtual void ProcessPacket(Packet packet)
        {
            if (packet is { Id: 3, Type: PacketType.ServerDataResponseValue })
            {
                OnPingReceived();
                _lastReceivedPing = DateTime.UtcNow;
                return;
            }
            
            OnPacketReceived(packet);
            
            if (packet.Type == PacketType.ServerDataChatMessage)
            {
                var rawMessage = Encoding.UTF8.GetString(packet.Body);

                var chatMessage = _chatMessageParser.Parse(rawMessage);
                if (chatMessage.HasValue)
                {
                    OnChatMessageReceived(chatMessage.Value);
                    return;
                }

                var squadCreatedMessage = _squadCreatedMessageParser.Parse(rawMessage);
                if (!squadCreatedMessage.HasValue)
                {
                    return;
                }
                
                OnSquadCreatedMessageReceived(squadCreatedMessage.Value);
                return;
            }
            
            if (!_pendingCommandResults.TryGetValue(packet.Id, out var command))
            {
                return;
            }

            command.AddPacket(packet);

            if (packet is not
                {
                    Type: PacketType.ServerDataResponseValue,
                    Body: { Length: 0 }
                })
            {
                return;
            }

            command.Complete();
            _pendingCommandResults.TryRemove(packet.Id, out _);
        }
        
        private static void Send(
            Socket socket,
            Packet packet
        )
        {
            var packetBytes = packet.ToArray();

            var bytesSent = socket.Send(packetBytes);
            if (bytesSent != packetBytes.Length)
            {
                throw new Exception("some bad packages wanted to get sent");
            }
        }

        private static void Send(
            Socket socket,
            params Packet[] packets
        )
        {
            var packetBytes = packets
                .SelectMany(x => x.ToArray())
                .ToArray();
            
            var bytesSent = socket.Send(packetBytes);
            if (bytesSent != packetBytes.Length)
            {
                throw new Exception("some bad packages wanted to get sent");
            }
        }

        public void Dispose()
        {
            Stop();
        }

        #region Command Definitions

        public async Task<List<Command>> ListCommandsAsync(
            CancellationToken cancellationToken
        )
        {
            var result = await WriteCommandAsync("ListCommands true", cancellationToken);

            return ListCommandsParser.Parse(Encoding.UTF8.GetString(result));
        }

        public async Task<List<Models.Squad>> ListSquadsAsync(
            CancellationToken cancellationToken
        )
        {
            var result = await WriteCommandAsync("ListSquads", cancellationToken);

            return ListSquadsParser.Parse(Encoding.UTF8.GetString(result));
        }

        public async Task<ListPlayersResult> ListPlayersAsync(
            CancellationToken cancellationToken
        )
        {
            var result = await WriteCommandAsync("ListPlayers", cancellationToken);

            return ListPlayersParser.Parse(Encoding.UTF8.GetString(result));
        }

        public async Task<List<string>> ListLevelsAsync(
            CancellationToken cancellationToken
        )
        {
            var result = await WriteCommandAsync("ListLevels", cancellationToken);

            return ListLevelsParser.Parse(Encoding.UTF8.GetString(result));
        }

        public async Task<List<string>> ListLayersAsync(
            CancellationToken cancellationToken
        )
        {
            var result = await WriteCommandAsync("ListLayers", cancellationToken);

            return ListLayersParser.Parse(Encoding.UTF8.GetString(result));
        }

        public async Task AdminKickAsync(
            string playerName,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminKick \"{playerName}\"", cancellationToken);
        }

        public async Task AdminKickAsync(
            ulong steamId64,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminKick \"{steamId64}\"", cancellationToken);
        }

        public async Task AdminKickByIdAsync(
            int playerId,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminKickById {playerId}", cancellationToken);
        }

        public async Task AdminWarnAsync(
            string playerId,
            string reason,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminWarn \"{playerId}\" \"{reason}\"", cancellationToken);
        }

        public async Task AdminWarnAsync(
            ulong steamId64,
            string reason,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminWarn \"{steamId64}\" \"{reason}\"", cancellationToken);
        }

        public async Task AdminWarnByIdAsync(
            int playerId,
            string reason,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminWarnById {playerId} \"{reason}\"", cancellationToken);
        }
        
        // AdminSetMaxNumPlayers <NumPlayers>
        public async Task AdminSetMaxNumPlayersAsync(
            ushort maxPlayers,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminSetMaxNumPlayers {maxPlayers}", cancellationToken);
        }
        
        // AdminSetNumReservedSlots <AdminSetNumReservedSlots>
        public async Task AdminSetNumReservedSlotsAsync(
            ushort reservedSlots,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminSetNumReservedSlots {reservedSlots}", cancellationToken);
        }
            
        // AdminSetPublicQueueLimit <PublicQueueLimit>
        public async Task AdminSetPublicQueueLimitAsync(
            ushort publicQueueLimit,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminSetPublicQueueLimit {publicQueueLimit}", cancellationToken);
        }
        
        // AdminRemovePlayerFromSquadById <PlayerId>
        public async Task AdminRemovePlayerFromSquadByIdAsync(
            int playerId,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminRemovePlayerFromSquadById {playerId}", cancellationToken);
        }
        
        // AdminRemovePlayerFromSquad <PlayerNameOrSteamId64>
        public async Task AdminRemovePlayerFromSquadAsync(
            string playerName,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminRemovePlayerFromSquad \"{playerName}\"", cancellationToken);
        }

        public async Task AdminRemovePlayerFromSquadAsync(
            ulong steamId64,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminRemovePlayerFromSquad {steamId64}", cancellationToken);
        }

        // AdminDemoteCommander <PlayerNameOrSteamId64>
        public async Task AdminDemoteCommanderAsync(
            string playerName,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDemoteCommander \"{playerName}\"", cancellationToken);
        }

        public async Task AdminDemoteCommanderAsync(
            ulong steamId64,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDemoteCommander {steamId64}", cancellationToken);
        }
        
        // AdminDemoteCommanderById <PlayerId>
        public async Task AdminDemoteCommanderByIdAsync(
            int playerId,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDemoteCommanderById {playerId}", cancellationToken);
        }
        
        // AdminDisbandSquad <Team> <SquadIndex>
        public async Task AdminDisbandSquadAsync(
            Team team,
            int squadIndex,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDisbandSquad {(int)team} {squadIndex}", cancellationToken);
        }
        
        // AdminChangeLayer <LayerName>
        public async Task AdminChangeLayerAsync(
            string layer,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminChangeLayer {layer}", cancellationToken);
        }
        
        // AdminSetNextLayer <LayerName>
        public async Task AdminSetNextLayerAsync(
            string layer,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminSetNextLayer {layer}", cancellationToken);
        }
        
        // AdminBroadcast "<Message>"
        public async Task AdminBroadcastAsync(
            string message,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminBroadcast \"{message}\"", cancellationToken);
        }

        // ChatToAdmin "<Message>"
        public async Task ChatToAdminAsync(
            string message,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"ChatToAdmin \"{message}\"", cancellationToken);
        }
        
        // AdminSetFogOfWar <0|1>
        public async Task AdminSetFogOfWarAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminSetFogOfWar {(isEnabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminForceAllVehicleAvailability <0|1>
        public async Task AdminForceAllVehicleAvailabilityAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminForceAllVehicleAvailability {(isEnabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminForceAllDeployableAvailability <0|1>
        public async Task AdminForceAllDeployableAvailabilityAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminForceAllDeployableAvailability {(isEnabled ? 1 : 0)}", cancellationToken);
        }

        // AdminForceAllRoleAvailability <0|1>
        public async Task AdminForceAllRoleAvailabilityAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminForceAllRoleAvailability {(isEnabled ? 1 : 0)}", cancellationToken);
        }

        // AdminForceAllActionAvailability <0|1>
        public async Task AdminForceAllActionAvailabilityAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminForceAllActionAvailability {(isEnabled ? 1 : 0)}", cancellationToken);
        }

        // AdminNoTeamChangeTimer <0|1>
        public async Task AdminNoTeamChangeTimerAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminNoTeamChangeTimer {(isEnabled ? 1 : 0)}", cancellationToken);
        }

        // AdminDisableVehicleClaiming <0|1>
        public async Task AdminDisableVehicleClaimingAsync(
            bool isDisabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDisableVehicleClaiming {(isDisabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminDisableVehicleTeamRequirement <0|1>
        public async Task AdminDisableVehicleTeamRequirementAsync(
            bool isDisabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDisableVehicleTeamRequirement {(isDisabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminDisableVehicleKitRequirement <0|1>
        public async Task AdminDisableVehicleKitRequirementAsync(
            bool isDisabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminDisableVehicleKitRequirement {(isDisabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminAlwaysValidPlacement <0|1>
        public async Task AdminAlwaysValidPlacementAsync(
            bool isEnabled,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminAlwaysValidPlacement {(isEnabled ? 1 : 0)}", cancellationToken);
        }
        
        // AdminAddCameraman <PlayerNameOrSteamId64>
        public async Task AdminAddCameramanAsync(
            string playerName,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminAddCameraman \"{playerName}\"", cancellationToken);
        }
        
        public async Task AdminAddCameramanAsync(
            ulong steamId64,
            CancellationToken cancellationToken
        )
        {
            await WriteCommandAsync($"AdminAddCameraman {steamId64}", cancellationToken);
        }

        #endregion

        #region Invocations 
        
        protected virtual void OnChatMessageReceived(
            ChatMessage chatMessage
        )
        {
            ChatMessageReceived?.Invoke(chatMessage);
        }

        protected virtual void OnSquadCreatedMessageReceived(
            SquadCreatedMessage squadCreatedMessage
        )
        {
            SquadCreatedMessageReceived?.Invoke(squadCreatedMessage);
        }

        protected virtual void OnPacketReceived(
            Packet packet
        )
        {
            PacketReceived?.Invoke(packet);
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke();
        }

        protected virtual void OnBytesReceived(
            byte[] bytes
        )
        {
            BytesReceived?.Invoke(bytes);
        }

        protected virtual void OnPingReceived()
        {
            PingReceived?.Invoke();
        }
        
        #endregion
    }
}
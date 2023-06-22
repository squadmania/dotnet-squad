using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        public ListCommandsParser ListCommandsParser { get; set; } = new ListCommandsParser();
        public ListSquadsParser ListSquadsParser { get; set; } = new ListSquadsParser();
        public ListLayersParser ListLayersParser { get; set; } = new ListLayersParser();
        public ListLevelsParser ListLevelsParser { get; set; } = new ListLevelsParser();
        public ListPlayersParser ListPlayersParser { get; set; } = new ListPlayersParser();

        private readonly ChatMessageParser _chatMessageParser = new();
        private readonly SquadCreatedMessageParser _squadCreatedMessageParser = new();

        public bool IsConnected => _socket is { Connected: true };
        private byte[] PreviousBuffer { get; set; } = Array.Empty<byte>();
        
        private readonly IPEndPoint _endPoint;
        private readonly string _password;
        
        private Socket? _socket;
        
        private int _packetIdCounter;

        private readonly ConcurrentDictionary<int, RconClientCommandResult> _rconClientCommands =
            new ConcurrentDictionary<int, RconClientCommandResult>();

        public event Action<ChatMessage>? ChatMessageReceived;
        public event Action<SquadCreatedMessage>? SquadCreatedMessageReceived;

        public RconClient(IPEndPoint endPoint, string password)
        {
            _endPoint = endPoint;
            _password = password;
        }

        public void Connect()
        {
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_endPoint);

            Authenticate();
            
            // start listening for first async packet
            SetupContextSwitchedPacket();
        }

        private void SetupContextSwitchedPacket()
        {
            var state = new PacketReadState(new byte[4]);
            BeginBufferOrReceive(state.Data, 4, ReadPacketSizeCallback, state);
        }

        private void ReadPacketSizeCallback(
            IAsyncResult asyncResult
        )
        {
            var state = (PacketReadState)asyncResult.AsyncState;
            var size = Packet.ParseSize(state.Data);

            PacketReadState dataState;
            
            if (size == 10 && _socket!.Available + PreviousBuffer.Length >= 17)
            {
                dataState = new PacketReadState(new byte[17]);
                BeginBufferOrReceive(dataState.Data, 17, ReadBrokenPacketDataCallback, dataState);
                
                return;
            }

            dataState = new PacketReadState(new byte[size]);
            BeginBufferOrReceive(dataState.Data, size, ReadPacketDataCallback, dataState);
        }

        private void ReadPacketDataCallback(
            IAsyncResult asyncResult
        )
        {
            var state = (PacketReadState)asyncResult.AsyncState;

            var packet = Packet.Parse(state.Data);
            
            ProcessPacket(packet);
            
            // continue listening for async packet
            SetupContextSwitchedPacket();
        }

        private void ReadBrokenPacketDataCallback(
            IAsyncResult asyncResult
        )
        {
            var state = (PacketReadState)asyncResult.AsyncState;
            
            var packet = Packet.Parse(state.Data);
            if (packet.IsBroken)
            {
                // continue listening for async packet
                SetupContextSwitchedPacket();
                return;
            }

            packet = Packet.Parse(state.Data[..10]);

            var newPreviousBuffer = new byte[7 + PreviousBuffer.Length];
            state.Data[10..].CopyTo(newPreviousBuffer, 0);
            PreviousBuffer.CopyTo(newPreviousBuffer, 7);
            PreviousBuffer = newPreviousBuffer;
            
            ProcessPacket(packet);
            
            // continue listening for async packet
            SetupContextSwitchedPacket();
        }

        private void Authenticate()
        {
            var authPacketId = GetNextPacketId();
            var requestPacket = new Packet(
                authPacketId,
                PacketType.ServerDataAuth,
                _password
            );
            Write(requestPacket);

            Read();
            var response2 = Read();

            if (response2.Id == -1)
            {
                throw new Exception("invalid authentication password");
            }
        }

        public void Disconnect()
        {
            _socket?.Close();
            _socket?.Dispose();
            _socket = null;

            foreach (var result in _rconClientCommands)
            {
                result.Value.Cancel();
            }
            
            _rconClientCommands.Clear();

            _packetIdCounter = 0;
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

            _rconClientCommands[packetId] = commandResult;
            WritePackets(commandPacket, emptyPacket);
            
            var packets = await commandResult.Result;

            return packets.SelectMany(x => x.Body).ToArray();
        }

        protected void WritePackets(
            params Packet[] packets
        )
        {
            var buffer = packets.SelectMany(x => x.ToArray()).ToArray();

            _socket?.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, WritePacketsCallback, null);
        }

        private void WritePacketsCallback(
            IAsyncResult asyncResult
        )
        {
            _socket?.EndSend(asyncResult);
        }

        protected int GetNextPacketId()
        {
            return Interlocked.Increment(ref _packetIdCounter);
        }
        
        protected virtual void ProcessPacket(Packet packet)
        {
            if (packet.Type == PacketType.ServerDataChatMessage)
            {
                Task.Run(
                    () =>
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
                    }
                );
                return;
            }
            
            if (!_rconClientCommands.TryGetValue(packet.Id, out var command))
            {
                return;
            }

            command.AddPacket(packet);

            if (!(packet is
                {
                    Type: PacketType.ServerDataResponseValue,
                    Body: { Length: 0 }
                }))
            {
                return;
            }

            Task.Run(() => command.Complete());
            _rconClientCommands.TryRemove(packet.Id, out _);
        }

        private void BeginBufferOrReceive(
            byte[] data,
            int size,
            AsyncCallback callback,
            object state
        )
        {
            var previousBufferLength = PreviousBuffer.Length;
            if (previousBufferLength == 0)
            {
                _socket?.BeginReceive(data, 0, size, SocketFlags.None, callback, state);
                return;
            }
            
            if (previousBufferLength >= size)
            {
                // all requested bytes can be fetched from the buffer
                PreviousBuffer[..4].CopyTo(data, 0);
                PreviousBuffer = PreviousBuffer[4..];
                
                callback.Invoke(new CompletedAsyncResult(state));
                return;
            }
            
            // we need to also fetch bytes from the network stream
            PreviousBuffer.CopyTo(data, 0);
            PreviousBuffer = Array.Empty<byte>();
            
            var remainingCount = size - previousBufferLength;

            _socket?.BeginReceive(data, previousBufferLength, remainingCount, SocketFlags.None, callback, state);
        }

        private Packet Read()
        {
            var sizeBytes = new byte[4];
            var bytesRead = BufferOrReceive(sizeBytes, 4);
            if (bytesRead != 4)
            {
                throw new Exception("unexpected amount of bytes received");
            }

            var size = Packet.ParseSize(sizeBytes);
            byte[] dataBytes;

            if (size == 10 && _socket!.Available + PreviousBuffer.Length >= 17)
            {
                dataBytes = new byte[17];
                bytesRead = BufferOrReceive(dataBytes, 17);
                if (bytesRead != 17)
                {
                    throw new Exception("unexpected amount of bytes received");
                }

                var packet = Packet.Parse(dataBytes);
                if (packet.IsBroken)
                {
                    return packet;
                }

                packet = Packet.Parse(dataBytes[..10]);

                var newPreviousBuffer = new byte[7 + PreviousBuffer.Length];
                dataBytes[10..].CopyTo(newPreviousBuffer, 0);
                PreviousBuffer.CopyTo(newPreviousBuffer, 7);
                PreviousBuffer = newPreviousBuffer;
                
                return packet;
            }

            dataBytes = new byte[size];
            bytesRead = BufferOrReceive(dataBytes, size);
            if (bytesRead != size)
            {
                throw new Exception("unexpected amount of bytes received");
            }

            return Packet.Parse(dataBytes);
        }

        private int BufferOrReceive(
            byte[] data,
            int size
        )
        {
            var previousBufferLength = PreviousBuffer.Length;
            if (previousBufferLength == 0)
            {
                return _socket!.Receive(data, 0, size, SocketFlags.None);
            }

            if (previousBufferLength >= size)
            {
                PreviousBuffer[..size].CopyTo(data, 0);
                PreviousBuffer = PreviousBuffer[size..];

                return size;
            }
            
            PreviousBuffer.CopyTo(data, 0);
            PreviousBuffer = Array.Empty<byte>();

            var remainingCount = size - previousBufferLength;

            var bytesReceived = _socket!.Receive(data, previousBufferLength, remainingCount, SocketFlags.None);
            return previousBufferLength + bytesReceived;
        }
        

        private void Write(
            Packet packet
        )
        {
            _socket?.Send(packet.ToArray());
        }

        private void Write(
            params Packet[] packets
        )
        {
            _socket?.Send(
                packets
                    .SelectMany(x => x.ToArray())
                    .ToArray()
            );
        }

        public void Dispose()
        {
            Disconnect();
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
    }
}
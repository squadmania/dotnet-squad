using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squadmania.Squad.Rcon
{
    public class RconClientCommandResult
    {
        private readonly TaskCompletionSource<IReadOnlyList<Packet>> _taskCompletionSource;
        public Task<IReadOnlyList<Packet>> Result => _taskCompletionSource.Task;

        private readonly List<Packet> _packets = new List<Packet>();
        public IReadOnlyList<Packet> Packets => _packets;
        public readonly Packet[] RequestPackets;

        public RconClientCommandResult(Packet[] requestPackets)
        {
            RequestPackets = requestPackets;
            _taskCompletionSource =
                new TaskCompletionSource<IReadOnlyList<Packet>>();
        }

        public void AddPacket(
            Packet packet
        )
        {
            _packets.Add(packet);
        }

        public void ClearPackets()
        {
            _packets.Clear();
        }

        public void Cancel()
        {
            _taskCompletionSource.SetCanceled();
        }

        public void Complete()
        {
            try
            {
                _taskCompletionSource.SetResult(_packets);
            }
            catch
            {
                Console.WriteLine("some error occurred");
            }
        }
    }
}
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

        public RconClientCommandResult()
        {
            _taskCompletionSource =
                new TaskCompletionSource<IReadOnlyList<Packet>>();
        }

        public void AddPacket(
            Packet packet
        )
        {
            _packets.Add(packet);
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
            catch (Exception e)
            {
                Console.WriteLine("some error occurred");
            }
        }
    }
}
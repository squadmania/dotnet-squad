using System.Net.Sockets;
using System.Threading;

namespace Squadmania.Squad.Rcon
{
    public class RconClientThreadState
    {
        public Socket Socket { get; }
        public CancellationToken CancellationToken { get; }
        
        public RconClientThreadState(
            Socket socket,
            CancellationToken cancellationToken
        )
        {
            Socket = socket;
            CancellationToken = cancellationToken;
        }
    }
}
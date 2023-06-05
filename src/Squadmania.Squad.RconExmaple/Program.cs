using System.Net;
using System.Text;
using Squadmania.Squad.Rcon;

namespace Squadmania.Squad.RconExmaple
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var rconClient = new RconClient(
                new IPEndPoint(
                    IPAddress.Parse("127.0.0.1"),
                    21114
                ),
                "1234567"
            );

            rconClient.Connect();

            var commands = await rconClient.ListCommandsAsync(CancellationToken.None);

            for (var i = 0; i < 1000; i++)
            {
                var j = i;
                Task.Run(
                    async () =>
                    {
                        var commands = await rconClient.ListCommandsAsync(CancellationToken.None);
                        
                        Console.WriteLine($"Received {j}");
                    }
                ).ConfigureAwait(false);
            }

            Console.ReadKey();
        }
    }
}
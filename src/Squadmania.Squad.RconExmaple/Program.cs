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

            while (true)
            {
                var line = Console.ReadLine();

                var result = await rconClient.WriteCommandAsync(line, CancellationToken.None);
                
                Console.WriteLine(Encoding.UTF8.GetString(result));
            }

            Console.ReadKey();
        }
    }
}
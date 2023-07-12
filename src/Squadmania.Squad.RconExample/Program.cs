using System.Net;
using System.Text;
using Squadmania.Squad.Rcon;
using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.RconExample
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using var rconClient = new RconClient(
                new IPEndPoint(
                    IPAddress.Parse("127.0.0.1"),
                    21114
                ),
                "1234567"
            );

            rconClient.Start();

            var manualResetEvent = new ManualResetEvent(false);
            rconClient.Connected += () => manualResetEvent.Set();
            rconClient.ExceptionThrown += e => Console.WriteLine(e.Message);

            var currentMap = await rconClient.ShowCurrentMapAsync(default);
            var nextMap = await rconClient.ShowNextMapAsync(default);
            
            Console.WriteLine($"current: {currentMap.LayerName}, next: {nextMap.LayerName}");

            manualResetEvent.WaitOne();

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine() ?? "";

                if (!string.IsNullOrEmpty(line))
                {
                    var result = await rconClient.WriteCommandAsync(line, CancellationToken.None);

                    Console.WriteLine(Encoding.UTF8.GetString(result));
                }
            }

            Console.ReadKey();
        }

        private static void SquadCreatedMessageReceivedHandler(
            SquadCreatedMessage squadCreatedMessage
        )
        {
            Console.WriteLine($"Squad created message received: {squadCreatedMessage.SquadName}, {squadCreatedMessage.PlayerSteamId64}");
        }

        private static void ChatMessageReceivedHandler(
            ChatMessage chatMessage
        )
        {
            Console.WriteLine($"Normal message received: {chatMessage.Message}");
        }
    }
}
using System.Net;
using System.Text;
using Squadmania.Squad.Rcon;
using Squadmania.Squad.Rcon.Parsers;

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
            
            rconClient.ChatMessageReceived += ChatMessageReceivedHandler;
            rconClient.SquadCreatedMessageReceived += SquadCreatedMessageReceivedHandler;

            while (true)
            {
                var line = Console.ReadLine();

                var result = await rconClient.WriteCommandAsync(line, CancellationToken.None);
                
                Console.WriteLine(Encoding.UTF8.GetString(result));
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
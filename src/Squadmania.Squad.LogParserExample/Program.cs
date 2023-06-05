
using Squadmania.Squad.LogParser;

namespace Squadmania.Squad.LogParserExample;

public static class Program
{
    public static async Task Main(
        string[] args
    )
    {
        var fileLineReader = new FileLineReader(@"C:\Users\agphe\Documents\SquadTestServer\server\SquadGame\Saved\Logs\SquadGame.log");

        while (true)
        {
            var line = fileLineReader.ReadLine();
            Console.WriteLine(line);
        }
    }
}

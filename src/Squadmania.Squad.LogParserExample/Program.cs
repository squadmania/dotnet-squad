
using Squadmania.Squad.LogParser;
using Squadmania.Squad.LogParser.Parsers;

namespace Squadmania.Squad.LogParserExample;

public static class Program
{
    public static async Task Main(
        string[] args
    )
    {
        var fileLineReader = new FileLineReader(@"test\SquadGame.log");

        var logMessageParser = new LogMessageParser();
        
        while (true)
        {
            var line = fileLineReader.ReadLine();
            var logMessage = logMessageParser.Parse(line);
            
            if (logMessage is { Payload: not null })
            {
                Console.WriteLine(line);
            }
        }
    }
}

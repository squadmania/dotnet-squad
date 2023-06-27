
using Squadmania.Squad.LogParser;
using Squadmania.Squad.LogParser.Parsers;

namespace Squadmania.Squad.LogParserExample;

public static class Program
{
    public static async Task Main(
        string[] args
    )
    {
        var continuousFileLineReader = new ContinuousFileLineReader(@"test\SquadGame.log");
        var logMessageParser = new LogMessageParser();
        var logMessageReader = new LogMessageReader(continuousFileLineReader, logMessageParser);

        await foreach (var logMessage in logMessageReader)
        {
            if (logMessage.Payload == null)
            {
                continue;
            }
            
            Console.WriteLine(logMessage.Raw);
        }
    }
}

using System.CommandLine;

namespace Squadmania.Squad.Rcon.Cli;

public static class Program
{
    public static async Task<int> Main(
        string[] args
    )
    {
        var rootCommand = new RootCommand();

        return await rootCommand.InvokeAsync(args);
    }
}
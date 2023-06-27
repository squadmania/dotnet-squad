using System.CommandLine;

namespace Squadmania.Squad.Rcon.Cli;

public class RootCommand : System.CommandLine.RootCommand
{
    public static Option<bool> InteractiveOption = new (new string[]{"-i", "--interactive"}, () => true);
    public static Option<string> IpEndpointOption = new(new string[]{"-a", "--endpoint"}, () => "127.0.0.1:21114");
    public static Option<string> PasswordOption = new(new string[]{"-p", "--password"}, () => "123456");

    public RootCommand() : base("Opens the Squad rcon.")
    {
        AddOption(InteractiveOption);
        AddOption(IpEndpointOption);
        AddOption(PasswordOption);

        Handler = new RootCommandHandler();
    }
}
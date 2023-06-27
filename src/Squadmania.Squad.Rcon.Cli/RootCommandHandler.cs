using System.CommandLine.Invocation;
using System.Net;
using System.Text;

namespace Squadmania.Squad.Rcon.Cli;

public class RootCommandHandler : ICommandHandler
{
    public int Invoke(
        InvocationContext context
    )
    {
        return InvokeAsync(context).Result;
    }

    public async Task<int> InvokeAsync(
        InvocationContext context
    )
    {
        var endpointStr = context.ParseResult.GetValueForOption(RootCommand.IpEndpointOption) ?? "";
        var endpoint = IPEndPoint.Parse(endpointStr);
        var password = context.ParseResult.GetValueForOption(RootCommand.PasswordOption) ?? "";
        
        var interactive = context.ParseResult.GetValueForOption(RootCommand.InteractiveOption);
        
        using var rconClient = new RconClient(
            endpoint,
            password
        );
        rconClient.Start();
        
        if (interactive)
        {
            var manualResetEvent = new ManualResetEvent(false);
            rconClient.Connected += () => manualResetEvent.Set();

            manualResetEvent.WaitOne();
            
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                var result = await rconClient.WriteCommandAsync(line ?? "", CancellationToken.None);
                
                Console.WriteLine(Encoding.UTF8.GetString(result));
            }
        }

        return 0;
    }
}
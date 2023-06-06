using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ListCommandsParser : ICommandParser<List<Command>>
    {
        private static readonly Regex CommandRegex = new Regex("^([A-Za-z]+)\\s+(.*)\\s+(\\([A-Za-z\\s\\.\\-]+\\))$");
        
        public List<Command> Parse(string input)
        {
            var allLines = input
                .Replace("\r\n", "\n")
                .Split("\n");
            if (allLines.Length <= 1)
            {
                return new List<Command>();
            }

            var commands = new List<Command>();
            
            foreach (var line in allLines[new Range(1, allLines.Length - 1)])
            {
                var match = CommandRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                var name = match.Groups[1].Value;
                var parameterDescription = match.Groups[2].Value;
                var description = match.Groups[3]
                    .Value
                    .TrimStart('(')
                    .TrimEnd(')');
                
                commands.Add(new Command(name, parameterDescription, description));
            }

            return commands;
        }
    }
}
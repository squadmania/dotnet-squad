using System.Collections.Generic;
using System.Linq;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ListLayersParser : ICommandParser<List<string>>
    {
        private const string Header = "List of available layers :";
        
        public List<string> Parse(
            string input
        )
        {
            input = input.Replace(Header, "");
            
            return input.Split("\n").ToList();
        }
    }
}
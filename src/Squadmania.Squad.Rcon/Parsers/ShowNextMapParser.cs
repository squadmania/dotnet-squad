using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ShowNextMapParser : ICommandParser<NextMap?>
    {
        private static readonly Regex NextMapRegex = new ("^Next level is (.*), layer is (.*)$");
        
        public NextMap? Parse(
            string input
        )
        {
            var match = NextMapRegex.Match(input);
            if (!match.Success)
            {
                return null;
            }

            var levelName = match.Groups[1].Value;
            var layerName = match.Groups[2].Value;

            return new NextMap(
                levelName,
                layerName
            );
        }
    }
}
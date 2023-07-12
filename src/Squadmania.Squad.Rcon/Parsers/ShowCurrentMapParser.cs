using System.Text.RegularExpressions;
using Squadmania.Squad.Rcon.Models;

namespace Squadmania.Squad.Rcon.Parsers
{
    public class ShowCurrentMapParser : ICommandParser<CurrentMap?>
    {
        private static readonly Regex CurrentMapRegex = new ("^Current level is (.*), layer is (.*)$");

        public CurrentMap? Parse(
            string input
        )
        {
            var match = CurrentMapRegex.Match(input);
            if (!match.Success)
            {
                return null;
            }

            var levelName = match.Groups[1].Value;
            var layerName = match.Groups[2].Value;

            return new CurrentMap(
                levelName,
                layerName
            );
        }
    }
}
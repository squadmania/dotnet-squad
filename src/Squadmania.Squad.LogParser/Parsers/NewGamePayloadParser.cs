using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class NewGamePayloadParser : IPayloadParser<NewGamePayload>
    {
        private static readonly Regex PayloadData = new(@"Bringing World \/([A-z]+)\/(?:Maps\/)?([A-z0-9-]+)\/(?:.+\/)?([A-z0-9-]+)(?:\.[A-z0-9-]+)");

        public LogMessageType LogMessageType => LogMessageType.World;

        public NewGamePayload? ParseTyped(
            string value
        )
        {
            var match = PayloadData.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new NewGamePayload(
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value
            );
        }

        public object? Parse(
            string value
        )
        {
            return ParseTyped(value);
        }
    }
}
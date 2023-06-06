using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class ServerTickRatePayloadParser : IPayloadParser<ServerTickRatePayload>
    {
        private static readonly Regex PayloadRegex = new(@"USQGameState: Server Tick Rate: ([0-9.]+)");
        
        public LogMessageType LogMessageType => LogMessageType.Squad;

        public ServerTickRatePayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new ServerTickRatePayload(
                float.Parse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture)
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
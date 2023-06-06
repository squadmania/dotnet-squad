using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class RoundEndedPayloadParser : IPayloadParser<RoundEndedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"Match State Changed from InProgress to WaitingPostMatch");
        
        public LogMessageType LogMessageType => LogMessageType.GameState;

        public RoundEndedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new RoundEndedPayload();
        }

        public object? Parse(
            string value
        )
        {
            return ParseTyped(value);
        }
    }
}
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerControllerConnectedPayloadParser : IPayloadParser<PlayerControllerConnectedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"PostLogin: NewPlayer: BP_PlayerController_C .+(BP_PlayerController_C_[0-9]+)");
        
        public LogMessageType LogMessageType => LogMessageType.Squad;

        public PlayerControllerConnectedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerControllerConnectedPayload(
                match.Groups[1].Value
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
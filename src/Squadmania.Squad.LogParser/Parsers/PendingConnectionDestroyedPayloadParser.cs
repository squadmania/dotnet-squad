using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PendingConnectionDestroyedPayloadParser : IPayloadParser<PendingConnectionDestroyedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"UNetConnection::PendingConnectionLost\. \[UNetConnection\] RemoteAddr: ([0-9]{17}):[0-9]+, Name: (SteamNetConnection_[0-9]+), Driver: GameNetDriver (SteamNetDriver_[0-9]+), IsServer: YES, PC: NULL, Owner: NULL, UniqueId: (?:Steam:UNKNOWN \[.+\]|INVALID) bPendingDestroy=0");
        
        public LogMessageType LogMessageType => LogMessageType.Net;

        public PendingConnectionDestroyedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PendingConnectionDestroyedPayload(
                ulong.Parse(match.Groups[1].Value),
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
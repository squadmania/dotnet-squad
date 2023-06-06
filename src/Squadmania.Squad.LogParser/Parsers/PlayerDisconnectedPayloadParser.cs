using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class PlayerDisconnectedPayloadParser : IPayloadParser<PlayerDisconnectedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"UNetConnection::Close: \[UNetConnection\] RemoteAddr: ([0-9]{17}):[0-9]+, Name: SteamNetConnection_[0-9]+, Driver: GameNetDriver SteamNetDriver_[0-9]+, IsServer: YES, PC: (BP_PlayerController_C_[0-9]+), Owner: BP_PlayerController_C_[0-9]+, UniqueId: Steam:UNKNOWN \[.*\], Channels: [0-9]+, Time: [0-9.:-]+");
        
        public LogMessageType LogMessageType => LogMessageType.Net;

        public PlayerDisconnectedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new PlayerDisconnectedPayload(
                ulong.Parse(match.Groups[1].Value),
                match.Groups[2].Value
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
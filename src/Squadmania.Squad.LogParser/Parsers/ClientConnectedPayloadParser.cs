using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public sealed class ClientConnectedPayloadParser : IPayloadParser<ClientConnectedPayload>
    {
        private static readonly Regex PayloadRegex = new(@"AddClientConnection: Added client connection: \[UNetConnection\] RemoteAddr: ([0-9]{17}):[0-9]+, Name: (SteamNetConnection_[0-9]+), Driver: GameNetDriver (SteamNetDriver_[0-9]+) (SteamNetDriver_[0-9]+), IsServer: YES, PC: NULL, Owner: NULL, UniqueId: INVALID");

        public LogMessageType LogMessageType => LogMessageType.Net;

        public ClientConnectedPayload? ParseTyped(
            string value
        )
        {
            var match = PayloadRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            return new ClientConnectedPayload(
                ulong.Parse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture),
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
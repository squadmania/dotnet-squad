using System.Text.RegularExpressions;

namespace Squadmania.Squad.Rcon.Parsers
{
    public sealed class ChatMessageParser
    {
        private static readonly Regex ChatMessageRegex = new (
            @"^\[(ChatSquad|ChatAdmin|ChatTeam|ChatAll)\] \[SteamID:([0-9]+)\] (.*) : (.*)$"
        );
    
        private static bool TryParseChannel(
            string value,
            out ChatMessageChannel channel
        )
        {
            switch (value)
            {
                case "ChatSquad":
                    channel = ChatMessageChannel.Squad;
                    break;
                case "ChatAdmin":
                    channel = ChatMessageChannel.Admin;
                    break;
                case "ChatTeam":
                    channel = ChatMessageChannel.Team;
                    break;
                case "ChatAll":
                    channel = ChatMessageChannel.All;
                    break;
                default:
                    channel = ChatMessageChannel.All;
                    return false;
            }
        
            return true;
        }

        public ChatMessage? Parse(
            string value
        )
        {
            var match = ChatMessageRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            if (!TryParseChannel(match.Groups[1].Value, out var channel))
            {
                return null;
            }

            if (!long.TryParse(match.Groups[2].Value, out var playerSteamId64))
            {
                return null;
            }

            var playerName = match.Groups[3].Value;
            var message = match.Groups[4].Value;

            return new ChatMessage(
                channel,
                playerSteamId64,
                playerName,
                message
            );
        }
    }
}
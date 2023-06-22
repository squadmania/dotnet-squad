namespace Squadmania.Squad.Rcon.Parsers
{
    public readonly struct ChatMessage
    {
        public ChatMessageChannel Channel { get; }
        public long PlayerSteamId64 { get; }
        public string PlayerName { get; }
        public string Message { get; }

        public ChatMessage(
            ChatMessageChannel channel,
            long playerSteamId64,
            string playerName,
            string message
        )
        {
            Channel = channel;
            PlayerSteamId64 = playerSteamId64;
            PlayerName = playerName;
            Message = message;
        }
    }
}
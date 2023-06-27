namespace Squadmania.Squad.Rcon.Models
{
    public readonly struct ChatMessage
    {
        public ChatMessageChannel Channel { get; }
        public ulong PlayerSteamId64 { get; }
        public string PlayerName { get; }
        public string Message { get; }

        public ChatMessage(
            ChatMessageChannel channel,
            ulong playerSteamId64,
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
namespace Squadmania.Squad.Rcon
{
    public readonly struct PacketReadState
    {
        public PacketReadState(
            byte[] data
        )
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}
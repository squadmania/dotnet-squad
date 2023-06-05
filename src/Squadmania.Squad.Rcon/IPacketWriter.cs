namespace Squadmania.Squad.Rcon
{
    public interface IPacketWriter
    {
        public void Write(
            Packet packet
        );
    }
}
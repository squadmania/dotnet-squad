namespace Squadmania.Squad.Rcon.Parsers
{
    public interface ICommandParser<out T>
    {
        public T Parse(string input);
    }
}
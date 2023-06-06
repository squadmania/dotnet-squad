using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public interface IPayloadParser
    {
        public LogMessageType LogMessageType { get; }
        
        public object? Parse(
            string value
        );
    }
    
    public interface IPayloadParser<out T> : IPayloadParser
    {
        public T? ParseTyped(
            string value
        );
    }
}
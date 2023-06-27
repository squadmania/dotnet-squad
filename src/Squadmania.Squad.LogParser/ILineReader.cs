using System.Collections.Generic;

namespace Squadmania.Squad.LogParser
{
    public interface ILineReader : IAsyncEnumerable<string>
    {
    }
}
using System;

namespace Squadmania.Squad.LogParser
{
    public class InputLineReader : ILineReader
    {
        public virtual string ReadLine()
        {
            return Console.In.ReadLine();
        }
    }
}
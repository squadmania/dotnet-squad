using System;
using System.Threading;

namespace Squadmania.Squad.Rcon
{
    internal struct CompletedAsyncResult : IAsyncResult
    {
        public CompletedAsyncResult(
            object asyncState
        )
        {
            AsyncState = asyncState;
        }

        public object AsyncState { get; }
        public WaitHandle AsyncWaitHandle => new Mutex();
        public bool CompletedSynchronously => true;
        public bool IsCompleted => true;
    }
}
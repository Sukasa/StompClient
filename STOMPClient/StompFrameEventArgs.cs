using System;

namespace StompClient
{
    public class StompFrameEventArgs : EventArgs
    {
        
        private StompFrame _Frame;

        public StompFrame Frame { get { return _Frame; } }

        internal StompFrameEventArgs(StompFrame Frame)
        {
            _Frame = Frame;
        }
    }
}

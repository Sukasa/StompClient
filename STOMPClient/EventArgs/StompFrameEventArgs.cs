using System;
using STOMP.Frames;

namespace STOMP
{
    /// <summary>
    ///     Base event args class for Stomp frame events
    /// </summary>
    public class StompFrameEventArgs : EventArgs
    {
        
        private StompFrame _Frame;

        /// <summary>
        ///     The Stomp frame for this event
        /// </summary>
        public StompFrame Frame { get { return _Frame; } }

        internal StompFrameEventArgs(StompFrame Frame)
        {
            _Frame = Frame;
        }
    }
}

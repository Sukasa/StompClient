namespace StompClient
{
    /// <summary>
    ///     EventArgs class for stomp server errors
    /// </summary>
    public class StompErrorEventArgs : StompFrameEventArgs
    {
        private string _ErrorMessage;

        /// <summary>
        ///     The simple error message returned from the server
        /// </summary>
        public string ErrorMessage { get { return _ErrorMessage;  } }

        internal StompErrorEventArgs(StompErrorFrame Frame)
            : base(Frame)
        {
            _ErrorMessage = Frame._errorMessage;
        }
    }
}

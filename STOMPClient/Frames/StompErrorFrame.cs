namespace StompClient
{
    /// <summary>
    ///     The Error frame sent from the server when there is an error in the protocol
    /// </summary>
    [StompFrameType("ERROR", StompFrameDirection.ServerToClient)]
    public class StompErrorFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("message")]
        internal string _errorMessage = null;

        /// <summary>
        ///     The error message given by the server
        /// </summary>
        public string ErrorMessage { get { return _errorMessage; } }

        internal StompErrorFrame()
        {

        }
    }
}

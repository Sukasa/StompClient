namespace StompClient
{
    /// <summary>
    ///     The Disconnect frame sent from the client to gracefully terminate a connection
    /// </summary>
    /// <remarks>
    ///     If a Receipt is requested, the server will attempt to send a Receipt frame in response to this Disconnect frame before closing the connection. 
    /// </remarks>
    [StompFrameType("DISCONNECT", StompFrameDirection.ClientToServer)]
    class StompDisconnectFrame : StompFrame
    {
        /// <summary>
        ///     Creates a disconnect frame
        /// </summary>
        public StompDisconnectFrame()
        {

        }
    }
}

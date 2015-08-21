using System;

namespace StompClient
{
    /// <summary>
    ///     Denotes the frame type and direction of a Stomp frame
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StompFrameType : Attribute
    {
        internal string _FrameType;
        internal StompFrameDirection _Direction;

        /// <summary>
        ///     Creates a StompFrameType attribute denoting Frame type and intended direction
        /// </summary>
        /// <param name="FrameType">
        ///     The type of frame to send, which should be in uppercase, e.g. SEND, BEGIN
        /// </param>
        /// <param name="Direction">
        ///     The direction the frame is meant to travel in.  Most frames should be ClientToServer or ServerToClient
        /// </param>
        public StompFrameType(string FrameType, StompFrameDirection Direction)
        {
            _FrameType = FrameType.ToUpper();
            _Direction = Direction;
        }
    }

    /// <summary>
    ///     The direction in which the frame is meant to travel
    /// </summary>
    public enum StompFrameDirection
    {
        /// <summary>
        ///     The frame travels exclusively from client to server
        /// </summary>
        ClientToServer,


        /// <summary>
        ///     The frame travels exclusively from server to client
        /// </summary>
        ServerToClient,

        /// <summary>
        ///     The frame is sent in both directions
        /// </summary>
        Bidirectional
    }

    /// <summary>
    ///     Identifies a Frame header that should be automatically serialized or deserialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class StompHeaderIdentifier : Attribute
    {
        internal string _HeaderIdentifier;
        internal bool _IsOptional;

        /// <summary>
        ///     Creates a new StompHeaderIdentifier attribute with the given parameters
        /// </summary>
        /// <param name="HeaderIdentifier">
        ///     The identifier of the header as should be transmitted on the wire.  Should be lowercase.
        /// </param>
        /// <param name="Optional">
        ///     TRUE if the header parameter is optional to include
        /// </param>
        public StompHeaderIdentifier(string HeaderIdentifier, bool Optional = false)
        {
            _HeaderIdentifier = HeaderIdentifier.ToLower();
            _IsOptional = Optional;
        }
    }
}

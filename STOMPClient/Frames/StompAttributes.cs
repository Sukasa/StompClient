using System;

namespace StompClient
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StompFrameType : Attribute
    {
        internal string _FrameType;
        internal StompFrameDirection _Direction;

        public StompFrameType(string FrameType, StompFrameDirection Direction)
        {
            _FrameType = FrameType;
            _Direction = Direction;
        }
    }

    public enum StompFrameDirection
    {
        ClientToServer,
        ServerToClient,
        Bidirectional
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class StompHeaderIdentifier : Attribute
    {
        internal string _HeaderIdentifier;
        internal bool _IsOptional;

        public StompHeaderIdentifier(string HeaderIdentifier, bool Optional = false)
        {
            _HeaderIdentifier = HeaderIdentifier;
            _IsOptional = Optional;
        }
    }
}

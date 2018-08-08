using System;
using System.Runtime.Serialization;

namespace WS_Common
{
    [Serializable]
    internal class WebSocketServerException : Exception
    {
        public WebSocketServerException()
        {
        }

        public WebSocketServerException(string message) : base(message)
        {
        }

        public WebSocketServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WebSocketServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
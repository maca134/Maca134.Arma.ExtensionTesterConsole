using System;
using System.Runtime.Serialization;

namespace Maca134.Arma.ExtensionTesterConsole
{
    [Serializable]
    internal class ArmaDllException : Exception
    {
        public ArmaDllException()
        {
        }

        public ArmaDllException(string message) : base(message)
        {
        }

        public ArmaDllException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ArmaDllException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
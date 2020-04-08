using System;
using System.Runtime.Serialization;

namespace HealthCheck.Framework
{
    [Serializable]
    public class DuplicateHealthCheckException : Exception
    {
        public DuplicateHealthCheckException()
        {
        }

        public DuplicateHealthCheckException(string message)
            : base(message)
        {
        }

        public DuplicateHealthCheckException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DuplicateHealthCheckException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

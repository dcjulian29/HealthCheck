using System;
using System.Runtime.Serialization;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   The exception that is thrown when one a duplicate health check is defined.
    /// </summary>
    [Serializable]
    public class DuplicateHealthCheckException : Exception
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="DuplicateHealthCheckException" /> class.
        /// </summary>
        public DuplicateHealthCheckException()
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DuplicateHealthCheckException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DuplicateHealthCheckException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DuplicateHealthCheckException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public DuplicateHealthCheckException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DuplicateHealthCheckException" /> class.
        /// </summary>
        /// <param name="info">
        ///   The <see
        ///   cref="T:System.Runtime.Serialization.SerializationInfo">SerializationInfo</see> that
        ///   holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///   The <see cref="T:System.Runtime.Serialization.StreamingContext">StreamingContext</see>
        ///   that contains contextual information about the source or destination.
        /// </param>
        protected DuplicateHealthCheckException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

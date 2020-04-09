using System;
using System.Runtime.Serialization;

namespace HealthCheck
{
    /// <summary>
    ///   The exception that is thrown when one a duplicate health check is defined.
    /// </summary>
    [Serializable]
    public class MissingRequiredSettingException : Exception
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="MissingRequiredSettingException" /> class.
        /// </summary>
        public MissingRequiredSettingException()
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MissingRequiredSettingException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MissingRequiredSettingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MissingRequiredSettingException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public MissingRequiredSettingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MissingRequiredSettingException" /> class.
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
        protected MissingRequiredSettingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

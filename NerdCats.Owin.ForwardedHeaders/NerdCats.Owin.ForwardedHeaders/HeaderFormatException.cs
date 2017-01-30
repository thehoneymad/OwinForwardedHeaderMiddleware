namespace NerdCats.Owin
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a format exception or protocol violation in RFC-7239 compliant forwarded headers
    /// </summary>
    public class HeaderFormatException : Exception
    {
        /// <summary>
        /// Instantiates a HeaderFormatException with a message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public HeaderFormatException(string message) : base(message)
        {
        }

        /// <summary>
        /// Instantiates a HeaderFormatException with a message and an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public HeaderFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="malformedSegment"></param>
        public HeaderFormatException(string headerName, string malformedSegment)
            : this($"Malformed {headerName} encountered, malformed segment = {malformedSegment}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the HeaderFormatException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The System.Runtime.Serialization.SerializationInfo that holds the serialized 
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.
        /// </param>
        protected HeaderFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
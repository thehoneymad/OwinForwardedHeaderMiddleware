namespace NerdCats.Owin
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception for symmetry errors in X-Forwarded-* standard headers.
    /// </summary>
    public class HeaderSymmetryException : Exception
    {
        /// <summary>
        /// Instantiates a HeaderSymmetryException with a message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public HeaderSymmetryException(string message) : base(message)
        {
        }

        /// <summary>
        /// Instantiates a HeaderSymmetryException with a message and an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public HeaderSymmetryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HeaderSymmetryException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The System.Runtime.Serialization.SerializationInfo that holds the serialized 
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.
        /// </param>
        protected HeaderSymmetryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
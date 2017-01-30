namespace NerdCats.Owin
{
    /// <summary>
    /// Standard definition for different types of forwarded headers implementation
    /// </summary>
    public enum ForwardHeaderStandard
    {
        /// <summary>
        /// Standard not specified
        /// </summary>
        Unspecified,
        /// <summary>
        /// RFC-7239 compliant forwarded headers
        /// </summary>
        Forwarded,
        /// <summary>
        /// X-Forwarded-* standard forwarded headers
        /// </summary>
        Xforwarded
    }
}
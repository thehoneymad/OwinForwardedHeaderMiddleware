namespace NerdCats.Owin
{
    /// <summary>
    /// Options to specify while use ForwardedHeadersHandler
    /// </summary>
    public struct ForwardedHeadersOptions
    {
        /// <summary>
        /// This will enable all specification guided errors in both of the 
        /// forwarded headers implementation in <see cref="ForwardedHeadersHandler"/>.
        /// Expect a <see cref="HeaderSymmetryException"/> in case of a specification violation
        /// </summary>
        public bool RequireHeaderSymmetry { get; set; }

        /// <summary>
        /// Limit the Forward headers used in <see cref="ForwardedHeadersHandler"/>
        /// </summary>
        public int? ForwardLimit { get; set; }

        /// <summary>
        /// Preferred forward headers standard while using <see cref="ForwardedHeadersHandler"/>
        /// </summary>
        public ForwardHeaderStandard PreferredStandard { get; set; }
    }
}
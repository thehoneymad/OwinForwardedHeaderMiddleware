namespace NerdCats.Owin
{
    using System.Collections.Generic;
    using Microsoft.Owin;

    /// <summary>
    /// Generic interface to define a http forwarded header wrapper
    /// </summary>
    public interface IForwardedHeaders
    {
        /// <summary>
        /// Forwarded port value defined X-Forwarded-Port header. Ignored in RFC-7239 implementation here
        /// </summary>
        int? ForwardedPort { get; }
        /// <summary>
        /// List of forwarder sets found in the header description
        /// </summary>
        List<ForwarderSet> Forwarders { get; set; }
        /// <summary>
        /// Apply changes to the current http request according to the forwarders found in the header
        /// </summary>
        /// <param name="request">Owin request context</param>
        /// <returns>Updated set of headers after applying the changes</returns>
        IHeaderDictionary ApplyForwardedHeaders(IOwinRequest request);
    }
}
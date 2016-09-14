namespace NerdCats.Owin
{
    using System.Collections.Generic;
    using Microsoft.Owin;

    public interface IForwardedHeaders
    {
        int? ForwardedPort { get; }
        List<ForwarderSet> Forwarders { get; set; }
        IHeaderDictionary ApplyForwardedHeaders(IOwinRequest request);
    }
}
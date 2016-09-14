namespace NerdCats.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Owin;

    public class XForwardedHeaders : IForwardedHeaders
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";
        private const string XForwardedHostHeaderName = "X-Forwarded-Host";
        private const string XForwardedProtoHeaderName = "X-Forwarded-Proto";
        private const string XForwardedSchemeHeaderName = "X-Forwarded-Scheme";
        private const string XForwardedPortHeaderName = "X-Forwarded-Port";

        private IList<string> ForwardedHost { get; set; }
        private IList<string> ForwardedProtocol { get; set; }
        private IList<string> ForwardedFor { get; set; }

        public XForwardedHeaders(IOwinRequest request, ForwardedHeadersOptions options)
        {
            ProcessForwardedHeaders(request, options);
        }

        public IHeaderDictionary ApplyForwardedHeaders(IOwinRequest request)
        {
            var firstForwarderSet = this.Forwarders?.FirstOrDefault();
            if (firstForwarderSet == null) return request.Headers;

            request.Scheme = firstForwarderSet.Value.Scheme;

            if (!string.IsNullOrWhiteSpace(firstForwarderSet.Value.Host))
                request.Host = new HostString(firstForwarderSet.Value.Host);

            var port = this.ForwardedPort;
            if (port != null)
            {
                request.RemotePort = port;

                var host = request.Host.Value;
                var pos = host.IndexOf(':');

                /* INFO: If the following two lines confuses anyone as it did confuse me
                 * the gist is the following: 
                 * If the forwarded port is defined in the headers and the forwarded host string
                 * also contains a port value attached to it, we are just making sure that the 
                 * attached port value is the same on mentioned in the forwarded port header.
                 * 
                 * If they are different, the forwarded port header is given precedence here. 
                 * 
                 * And yes, I know, it confused me too.
                 * */
                var hostString = pos == -1 ? host : host.Substring(0, pos);
                request.Host = new HostString($"{hostString}:{port}");
            }

            request.RemoteIpAddress = firstForwarderSet.Value.RemoteIpAndPort;
            return request.Headers;
        }

        private void ProcessForwardedHeaders(IOwinRequest request, ForwardedHeadersOptions options)
        {
            int entryCount = 0;

            ForwardedFor = request.Headers.GetCommaSeparatedValues(XForwardedForHeaderName);
            if (ForwardedFor != null) entryCount = Math.Max(ForwardedFor.Count(), entryCount);

            ForwardedProtocol = request.Headers.GetCommaSeparatedValues(XForwardedProtoHeaderName);
            if (ForwardedProtocol != null) entryCount = Math.Max(ForwardedProtocol.Count(), entryCount);

            var protocolKeyValue = GetProtocolHeaderString(request);
            ForwardedProtocol = protocolKeyValue.Value;

            if (options.RequireHeaderSymmetry && (ForwardedFor?.Count() != ForwardedProtocol?.Count()))
                throw new NotSupportedException($"Parameter count mismatch between {XForwardedForHeaderName} and {protocolKeyValue.Key} ");

            ForwardedHost = request.Headers.GetCommaSeparatedValues(XForwardedHostHeaderName);
            if (options.RequireHeaderSymmetry
                && (ForwardedFor?.Count() != ForwardedHost?.Count()
                       || ForwardedProtocol?.Count() != ForwardedHost?.Count()))
                throw new NotSupportedException($"Parameter count mismatch between {XForwardedHostHeaderName} and {protocolKeyValue.Key} or {XForwardedForHeaderName}");

            if (ForwardedHost != null)
                entryCount = Math.Max(ForwardedHost.Count(), entryCount);

            if (!string.IsNullOrWhiteSpace(request.Headers[XForwardedPortHeaderName]))
                ForwardedPort = int.Parse(request.Headers[XForwardedPortHeaderName]);

            if (options.ForwardLimit.HasValue && entryCount > options.ForwardLimit.Value)
            {
                entryCount = options.ForwardLimit.Value;
            }

            var forwarders = new ForwarderSet[entryCount];
            for (int count = 0; count < entryCount; count++)
            {
                // INFO: They get processed in reverse order, right to left.
                var set = new ForwarderSet();
                if (count < ForwardedFor?.Count)
                {
                    set.RemoteIpAndPort = ForwardedFor[ForwardedFor.Count() - count - 1];
                }
                if (count < ForwardedProtocol?.Count)
                {
                    set.Scheme = ForwardedProtocol[ForwardedProtocol.Count - count - 1];
                }
                if (count < ForwardedHost?.Count)
                {
                    set.Host = ForwardedHost[ForwardedHost.Count - count - 1];
                }
                forwarders[count] = set;
            }

            this.Forwarders = forwarders.ToList();
        }

        private static KeyValuePair<string, IList<string>> GetProtocolHeaderString(IOwinRequest request)
        {
            /* INFO: Numerous proxy/reverse-proxy servers uses X-Forwarded-Scheme insted of X-Forwarded Proto.
             * Making sure X-Forwarded-Proto is read before checking X-Forwarded-Scheme and only X-Forwarded Proto
             * is used as they both is used.
             */

            if (!string.IsNullOrWhiteSpace(request.Headers[XForwardedProtoHeaderName]))
            {
                return new KeyValuePair<string, IList<string>>(XForwardedProtoHeaderName, request.Headers.GetCommaSeparatedValues(XForwardedProtoHeaderName));
            }
            else if (!string.IsNullOrWhiteSpace(request.Headers[XForwardedSchemeHeaderName]))
            {
                return new KeyValuePair<string, IList<string>>(XForwardedSchemeHeaderName, request.Headers.GetCommaSeparatedValues(XForwardedSchemeHeaderName));
            }

            return default(KeyValuePair<string, IList<string>>);
        }

        public int? ForwardedPort { get; set; }
        public List<ForwarderSet> Forwarders { get; set; }
    }
}
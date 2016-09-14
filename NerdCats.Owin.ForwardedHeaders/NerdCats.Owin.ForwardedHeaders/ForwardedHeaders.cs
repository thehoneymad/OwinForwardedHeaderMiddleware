namespace NerdCats.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Owin;

    public class ForwardedHeaders : IForwardedHeaders
    {
        private const string ForwardedForHeaderName = "Forwarded";
        private const string ForParam = "for";
        private const string HostParam = "host";
        private const string ProtoParam = "proto";

        private IList<string> ForwardedFor;
        private ForwardedHeadersOptions options;

        public ForwardedHeaders(IOwinRequest request, ForwardedHeadersOptions options)
        {
            this.options = options;
            ProcessForwardedHeaders(request, options);
        }

        public int? ForwardedPort { get; }
        public List<ForwarderSet> Forwarders { get; set; }
        public IHeaderDictionary ApplyForwardedHeaders(IOwinRequest request)
        {
            var firstForwarderSet = this.Forwarders?.FirstOrDefault();
            if (firstForwarderSet == null) return request.Headers;

            request.Scheme = firstForwarderSet.Value.Scheme;

            if (!string.IsNullOrWhiteSpace(firstForwarderSet.Value.Host))
                request.Host = new HostString(firstForwarderSet.Value.Host.Trim('\"'));

            request.RemoteIpAddress = firstForwarderSet.Value.RemoteIpAndPort;
            return request.Headers;
        }
        private void ProcessForwardedHeaders(IOwinRequest request, ForwardedHeadersOptions options)
        {
            ForwardedFor = request.Headers.GetCommaSeparatedValues(ForwardedForHeaderName);
            if (ForwardedFor?.Count == 0) return;
            var forwarders = new List<ForwarderSet>();

            foreach (var fieldValue in ForwardedFor)
            {
                var pairs = fieldValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Reverse();

                foreach (var keyVal in pairs)
                {
                    var forwarder = new ForwarderSet();

                    var keyValSplits = keyVal.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValSplits.Length != 2)
                    {
                        if (options.RequireHeaderSymmetry)
                            throw new NotSupportedException($"Failed to parse {keyValSplits} into a valid parameter = value pair");
                        continue;
                    }

                    switch (keyValSplits.First().ToLower())
                    {
                        case ForParam:
                            forwarder.Host = keyValSplits.Last();
                            break;
                        case HostParam:
                            forwarder.Host = keyValSplits.Last();
                            break;
                        case ProtoParam:
                            forwarder.Scheme = keyValSplits.Last();
                            break;
                        default:
                            if (options.RequireHeaderSymmetry)
                                throw new NotSupportedException($"Invalid parameter {keyValSplits.First()} encountered");
                            break;
                    }

                    forwarders.Add(forwarder);
                }
            }

            this.Forwarders = forwarders;
        }

    }
}

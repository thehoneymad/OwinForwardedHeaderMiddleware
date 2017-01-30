namespace NerdCats.Owin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    /// <summary>
    /// Owin Middleware that can be used to handle requests forwarded by load balancers.
    /// Code originally taken from StoryHub
    /// Handles X-Forwarded-Proto, X-Forwarded-Port 
    /// </summary>
    public class ForwardedHeadersHandler : OwinMiddleware
    {
        private const string ForwardedHeaderName = "Forwarded";
        private const string XForwardedHeaderPrefix = "X-Forward";

        private readonly ForwardedHeadersOptions options;

        public ForwardedHeadersHandler(OwinMiddleware next, ForwardedHeadersOptions options = default(ForwardedHeadersOptions)) : base(next)
        {
            this.options = options;
        }

        public override Task Invoke(IOwinContext context)
        {
            var request = context.Request;

            if (options.PreferredStandard != ForwardHeaderStandard.Unspecified)
            {
                ProcessForwardHeaders(context, request, options.PreferredStandard);
            }
            else
            {
                var forwardedHeaderStandard = GetForwardedHeaderStandard(context.Request.Headers);

                if (forwardedHeaderStandard == ForwardHeaderStandard.Unspecified)
                    return Next.Invoke(context);

                ProcessForwardHeaders(context, request, forwardedHeaderStandard);
            }

            return Next.Invoke(context);
        }

        private void ProcessForwardHeaders(IOwinContext context, IOwinRequest request, ForwardHeaderStandard forwardedHeaderStandard)
        {
            switch (forwardedHeaderStandard)
            {
                case ForwardHeaderStandard.Forwarded:
                    {
                        var forwardedHeaders = new ForwardedHeaders(request, options);
                        ProcessForwardedHeaders(forwardedHeaders, context, request);
                        break;
                    }
                case ForwardHeaderStandard.Xforwarded:
                    {
                        var forwardedHeaders = new XForwardedHeaders(request, options);
                        ProcessForwardedHeaders(forwardedHeaders, context, request);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Processing of {forwardedHeaderStandard} is not supported/implemented");
            }
        }

        private void ProcessForwardedHeaders(IForwardedHeaders headers, IOwinContext context, IOwinRequest request)
        {
            var updatedHeaders = headers.ApplyForwardedHeaders(request);
            context.UpdateRequestHeaders(updatedHeaders);
        }

        private ForwardHeaderStandard GetForwardedHeaderStandard(IHeaderDictionary headers)
        {
            /* INFO: Usually there are two standards we are expecting here.
             * Either the X-Forwarded-* headers or the Forwarded header 
             * defined in RFC-7239 here: https://tools.ietf.org/pdf/rfc7239.pdf.
             * 
             * Giving precedence to the RFC here, if we see that we have Forwarded
             * header set here, we would definitely pick that one over the X-Forwarded one.
             */

            if (!string.IsNullOrWhiteSpace(headers[ForwardedHeaderName]))
                return ForwardHeaderStandard.Forwarded;

            if (headers.Keys.Any(x => x.StartsWith(XForwardedHeaderPrefix) && !string.IsNullOrWhiteSpace(headers[x])))
                return ForwardHeaderStandard.Xforwarded;

            return ForwardHeaderStandard.Unspecified;
        }
    }
}

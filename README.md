# RFC-7234 compliant reverse-proxy middleware for OWIN

To use the middleware in a OWIN startup:

        public void Configuration(IAppBuilder app)
        {
            app.UseForwardHeaders(options: default(ForwardedHeadersOptions));
        }

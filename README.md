# RFC-7234 compliant reverse-proxy middleware for OWIN

To use the middleware in a OWIN startup:

        public void Configuration(IAppBuilder app)
        {
            app.UseForwardHeaders(options: default(ForwardedHeadersOptions));
        }

You can opt for a X-Forwarded-\* standard of proxy headers or you can opt for the new RFC-7234 compliant forwarded headers. Using ForwardedHeadersOptions you can define your desired standard. 

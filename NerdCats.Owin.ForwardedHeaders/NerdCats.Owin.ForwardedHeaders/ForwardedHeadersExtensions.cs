using Owin;

namespace NerdCats.Owin
{
    using System;
    using Owin;

    public static class ForwardedHeadersExtensions
    {
        public static IAppBuilder UseForwardHeaders(this IAppBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Use<ForwardedHeadersHandler>();
        }

        public static IAppBuilder UseForwardHeaders(this IAppBuilder builder, ForwardedHeadersOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Use<ForwardedHeadersHandler>(options);
        }
    }
}

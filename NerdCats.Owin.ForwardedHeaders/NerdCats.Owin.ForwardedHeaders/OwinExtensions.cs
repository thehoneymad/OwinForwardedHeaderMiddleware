namespace NerdCats.Owin
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Owin;

    /// <summary>
    /// Simple Owin extension methods to make life easier
    /// </summary>
    public static class OwinExtensions
    {
        public static void UpdateRequestHeaders(this IOwinContext context, IHeaderDictionary headers)
        {
            /* INFO: For some reason the Owin header changes done through the Owin wrappers are not kept unless we make 
             * a copy of the header dictionary and assigns that to the Owin environment.
             * It's an existing Owin bug and this method should only be removed when the bug is fixed in later owin versions.
             */
            var mutableHeaders = new Dictionary<string, string[]>(headers, StringComparer.OrdinalIgnoreCase);
            context.Environment["owin.RequestHeaders"] = mutableHeaders;
        }
    }
}

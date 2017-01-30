namespace NerdCats.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Owin;

    /// <summary>
    /// RFC-7239 compliant http forwarded header implementation
    /// </summary>
    public class ForwardedHeaders : IForwardedHeaders
    {
        private const string ForwardedForHeaderName = "Forwarded";
        private const string ForParam = "for";
        private const string HostParam = "host";
        private const string ProtoParam = "proto";
        private const string ByParam = "by";

        private IList<string> ForwardedFor;
        private ForwardedHeadersOptions options;

        /// <summary>
        /// Creates a new instance of RFC-7239 compliant forwarded header implementation
        /// </summary>
        /// <param name="request">Current IOwinRequest</param>
        /// <param name="options">Forwarded headers options</param>
        public ForwardedHeaders(IOwinRequest request, ForwardedHeadersOptions options)
        {
            this.options = options;
            ProcessForwardedHeaders(request);
        }

        /// <inheritDoc/>
        public int? ForwardedPort { get; }

        /// <inheritDoc/>
        public List<ForwarderSet> Forwarders { get; set; }

        /// <inheritDoc/>
        public IHeaderDictionary ApplyForwardedHeaders(IOwinRequest request)
        {
            var firstForwarderSet = this.Forwarders?.LastOrDefault();
            if (firstForwarderSet == null) return request.Headers;

            if (!String.IsNullOrWhiteSpace(firstForwarderSet.Value.Scheme))
                request.Scheme = firstForwarderSet.Value.Scheme;

            if (!string.IsNullOrWhiteSpace(firstForwarderSet.Value.Host))
            {
                request.Host = new HostString(firstForwarderSet.Value.Host.Trim('\"'));
            }

            /*
             * INFO: We potentially do not know how "by" token=value params are 
             * supposed to be translated back to a backend server through request
             * payload. Thus using a temporary "Forwarded-by" header to pass it along.
             * 
             * TODO: If a real life usage of this parameter in a well known HTTP request header
             * is found, please update the following implementation.
             */
            if (!string.IsNullOrWhiteSpace(firstForwarderSet.Value.By))
                request.Headers.Add("Forwarded-by", new[] { firstForwarderSet.Value.By });

            request.RemoteIpAddress = firstForwarderSet.Value.RemoteIpAndPort?.Trim('\"');
            return request.Headers;
        }

        private void ProcessForwardedHeaders(IOwinRequest request)
        {
            ForwardedFor = request.Headers.GetCommaSeparatedValues(ForwardedForHeaderName)
               .Reverse().ToList();

            if (options.ForwardLimit.HasValue && ForwardedFor.Count > options.ForwardLimit.Value)
            {
                ForwardedFor = ForwardedFor.Take(options.ForwardLimit.Value).ToList();
            }

            if (ForwardedFor?.Count == 0) return;
            var forwarders = new List<ForwarderSet>();

            /*
             * INFO:
             * A forwarded pair is defined by RFC 7239  as
             * forwarded-pair = token "=" value 
             * 
             * We delimited token on the left side of the expression as by|for|proto|host 
             * because that's only what we are interested in. According to RFC 7230 a token is defined as 
             *      token = 1*tchar;
             *      tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*"/ "+" / "-" / "." / "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA ; any VCHAR, except delimiters
             *  
             * value can be expressed as value = token / quoted-string. For this expression 
             * we did use the actual token representation from RFC-7230.
             *  
             * Quoted string is described as:
             *       quoted-string  = DQUOTE *( qdtext / quoted-pair ) DQUOTE
             *       qdtext         = HTAB / SP /%x21 / %x23-5B / %x5D-7E / obs-text
             *       obs-text       = %x80-FF
             *       
             * Quoted pair looks is defined as: 
             *      quoted-pair    = "\" ( HTAB / SP / VCHAR / obs-text )
             *  
             * The complete regex we used here looks like:
             *  
             * The first part should match by|for|proto|host
             * (?<param>(?i)by|for|proto|host(?-i))
             *  
             * The RFC-7230 token compliant regex is
             * (?<tokenOrQstring>(?<token>[-!#$%&'*+.^_`|~0-9a-zA-Z]+)
             *  
             * The RFC-7230 qdtext compliant regex is
             * (?<qstring>"[\w\t\'\s\!#-[\]-\`\{-\}~]*")
             *  
             * The complete regex here is
             * (?<param>(?i)by|for|proto|host(?-i))=(?<tokenOrQstring>(?<token>[-!#$%&'*+.^_`|~0-9a-zA-Z]+)|((?<qstring>"[\w\t\'\s#-\[!\]-\`~\{-\}]*")|(?<quotedPair>\\[!-~\t\s]+)))
             *  
             * Since, it's a regex, it might deem error prone in future, kindly update the description
             * here to make sure we know what we did here. 
             * */

            const string paramGroupName = "param";
            const string tokenOrQstringGroupName = "tokenOrQstring";
            var forwardedPairRegex =
               $"(?<{paramGroupName}>(?i){ByParam}|{ForParam}|{ProtoParam}|{HostParam}(?-i))=(?<{tokenOrQstringGroupName}>(?<token>[-!#$%&\'*+.^_`|~0-9a-zA-Z]+)|((?<qstring>\"[\\w\\t\\\'\\s#-\\[!\\]-\\`~\\{{-\\}}]*\")|(?<quotedPair>\\\\[!-~\\t\\s]+)))";

            foreach (var fieldValue in ForwardedFor)
            {
                var pairs = fieldValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

                List<KeyValuePair<string, string>> filteredPairs = new List<KeyValuePair<string, string>>();
                foreach (var pair in pairs)
                {
                    var matches = Regex.Matches(pair, forwardedPairRegex, RegexOptions.ExplicitCapture);

                    /* 
                     * Throw exception if no match is found or more than one match is found
                     * Throw exception is full match length is not equal to the actual token=value pair we found
                     * */
                    if (matches.Count != 1 || matches[0].Length != pair.Length)
                        throw new HeaderFormatException(ForwardedForHeaderName, pair);

                    var token = matches[0].Groups["param"].Value;
                    if (string.IsNullOrWhiteSpace(token))
                        throw new HeaderFormatException("Empty forwarded-pair token encountered for token-value pair " + pair);
                    var value = matches[0].Groups[tokenOrQstringGroupName].Value;
                    if (string.IsNullOrWhiteSpace(value))
                        throw new HeaderFormatException("Empty forwarded-pair value encountered for token " + token);

                    filteredPairs.Add(new KeyValuePair<string, string>(token, value));
                }


                var duplicatePair = filteredPairs.GroupBy(x => x.Key).Where(g => g.Count() > 1).Select(x => x.Key).ToList();
                if (duplicatePair.Any())
                    throw new HeaderFormatException($"Duplicate key-value pair found in fields {string.Join(",", duplicatePair)}");

                var forwarder = new ForwarderSet();

                foreach (var keyVal in filteredPairs)
                {
                    switch (keyVal.Key.ToLower())
                    {
                        case ForParam:
                            forwarder.RemoteIpAndPort = keyVal.Value;
                            break;
                        case HostParam:
                            forwarder.Host = keyVal.Value;
                            break;
                        case ProtoParam:
                            forwarder.Scheme = keyVal.Value;
                            break;
                        case ByParam:
                            forwarder.By = keyVal.Value;
                            break;
                        default:
                            if (options.RequireHeaderSymmetry)
                                throw new HeaderSymmetryException($"Invalid parameter {keyVal.Key} encountered");
                            break;
                    }
                }
                forwarders.Add(forwarder);
            }
            this.Forwarders = forwarders;
        }

    }
}

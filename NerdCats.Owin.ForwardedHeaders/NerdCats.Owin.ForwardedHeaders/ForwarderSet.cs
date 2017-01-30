namespace NerdCats.Owin
{
    /// <summary>
    /// A single set of forwarder described in any standard http forwarded header
    /// </summary>
    public struct ForwarderSet
    {
        public string RemoteIpAndPort;
        public string Host;
        public string Scheme;
        public string By;
    }
}
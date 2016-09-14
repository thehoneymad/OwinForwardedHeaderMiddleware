namespace NerdCats.Owin
{
    public struct ForwardedHeadersOptions
    {
        public bool RequireHeaderSymmetry { get; set; }
        public int? ForwardLimit { get; set; }
        public ForwardHeaderStandard PreferredStandard { get; set; }
    }
}
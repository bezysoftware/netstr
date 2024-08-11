namespace Netstr.Options
{
    public record RelayInformationOptions
    {
        public string? Name { get; init; }

        public string? Description { get; init; }

        public string? Contact { get; init; }

        public string? PublicKey { get; init; }

        public int[]? SupportedNips { get; init; }
    }
}

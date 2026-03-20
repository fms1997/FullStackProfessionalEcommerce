namespace FulSpectrum.Api.Observability;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public bool EnableOtlpExporter { get; set; }
    public string? OtlpEndpoint { get; set; }
    public string ServiceName { get; set; } = "FulSpectrum.Api";
}

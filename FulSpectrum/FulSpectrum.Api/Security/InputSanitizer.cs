namespace FulSpectrum.Api.Security;

public static class InputSanitizer
{
    public static string Clean(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var cleaned = new string(value.Trim().Where(c => !char.IsControl(c)).ToArray());
        return cleaned;
    }
}

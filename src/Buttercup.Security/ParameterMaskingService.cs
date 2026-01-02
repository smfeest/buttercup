namespace Buttercup.Security;

internal sealed partial class ParameterMaskingService : IParameterMaskingService
{
    public string MaskToken(string token) => token.Length > 6 ? $"{token[..6]}…" : token;
}

namespace Buttercup.Security;

internal sealed partial class ParameterMaskingService : IParameterMaskingService
{
    public string MaskEmail(string email) =>
        string.Join(
            '@',
            email.Split('@').Select(part =>
            {
                var length = part.Length;

                return length < 6
                    ? new string('*', length)
                    : $"{part[..2]}{new string('*', length - 4)}{part[^2..]}";
            }));

    public string MaskToken(string token) => token.Length > 6 ? $"{token[..6]}…" : token;
}

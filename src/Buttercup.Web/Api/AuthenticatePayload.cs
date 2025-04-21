using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed record AuthenticatePayload
{
    public AuthenticatePayload(string accessToken, User user)
    {
        this.AccessToken = accessToken;
        this.User = user;
    }

    public AuthenticatePayload(AuthenticateError error) => this.Errors = [error];

    public bool IsSuccess => this.AccessToken is not null;
    public string? AccessToken { get; }
    public User? User { get; }
    public IReadOnlyList<AuthenticateError>? Errors { get; }
}

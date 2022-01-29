using Buttercup.Models;

namespace Buttercup.Web.Api;

public record AuthenticatePayload(bool IsSuccess, string? AccessToken = null, User? User = null);

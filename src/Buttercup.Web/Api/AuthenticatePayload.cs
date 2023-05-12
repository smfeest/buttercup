using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public record AuthenticatePayload(bool IsSuccess, string? AccessToken = null, User? User = null);

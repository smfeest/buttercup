namespace Buttercup.Web.Api;

[UnionType]
public abstract record AuthenticateError(string Message) : IMutationError;

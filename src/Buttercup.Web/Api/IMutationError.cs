namespace Buttercup.Web.Api;

[InterfaceType("Error")]
public interface IMutationError
{
    string Message { get; }
}

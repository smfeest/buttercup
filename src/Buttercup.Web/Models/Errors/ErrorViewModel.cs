namespace Buttercup.Web.Models.Errors;

public sealed record ErrorViewModel(string RequestId)
{
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}

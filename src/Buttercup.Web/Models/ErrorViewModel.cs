namespace Buttercup.Web.Models;

public sealed record ErrorViewModel(string RequestId)
{
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}

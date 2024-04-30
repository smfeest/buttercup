#pragma warning disable CA1716
namespace Buttercup.Web.Models.Error;
#pragma warning restore CA1716

public sealed record ErrorViewModel(string RequestId)
{
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}

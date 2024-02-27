using Buttercup.Application.Validation;

namespace Buttercup.Web.Api;

public sealed class InputObjectValidatorFactory(IServiceProvider serviceProvider)
    : IInputObjectValidatorFactory
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    public IInputObjectValidator<T> CreateValidator<T>(ISchema schema) where T : notnull =>
        new InputObjectValidator<T>(
            schema, this.serviceProvider.GetRequiredService<IValidator<T>>());
}

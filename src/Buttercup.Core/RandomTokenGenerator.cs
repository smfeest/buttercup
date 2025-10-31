namespace Buttercup;

internal sealed class RandomTokenGenerator(
    IRandomNumberGeneratorFactory randomNumberGeneratorFactory)
    : IRandomTokenGenerator
{
    public IRandomNumberGeneratorFactory RandomNumberGeneratorFactory { get; } =
        randomNumberGeneratorFactory;

    public string Generate(int n)
    {
        var rng = this.RandomNumberGeneratorFactory.Create();

        var bytes = new byte[n * 3];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
    }
}

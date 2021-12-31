namespace Buttercup.Web.Authentication;

public class RandomTokenGenerator : IRandomTokenGenerator
{
    public RandomTokenGenerator(IRandomNumberGeneratorFactory randomNumberGeneratorFactory) =>
        this.RandomNumberGeneratorFactory = randomNumberGeneratorFactory;

    public IRandomNumberGeneratorFactory RandomNumberGeneratorFactory { get; }

    public string Generate(int n)
    {
        var rng = this.RandomNumberGeneratorFactory.Create();

        var bytes = new byte[n * 3];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
    }
}

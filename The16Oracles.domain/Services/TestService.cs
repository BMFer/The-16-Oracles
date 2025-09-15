namespace The16Oracles.domain.Services
{
    public interface ITestService
    {
        string Name { get; }

    }

    public class TestService : ITestService
    {
        public string Name => this.GetType().Name;
    }
}

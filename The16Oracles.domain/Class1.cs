using The16Oracles.domain.Services;

namespace The16Oracles.domain
{
    public class Class1
    {
        private readonly ITestService _testService;
        public Class1(ITestService testService)
        {
            _testService = testService;
        }
    }
}

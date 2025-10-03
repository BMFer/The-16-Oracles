using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class TestServiceTests
    {
        [Test]
        public async Task GetDataModelName_ShouldReturnDataModelName()
        {
            // Arrange
            var dataModel = new DataModel();
            var testService = new TestService(dataModel);

            // Act
            var result = await testService.GetDataModelName();

            // Assert
            Assert.That(result, Is.EqualTo("DataModel"));
        }

        [Test]
        public async Task GetDataModelName_WithMockDataModel_ShouldReturnCustomName()
        {
            // Arrange
            var mockDataModel = new MockDataModel("CustomDataModel");
            var testService = new TestService(mockDataModel);

            // Act
            var result = await testService.GetDataModelName();

            // Assert
            Assert.That(result, Is.EqualTo("CustomDataModel"));
        }

        [Test]
        public void TestService_ShouldAcceptIDataModelInterface()
        {
            // Arrange
            IDataModel dataModel = new DataModel();

            // Act & Assert
            Assert.DoesNotThrow(() => new TestService(dataModel));
        }

        [Test]
        public async Task GetDataModelName_ShouldExecuteAsynchronously()
        {
            // Arrange
            var dataModel = new DataModel();
            var testService = new TestService(dataModel);

            // Act
            var task = testService.GetDataModelName();

            // Assert
            Assert.That(task, Is.InstanceOf<Task<string>>());
            var result = await task;
            Assert.That(result, Is.Not.Null);
        }

        // Helper mock class for testing
        private class MockDataModel : IDataModel
        {
            private readonly string _name;

            public MockDataModel(string name)
            {
                _name = name;
            }

            public string Name => _name;
        }
    }
}

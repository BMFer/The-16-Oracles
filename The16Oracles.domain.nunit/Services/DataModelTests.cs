using The16Oracles.domain.Services;

namespace The16Oracles.domain.nunit.Services
{
    [TestFixture]
    public class DataModelTests
    {
        [Test]
        public void DataModel_ShouldReturnClassName()
        {
            // Arrange
            var dataModel = new DataModel();

            // Act
            var name = dataModel.Name;

            // Assert
            Assert.That(name, Is.EqualTo("DataModel"));
        }

        [Test]
        public void DataModel_ShouldImplementIDataModel()
        {
            // Arrange
            var dataModel = new DataModel();

            // Assert
            Assert.That(dataModel, Is.InstanceOf<IDataModel>());
        }

        [Test]
        public void DataModel_NameProperty_ShouldBeReadOnly()
        {
            // Arrange
            var dataModel = new DataModel();

            // Act
            var name1 = dataModel.Name;
            var name2 = dataModel.Name;

            // Assert - Should return the same value consistently
            Assert.That(name1, Is.EqualTo(name2));
            Assert.That(name1, Is.EqualTo("DataModel"));
        }

        [Test]
        public void DataModel_Constructor_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new DataModel());
        }
    }
}

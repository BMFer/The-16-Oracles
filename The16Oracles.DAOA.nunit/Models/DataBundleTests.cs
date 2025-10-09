using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.nunit.Models;

[TestFixture]
public class DataBundleTests
{
    [Test]
    public void DataBundle_ShouldInstantiateSuccessfully()
    {
        // Arrange & Act
        var bundle = new DataBundle();

        // Assert
        Assert.That(bundle, Is.Not.Null);
    }

    [Test]
    public void DataBundle_ShouldBeEmptyClass()
    {
        // Arrange & Act
        var bundle = new DataBundle();

        // Assert - DataBundle should have no public properties
        var properties = typeof(DataBundle).GetProperties();
        Assert.That(properties.Length, Is.EqualTo(0));
    }
}

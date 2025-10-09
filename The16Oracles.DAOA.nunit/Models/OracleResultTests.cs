using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.nunit.Models;

[TestFixture]
public class OracleResultTests
{
    [Test]
    public void OracleResult_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var result = new OracleResult();

        // Assert
        Assert.That(result.ModuleName, Is.Null);
        Assert.That(result.ConfidenceScore, Is.EqualTo(0.0));
        Assert.That(result.Metrics, Is.Null);
        Assert.That(result.Timestamp, Is.EqualTo(default(DateTime)));
    }

    [Test]
    public void OracleResult_ShouldStoreModuleName()
    {
        // Arrange
        var result = new OracleResult
        {
            ModuleName = "Test Oracle"
        };

        // Assert
        Assert.That(result.ModuleName, Is.EqualTo("Test Oracle"));
    }

    [Test]
    public void OracleResult_ShouldStoreConfidenceScoreInRange()
    {
        // Arrange & Act
        var result = new OracleResult
        {
            ConfidenceScore = 0.75
        };

        // Assert
        Assert.That(result.ConfidenceScore, Is.EqualTo(0.75));
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
    }

    [Test]
    public void OracleResult_ShouldStoreNegativeConfidenceScore()
    {
        // Arrange & Act
        var result = new OracleResult
        {
            ConfidenceScore = -0.5
        };

        // Assert
        Assert.That(result.ConfidenceScore, Is.EqualTo(-0.5));
    }

    [Test]
    public void OracleResult_ShouldStoreMetrics()
    {
        // Arrange
        var metrics = new Dictionary<string, object>
        {
            ["InterestRate"] = 4.5,
            ["GDP"] = 25000.0,
            ["CPI"] = 320.5
        };

        // Act
        var result = new OracleResult
        {
            Metrics = metrics
        };

        // Assert
        Assert.That(result.Metrics, Is.Not.Null);
        Assert.That(result.Metrics.Count, Is.EqualTo(3));
        Assert.That(result.Metrics["InterestRate"], Is.EqualTo(4.5));
        Assert.That(result.Metrics["GDP"], Is.EqualTo(25000.0));
        Assert.That(result.Metrics["CPI"], Is.EqualTo(320.5));
    }

    [Test]
    public void OracleResult_ShouldStoreTimestamp()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var result = new OracleResult
        {
            Timestamp = timestamp
        };

        // Assert
        Assert.That(result.Timestamp, Is.EqualTo(timestamp));
    }

    [Test]
    public void OracleResult_ShouldStoreAllProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var metrics = new Dictionary<string, object>
        {
            ["Volatility"] = 0.25,
            ["Volume"] = 1000000
        };

        // Act
        var result = new OracleResult
        {
            ModuleName = "Complete Test Oracle",
            ConfidenceScore = 0.85,
            Metrics = metrics,
            Timestamp = timestamp
        };

        // Assert
        Assert.That(result.ModuleName, Is.EqualTo("Complete Test Oracle"));
        Assert.That(result.ConfidenceScore, Is.EqualTo(0.85));
        Assert.That(result.Metrics.Count, Is.EqualTo(2));
        Assert.That(result.Timestamp, Is.EqualTo(timestamp));
    }
}

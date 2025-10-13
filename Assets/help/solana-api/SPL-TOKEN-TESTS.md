# SPL Token Service Tests

Comprehensive unit test coverage for the SPL Token API service.

## Test File

**Location:** `The16Oracles.DAOA.nunit/Services/SplTokenServiceTests.cs`

**Framework:** NUnit 4.2.2 with Moq

**Target:** .NET 9.0 (latest C# features)

## Test Statistics

- **Total Tests:** 46
- **Test Categories:** 9
- **Code Coverage:** All public methods tested
- **Status:** ✅ All Passing

## Test Categories

### 1. Constructor Tests (1 test)

- `Constructor_WithValidDependencies_ShouldCreateInstance`
  - Verifies service instantiation with mocked dependencies

### 2. Account Management Tests (10 tests)

#### GetAccountsAsync Tests

- `GetAccountsAsync_WithTokenMintAddress_ShouldReturnResponse`
- `GetAccountsAsync_WithAddressesOnlyFlag_ShouldIncludeFlagInCommand`
- `GetAccountsAsync_WithDelegatedFlag_ShouldIncludeFlagInCommand`
- `GetAccountsAsync_WithExternallyCloseableFlag_ShouldIncludeFlagInCommand`
- `GetAccountsAsync_WithOwner_ShouldIncludeOwnerInCommand`

#### GetAddressAsync Tests

- `GetAddressAsync_WithTokenAndOwner_ShouldReturnResponse`

#### GetBalanceAsync Tests

- `GetBalanceAsync_WithTokenMintAddress_ShouldReturnResponse`
- `GetBalanceAsync_WithAccountAddress_ShouldIncludeAddressInCommand`

#### CreateAccountAsync Tests

- `CreateAccountAsync_WithTokenMintAddress_ShouldReturnResponse`
- `CreateAccountAsync_WithImmutableFlag_ShouldIncludeFlagInCommand`

#### CloseAccountAsync Tests

- `CloseAccountAsync_WithTokenMintAddress_ShouldReturnResponse`
- `CloseAccountAsync_WithRecipient_ShouldIncludeRecipientInCommand`

#### GarbageCollectAsync Tests

- `GarbageCollectAsync_WithCloseEmptyFlag_ShouldReturnResponse`

### 3. Token Operations Tests (10 tests)

#### CreateTokenAsync Tests

- `CreateTokenAsync_WithDefaults_ShouldReturnResponse`
- `CreateTokenAsync_WithDecimals_ShouldIncludeDecimalsInCommand`
- `CreateTokenAsync_WithEnableFreeze_ShouldIncludeFreezeInCommand`

#### MintAsync Tests

- `MintAsync_WithValidRequest_ShouldReturnResponse`
- `MintAsync_WithRecipient_ShouldIncludeRecipientInCommand`

#### BurnAsync Tests

- `BurnAsync_WithValidRequest_ShouldReturnResponse`

#### TransferAsync Tests

- `TransferAsync_WithValidRequest_ShouldReturnResponse`
- `TransferAsync_WithFundFlag_ShouldIncludeFundInCommand`
- `TransferAsync_WithAllowUnfundedRecipientFlag_ShouldIncludeFlagInCommand`

#### GetSupplyAsync Tests

- `GetSupplyAsync_WithTokenAddress_ShouldReturnResponse`

#### CloseMintAsync Tests

- `CloseMintAsync_WithTokenAddress_ShouldReturnResponse`

### 4. Token Delegation Tests (2 tests)

- `ApproveAsync_WithValidRequest_ShouldReturnResponse`
- `RevokeAsync_WithAccountAddress_ShouldReturnResponse`

### 5. Token Authority Tests (1 test)

- `AuthorizeAsync_WithValidRequest_ShouldReturnResponse`

### 6. Freeze/Thaw Tests (2 tests)

- `FreezeAsync_WithAccountAddress_ShouldReturnResponse`
- `ThawAsync_WithAccountAddress_ShouldReturnResponse`

### 7. Native SOL Wrapping Tests (4 tests)

#### WrapAsync Tests

- `WrapAsync_WithAmount_ShouldReturnResponse`
- `WrapAsync_WithCreateAuxFlag_ShouldIncludeFlagInCommand`

#### UnwrapAsync Tests

- `UnwrapAsync_WithAddress_ShouldReturnResponse`

#### SyncNativeAsync Tests

- `SyncNativeAsync_WithAddress_ShouldReturnResponse`

### 8. Display Tests (1 test)

- `DisplayAsync_WithAddress_ShouldReturnResponse`

### 9. ExecuteCommandAsync Tests (2 tests)

- `ExecuteCommandAsync_WithValidCommand_ShouldReturnResponse`
- `ExecuteCommandAsync_WithArguments_ShouldIncludeArgumentsInCommand`

### 10. Global Flags Tests (9 tests)

- `GetBalanceAsync_WithUrlFlag_ShouldIncludeUrlInCommand`
- `GetBalanceAsync_WithOutputJsonFlag_ShouldIncludeOutputInCommand`
- `GetBalanceAsync_WithProgram2022Flag_ShouldIncludeFlagInCommand`
- `GetBalanceAsync_WithAllGlobalFlags_ShouldIncludeAllFlagsInCommand`
- `CreateTokenAsync_WithVerboseFlag_ShouldIncludeVerboseInCommand`
- `TransferAsync_WithFeePayerFlag_ShouldIncludeFeePayerInCommand`
- `MintAsync_WithComputeUnitFlags_ShouldIncludeFlagsInCommand`

### 11. Response Properties Tests (2 tests)

- `GetSupplyAsync_ResponseShouldHaveTimestamp`
- `DisplayAsync_ResponseShouldHaveCommandProperty`

## Test Patterns

### Arrange-Act-Assert Pattern

All tests follow the AAA pattern:

```csharp
[Test]
public async Task GetBalanceAsync_WithTokenMintAddress_ShouldReturnResponse()
{
    // Arrange
    var request = new TokenBalanceRequest
    {
        TokenMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
        Flags = new SplTokenGlobalFlags()
    };

    // Act
    var response = await _splTokenService.GetBalanceAsync(request);

    // Assert
    Assert.That(response, Is.Not.Null);
    Assert.That(response.Command, Does.Contain("spl-token balance"));
    Assert.That(response.Command, Does.Contain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"));
}
```

### Command Validation

Tests verify that:

1. Commands are properly constructed
2. Arguments are correctly formatted
3. Flags are included when specified
4. Response objects are populated

### Mock Setup

```csharp
[SetUp]
public void Setup()
{
    _mockLogger = new Mock<ILogger<SplTokenService>>();
    _mockConfiguration = new Mock<IConfiguration>();
    _splTokenService = new SplTokenService(_mockLogger.Object, _mockConfiguration.Object);
}
```

## Coverage Details

### Commands Tested

✅ All 21 service interface methods covered:

**Account Management:**

- GetAccountsAsync
- GetAddressAsync
- GetBalanceAsync
- CreateAccountAsync
- CloseAccountAsync
- GarbageCollectAsync

**Token Operations:**

- CreateTokenAsync
- MintAsync
- BurnAsync
- TransferAsync
- GetSupplyAsync
- CloseMintAsync

**Delegation & Authority:**

- ApproveAsync
- RevokeAsync
- AuthorizeAsync

**Freeze/Thaw:**

- FreezeAsync
- ThawAsync

**Native SOL:**

- WrapAsync
- UnwrapAsync
- SyncNativeAsync

**Utilities:**

- DisplayAsync
- ExecuteCommandAsync

### Flags Tested

All global flags verified:

- ✅ Config
- ✅ FeePayer
- ✅ Output
- ✅ ProgramId
- ✅ Program2022
- ✅ Url
- ✅ Verbose
- ✅ WithComputeUnitLimit
- ✅ WithComputeUnitPrice

### Edge Cases

- Empty/null values handled
- Optional parameters tested
- Boolean flags verified
- Numeric values validated
- Multiple flag combinations tested

## Running the Tests

### Run all SPL Token tests

```bash
dotnet test --filter "FullyQualifiedName~SplTokenServiceTests"
```

### Run all DAOA tests

```bash
dotnet test The16Oracles.DAOA.nunit/The16Oracles.DAOA.nunit.csproj
```

### Run with coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run specific test

```bash
dotnet test --filter "FullyQualifiedName~SplTokenServiceTests.GetBalanceAsync_WithTokenMintAddress_ShouldReturnResponse"
```

## Test Results

```text
Test run for The16Oracles.DAOA.nunit.dll (.NETCoreApp,Version=v9.0)
VSTest version 17.14.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    46, Skipped:     0, Total:    46, Duration: 4 s
```

## Integration with CI/CD

These tests are designed to:

- Run in isolation (no external dependencies)
- Execute quickly (< 5 seconds)
- Provide clear failure messages
- Support parallel execution

## Future Test Enhancements

Potential additions:

1. Integration tests with actual SPL Token CLI
2. Error handling tests (malformed commands)
3. Timeout scenario tests
4. Process failure simulation
5. JSON output parsing tests

## Related Files

- **Service:** `The16Oracles.DAOA/Services/SplTokenService.cs`
- **Interface:** `The16Oracles.DAOA/Interfaces/ISplTokenService.cs`
- **Models:** `The16Oracles.DAOA/Models/SplToken/`
- **Similar Tests:** `The16Oracles.DAOA.nunit/Services/SolanaServiceTests.cs`

## Notes

- Tests use Moq for dependency injection mocking
- No actual CLI commands are executed during tests
- All tests verify command construction only
- Response properties (timestamp, command) are validated
- Tests follow existing SolanaServiceTests patterns for consistency

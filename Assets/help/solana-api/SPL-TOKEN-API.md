# SPL Token API Wrapper

A RESTful API wrapper for the Solana SPL Token CLI, integrated into The16Oracles.DAOA project.

## Overview

The SPL Token API provides HTTP endpoints for all major SPL Token CLI operations, enabling programmatic token management on Solana without direct CLI access.

**Base URL:** `/api/spl-token`

**Tag in Swagger:** `SPL Token CLI`

## Architecture

### Service Pattern

1. **Models** (`The16Oracles.DAOA.Models.SplToken`)
   - `SplTokenGlobalFlags` - Global flags for all commands
   - `SplTokenCommandResponse` - Standardized response model
   - Request models for each command

2. **Interface** (`ISplTokenService`)
   - Defines all SPL Token operations

3. **Service** (`SplTokenService`)
   - Executes CLI commands via Process
   - Handles output/error parsing
   - Supports JSON output format

4. **Endpoints** (Program.cs)
   - POST endpoints for all operations
   - Swagger/OpenAPI documentation
   - Grouped under "SPL Token CLI" tag

## API Endpoints

### Account Management

#### `POST /api/spl-token/accounts`
List all token accounts by owner
- Request: `TokenAccountsRequest`
- Filters: token mint, owner, delegated, externally-closeable

#### `POST /api/spl-token/address`
Get wallet address
- Request: `TokenAddressRequest`
- Options: owner, token mint

#### `POST /api/spl-token/balance`
Get token account balance
- Request: `TokenBalanceRequest`
- Options: token mint, account address, owner

#### `POST /api/spl-token/create-account`
Create a new token account
- Request: `CreateTokenAccountRequest`
- Options: immutable, owner, account keypair

#### `POST /api/spl-token/close`
Close a token account
- Request: `CloseTokenAccountRequest`
- Options: address, owner, close authority, recipient

#### `POST /api/spl-token/gc`
Cleanup unnecessary token accounts
- Request: `GarbageCollectRequest`
- Options: close empty associated accounts, owner

### Token Operations

#### `POST /api/spl-token/create-token`
Create a new token
- Request: `CreateTokenRequest`
- Options: decimals, mint authority, enable freeze

#### `POST /api/spl-token/mint`
Mint new tokens
- Request: `MintTokensRequest`
- Parameters: token address, amount, recipient

#### `POST /api/spl-token/burn`
Burn tokens from an account
- Request: `BurnTokensRequest`
- Parameters: account address, amount

#### `POST /api/spl-token/transfer`
Transfer tokens between accounts
- Request: `TransferTokensRequest`
- Parameters: token address, amount, recipient
- Options: from, fund recipient, allow unfunded

#### `POST /api/spl-token/supply`
Get token supply
- Request: `TokenSupplyRequest`
- Parameter: token address

#### `POST /api/spl-token/close-mint`
Close a token mint
- Request: `CloseMintRequest`
- Parameters: token address, close authority, recipient

### Token Delegation

#### `POST /api/spl-token/approve`
Approve a delegate for a token account
- Request: `ApproveRequest`
- Parameters: account, amount, delegate address

#### `POST /api/spl-token/revoke`
Revoke a delegate's authority
- Request: `RevokeRequest`
- Parameter: account address

### Token Authority

#### `POST /api/spl-token/authorize`
Authorize a new signing keypair
- Request: `AuthorizeRequest`
- Parameters: address, authority type (mint/freeze/owner/close), new authority

### Freeze/Thaw Operations

#### `POST /api/spl-token/freeze`
Freeze a token account
- Request: `FreezeAccountRequest`
- Parameters: account address, freeze authority

#### `POST /api/spl-token/thaw`
Thaw a token account
- Request: `ThawAccountRequest`
- Parameters: account address, freeze authority

### Native SOL Wrapping

#### `POST /api/spl-token/wrap`
Wrap native SOL in a SOL token account
- Request: `WrapRequest`
- Parameters: amount, wallet keypair

#### `POST /api/spl-token/unwrap`
Unwrap a SOL token account
- Request: `UnwrapRequest`
- Options: address, wallet keypair

#### `POST /api/spl-token/sync-native`
Sync a native SOL token account
- Request: `SyncNativeRequest`
- Options: address, owner

### Display

#### `POST /api/spl-token/display`
Query details of an SPL Token mint, account, or multisig
- Request: `DisplayRequest`
- Parameter: address

### Generic Execution

#### `POST /api/spl-token/execute`
Execute any SPL Token CLI command
- Request: `SplTokenCommandRequest`
- For advanced/custom operations

## Global Flags

All endpoints support the following global flags via `SplTokenGlobalFlags`:

- `Config` - Configuration file path
- `FeePayer` - Fee payer keypair
- `Output` - Output format (json, json-compact)
- `ProgramId` - SPL Token program ID
- `Program2022` - Use Token-2022 program
- `Url` - Solana RPC URL or moniker
- `Verbose` - Show additional information
- `WithComputeUnitLimit` - Compute unit limit
- `WithComputeUnitPrice` - Compute unit price

## Response Format

All endpoints return `SplTokenCommandResponse`:

```json
{
  "command": "spl-token balance ...",
  "success": true,
  "output": "100.5",
  "error": null,
  "exitCode": 0,
  "timestamp": "2024-01-15T10:30:00Z",
  "data": null
}
```

When `--output json` flag is used, the `data` field contains parsed JSON.

## Example Usage

### Create a Token Account

```http
POST /api/spl-token/create-account
Content-Type: application/json

{
  "tokenMintAddress": "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
  "owner": null,
  "immutable": false,
  "flags": {
    "url": "devnet",
    "output": "json"
  }
}
```

### Transfer Tokens

```http
POST /api/spl-token/transfer
Content-Type: application/json

{
  "tokenAddress": "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
  "amount": 100.5,
  "recipientAddress": "5FHwkrdxntdK24hgQU8qgBjn35Y1zwhz1GZwCkP4UJnC",
  "fund": true,
  "flags": {
    "url": "mainnet-beta"
  }
}
```

### Get Token Balance

```http
POST /api/spl-token/balance
Content-Type: application/json

{
  "tokenMintAddress": "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
  "flags": {
    "url": "devnet",
    "output": "json"
  }
}
```

## Requirements

- **SPL Token CLI** must be installed on the server
- Install: `cargo install spl-token-cli`
- Verify: `spl-token --version`

## Configuration

No additional configuration required. Service is registered in `Program.cs`:

```csharp
builder.Services.AddSingleton<ISplTokenService, SplTokenService>();
```

## Security Notes

- All operations require appropriate keypair access
- Use `--fee-payer` for transaction costs
- Consider using environment variables for keypair paths
- Never expose private keys in API requests
- Use secure connections (HTTPS) in production

## Testing

Access Swagger UI at: `https://localhost:5001/swagger`

All endpoints are documented with request/response schemas.

## Related Documentation

- [spl-token-help.md](./spl-token-help.md) - Complete CLI reference
- [spl-token-accounts.md](./spl-token-accounts.md) - Accounts command
- [spl-token-address.md](./spl-token-address.md) - Address command
- [spl-token-balance.md](./spl-token-balance.md) - Balance command
- [spl-token-close.md](./spl-token-close.md) - Close command
- [spl-token-create-account.md](./spl-token-create-account.md) - Create account command
- [spl-token-gc.md](./spl-token-gc.md) - Garbage collection command

## Source Code

- Models: `The16Oracles.DAOA/Models/SplToken/`
- Interface: `The16Oracles.DAOA/Interfaces/ISplTokenService.cs`
- Service: `The16Oracles.DAOA/Services/SplTokenService.cs`
- Endpoints: `The16Oracles.DAOA/Program.cs` (SPL Token API Endpoints region)

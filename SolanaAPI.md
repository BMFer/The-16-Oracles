# Solana CLI Web API Wrapper

This Web API wrapper provides RESTful endpoints for executing Solana CLI commands. All endpoints are available under `/api/solana/` and are tagged as "Solana CLI" in Swagger.

## API Overview

The API provides a comprehensive wrapper around the Solana CLI, organized by command categories matching the sol_help.md structure.

## Getting Started

1. Start the API server:
```bash
dotnet run --project The16Oracles.DAOA/The16Oracles.DAOA.csproj
```

2. Access Swagger UI at: `https://localhost:5001/swagger`

3. All Solana endpoints are grouped under the "Solana CLI" tag

## Global Flags

All endpoints support standard Solana CLI global flags through the `SolanaGlobalFlags` object:

```json
{
  "url": "devnet",
  "commitment": "confirmed",
  "output": "json",
  "keypair": "/path/to/keypair.json",
  "verbose": false,
  "noAddressLabels": false,
  "skipPreflight": false,
  "useQuic": false,
  "useTpuClient": false,
  "useUdp": false
}
```

### Supported URL Monikers
- `mainnet-beta` - Solana mainnet
- `testnet` - Testnet
- `devnet` - Devnet
- `localhost` - Local validator

### Commitment Levels
- `processed` - Query the most recent block
- `confirmed` - Query the most recent confirmed block
- `finalized` - Query the most recent finalized block

## API Endpoints

### Account & Balance Management

#### Get Balance
`POST /api/solana/balance`

Get the balance of a Solana account.

**Request Body:**
```json
{
  "address": "YourAddressHere",
  "flags": {
    "url": "devnet",
    "output": "json"
  }
}
```

#### Get Public Address
`GET /api/solana/address?url=devnet`

Get your public key/address.

#### Transfer SOL
`POST /api/solana/transfer`

Transfer SOL between accounts.

**Request Body:**
```json
{
  "recipientAddress": "RecipientAddressHere",
  "amount": 1.5,
  "flags": {
    "url": "devnet",
    "keypair": "/path/to/keypair.json"
  }
}
```

### Airdrop & Faucet

#### Request Airdrop
`POST /api/solana/airdrop`

Request SOL from a faucet (devnet/testnet only).

**Request Body:**
```json
{
  "amount": 1.0,
  "address": "YourAddressHere",
  "flags": {
    "url": "devnet"
  }
}
```

### Account Information

#### Get Account Details
`POST /api/solana/account`

Get detailed account information.

**Request Body:**
```json
{
  "address": "AccountAddressHere",
  "flags": {
    "url": "devnet",
    "output": "json"
  }
}
```

### Transaction Management

#### Get Transaction History
`POST /api/solana/transaction-history`

Get historical transactions for an address.

**Request Body:**
```json
{
  "address": "AddressHere",
  "limit": 10,
  "flags": {
    "url": "devnet"
  }
}
```

#### Confirm Transaction
`POST /api/solana/confirm`

Confirm a transaction by signature.

**Request Body:**
```json
{
  "signature": "TransactionSignatureHere",
  "flags": {
    "url": "devnet"
  }
}
```

#### Get Transaction Count
`GET /api/solana/transaction-count?url=devnet`

Get the current transaction count on the cluster.

#### Get Prioritization Fees
`GET /api/solana/prioritization-fees?url=devnet`

Get recent prioritization fees.

### Block & Slot Information

#### Get Block
`POST /api/solana/block`

Get a confirmed block by slot number.

**Request Body:**
```json
{
  "slot": 123456789,
  "flags": {
    "url": "devnet",
    "output": "json"
  }
}
```

#### Get Block Height
`GET /api/solana/block-height?url=devnet`

Get the current block height.

#### Get Current Slot
`GET /api/solana/slot?url=devnet`

Get the current slot number.

### Epoch Information

#### Get Epoch Info
`GET /api/solana/epoch-info?url=devnet`

Get information about the current epoch.

### Cluster Information

#### Get Cluster Version
`GET /api/solana/cluster-version?url=devnet`

Get the version of the cluster entrypoint.

#### Get Genesis Hash
`GET /api/solana/genesis-hash?url=devnet`

Get the genesis hash of the cluster.

#### Get Supply
`GET /api/solana/supply?url=devnet`

Get information about the cluster supply of SOL.

#### Get Inflation
`GET /api/solana/inflation?url=devnet`

Get inflation information.

#### Get Largest Accounts
`GET /api/solana/largest-accounts?url=devnet&limit=20`

Get addresses of largest cluster accounts.

### Validator Information

#### Get Validators
`GET /api/solana/validators?url=devnet`

Get summary information about current validators.

### Stake Account Management

#### Get Stake Account
`POST /api/solana/stake-account`

Get stake account information.

**Request Body:**
```json
{
  "accountAddress": "StakeAccountAddressHere",
  "flags": {
    "url": "devnet"
  }
}
```

#### Create Stake Account
`POST /api/solana/create-stake-account`

Create a new stake account.

**Request Body:**
```json
{
  "accountAddress": "NewStakeAccountAddress",
  "amount": 10.0,
  "flags": {
    "url": "devnet",
    "keypair": "/path/to/keypair.json"
  }
}
```

#### Delegate Stake
`POST /api/solana/delegate-stake`

Delegate stake to a vote account.

**Request Body:**
```json
{
  "stakeAccount": "StakeAccountAddress",
  "voteAccount": "VoteAccountAddress",
  "flags": {
    "url": "devnet",
    "keypair": "/path/to/keypair.json"
  }
}
```

### Vote Account Management

#### Get Vote Account
`POST /api/solana/vote-account`

Get vote account information.

**Request Body:**
```json
{
  "accountAddress": "VoteAccountAddressHere",
  "flags": {
    "url": "devnet"
  }
}
```

### Advanced Usage

#### Execute Generic Command
`POST /api/solana/execute`

Execute any Solana CLI command with custom arguments.

**Request Body:**
```json
{
  "command": "epoch",
  "arguments": {
    "--help": ""
  },
  "flags": {
    "url": "devnet",
    "verbose": true
  }
}
```

## Response Format

All endpoints return a standardized `SolanaCommandResponse`:

```json
{
  "command": "solana balance --url devnet",
  "success": true,
  "output": "1.5 SOL",
  "error": null,
  "exitCode": 0,
  "timestamp": "2025-10-12T10:30:00Z",
  "data": null
}
```

### Response Fields
- **command**: The full CLI command that was executed
- **success**: Boolean indicating if the command succeeded
- **output**: Standard output from the command
- **error**: Error message if the command failed
- **exitCode**: Process exit code (0 = success)
- **timestamp**: UTC timestamp when the command was executed
- **data**: Parsed JSON data (if `--output json` flag was used)

## Example Usage with cURL

### Get Balance
```bash
curl -X POST "https://localhost:5001/api/solana/balance" \
  -H "Content-Type: application/json" \
  -d '{
    "address": "YourAddressHere",
    "flags": {
      "url": "devnet"
    }
  }'
```

### Request Airdrop
```bash
curl -X POST "https://localhost:5001/api/solana/airdrop" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1.0,
    "flags": {
      "url": "devnet"
    }
  }'
```

### Get Cluster Version
```bash
curl -X GET "https://localhost:5001/api/solana/cluster-version?url=devnet"
```

## Requirements

1. **Solana CLI must be installed** on the server running the API
   - Install from: https://docs.solana.com/cli/install-solana-cli-tools
   - Verify with: `solana --version`

2. **Keypair Configuration** (for write operations)
   - Configure default keypair: `solana config set --keypair /path/to/keypair.json`
   - Or pass keypair path in request flags

3. **Network Configuration**
   - Default network can be configured: `solana config set --url devnet`
   - Or specify network in each request via flags

## Security Considerations

1. **Private Keys**: Never expose private keys through the API. Use secure keypair management.
2. **Rate Limiting**: Consider implementing rate limiting to prevent abuse.
3. **Authentication**: Add authentication/authorization for production deployments.
4. **Network Restrictions**: Restrict write operations to authorized users only.
5. **Input Validation**: All inputs are validated before execution.

## Error Handling

When a command fails, the response will indicate the failure:

```json
{
  "command": "solana balance InvalidAddress",
  "success": false,
  "output": "",
  "error": "Error: Invalid address",
  "exitCode": 1,
  "timestamp": "2025-10-12T10:30:00Z",
  "data": null
}
```

## Categories Covered

Based on the sol_help.md file, this API wrapper covers:

- ✅ Account & Balance Management
- ✅ Airdrop & Faucet
- ✅ Transaction Management
- ✅ Block & Slot Information
- ✅ Epoch Information
- ✅ Cluster Information
- ✅ Validator Information
- ✅ Stake Account Management
- ✅ Vote Account Management
- ✅ Generic Command Execution

## Future Enhancements

Potential additions:
- Nonce account management endpoints
- Address lookup table operations
- Program management endpoints
- Cryptographic operation endpoints
- Real-time transaction logs streaming
- WebSocket support for live updates
- Batch operation support

## Testing

Test the endpoints using Swagger UI at `https://localhost:5001/swagger` or with any HTTP client.

For quick testing on devnet:
1. Navigate to Swagger UI
2. Try the `/api/solana/cluster-version` endpoint (no parameters needed)
3. Try the `/api/solana/epoch-info` endpoint
4. Try the `/api/solana/balance` endpoint with a known devnet address

## Support

For issues or questions about:
- **Solana CLI**: https://docs.solana.com/cli
- **This API wrapper**: See project documentation

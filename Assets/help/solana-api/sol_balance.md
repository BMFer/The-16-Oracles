# solana balance

Get your balance

## Usage

```bash
solana balance [FLAGS] [OPTIONS] [ACCOUNT_ADDRESS]
```

## Flags

| Flag | Description |
|------|-------------|
| `-h, --help` | Prints help information |
| `--lamports` | Display balance in lamports instead of SOL |
| `--no-address-labels` | Do not use address labels in the output |
| `--skip-preflight` | Skip the preflight check when sending transactions |
| `--skip-seed-phrase-validation` | Skip validation of seed phrases. Use this if your phrase does not use the BIP39 official English word list |
| `--use-quic` | Use QUIC when sending transactions |
| `--use-tpu-client` | Use TPU client when sending transactions |
| `--use-udp` | Use UDP when sending transactions |
| `-V, --version` | Prints version information |
| `-v, --verbose` | Show additional information |

## Options

| Option | Description |
|--------|-------------|
| `--commitment <COMMITMENT_LEVEL>` | Return information at the selected commitment level<br>**Possible values:** `processed`, `confirmed`, `finalized` |
| `-C, --config <FILEPATH>` | Configuration file to use<br>**Default:** `/home/jhertz/.config/solana/cli/config.yml` |
| `-u, --url <URL_OR_MONIKER>` | URL for Solana's JSON RPC or moniker (or their first letter)<br>**Options:** `mainnet-beta`, `testnet`, `devnet`, `localhost` |
| `-k, --keypair <KEYPAIR>` | Filepath or URL to a keypair |
| `--output <FORMAT>` | Return information in specified output format<br>**Possible values:** `json`, `json-compact` |
| `--ws <URL>` | WebSocket URL for the solana cluster |

## Arguments

### `<ACCOUNT_ADDRESS>` (optional)

Account balance to check. If not provided, uses the default configured keypair. Address can be one of:

- **Base58-encoded public key** - Standard Solana public key format
- **Path to a keypair file** - Local file path to a keypair
- **Hyphen (`-`)** - Signals a JSON-encoded keypair on stdin
- **`ASK` keyword** - To recover a keypair via its seed phrase
- **Hardware wallet keypair URL** - e.g., `usb://ledger`

## Examples

### Check your default wallet balance
```bash
solana balance
```

### Check balance of a specific address
```bash
solana balance 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### Check balance in lamports
```bash
solana balance --lamports
```

### Check balance of a specific address in lamports
```bash
solana balance --lamports 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### Check balance from a keypair file
```bash
solana balance ~/my-wallet.json
```

### Check balance from hardware wallet
```bash
solana balance usb://ledger
```

### Check balance in JSON format
```bash
solana balance --output json
```

### Check balance on testnet
```bash
solana balance -u testnet
```

### Check balance with specific commitment level
```bash
solana balance --commitment finalized 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### Check balance with verbose output
```bash
solana balance -v
```

## Understanding Balance Display

### SOL vs Lamports

- **SOL** - The standard unit of Solana's native token (default display)
- **Lamports** - The smallest unit of SOL
- **Conversion**: 1 SOL = 1,000,000,000 lamports (1 billion lamports)

### Example
```
2.5 SOL = 2,500,000,000 lamports
```

## Commitment Levels

The `--commitment` flag determines how confirmed a transaction must be:

- **`processed`** - Fastest, but least confirmed (may be rolled back)
- **`confirmed`** - Majority of network has confirmed (recommended for most use cases)
- **`finalized`** - Highest level of certainty (may take a few seconds longer)

## Notes

- If no address is specified, the command uses the keypair configured in your Solana CLI config
- Balance queries require network connectivity to fetch current state
- Use `--lamports` when you need exact precision, especially for programs or when dealing with small amounts
- The balance shown is the total SOL held by the account, not including any tokens (SPL tokens)
- For production applications, it's recommended to use `--commitment finalized` to ensure the most accurate balance

## Related Commands

- `solana address` - Get your public key/address
- `solana account` - View detailed account information
- `solana transfer` - Transfer SOL to another address
- `solana airdrop` - Request SOL from a faucet (testnet/devnet only)

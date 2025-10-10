# solana account

Show the contents of an account

## Usage

```bash
solana account [FLAGS] [OPTIONS] <ACCOUNT_ADDRESS>
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
| `-o, --output-file <FILEPATH>` | Write the account data to this file |
| `--output <FORMAT>` | Return information in specified output format<br>**Possible values:** `json`, `json-compact` |
| `--ws <URL>` | WebSocket URL for the solana cluster |

## Arguments

### `<ACCOUNT_ADDRESS>` (required)

Account contents to show. Address can be one of:

- **Base58-encoded public key** - Standard Solana public key format
- **Path to a keypair file** - Local file path to a keypair
- **Hyphen (`-`)** - Signals a JSON-encoded keypair on stdin
- **`ASK` keyword** - To recover a keypair via its seed phrase
- **Hardware wallet keypair URL** - e.g., `usb://ledger`

## Examples

### View account with base58 address
```bash
solana account 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### View account balance in lamports
```bash
solana account --lamports 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### Export account data to file
```bash
solana account -o account_data.json 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### View account from keypair file
```bash
solana account ~/my-keypair.json
```

### View account in JSON format
```bash
solana account --output json 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### View account with verbose output
```bash
solana account -v 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### View account from hardware wallet
```bash
solana account usb://ledger
```

## Notes

- The account command shows detailed information about any Solana account including balance, owner, data, and executable status
- Use `--lamports` flag to see the exact balance in lamports (1 SOL = 1,000,000,000 lamports)
- The `-o` flag is useful for exporting account data for further analysis or backup
- For real-time updates, consider using the `--commitment` flag with appropriate level

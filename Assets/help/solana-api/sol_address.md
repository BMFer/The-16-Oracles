# solana address

Get your public key

## Usage

```bash
solana address [FLAGS] [OPTIONS]
```

## Flags

| Flag | Description |
|------|-------------|
| `--confirm-key` | Confirm key on device; only relevant if using remote wallet |
| `-h, --help` | Prints help information |
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

## Examples

### Get your default wallet address
```bash
solana address
```

### Get address from specific keypair file
```bash
solana address -k ~/my-wallet.json
```

### Get address from hardware wallet
```bash
solana address -k usb://ledger
```

### Get address with confirmation on hardware device
```bash
solana address -k usb://ledger --confirm-key
```

### Get address in JSON format
```bash
solana address --output json
```

### Get address with verbose output
```bash
solana address -v
```

## Notes

- This command displays the public key (address) associated with your configured keypair
- By default, it uses the keypair specified in your Solana CLI configuration file
- Use `-k` to specify a different keypair file if you want to check addresses for other wallets
- The `--confirm-key` flag is useful when using hardware wallets (like Ledger) to verify the address on the device screen
- The address displayed is in base58-encoded format, which is the standard Solana address format
- This command does not require network connectivity as it only reads the public key from your local keypair

## Related Commands

- `solana balance` - Check the SOL balance of an address
- `solana account` - View detailed account information
- `solana config get` - View your current CLI configuration including the default keypair path

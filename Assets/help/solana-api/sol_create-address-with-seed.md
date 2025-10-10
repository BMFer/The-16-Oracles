# solana create-address-with-seed

Generate a derived account address with a seed. For program derived addresses (PDAs), use the `find-program-derived-address` command instead.

## Overview

This command creates a deterministic address derived from a base public key and a seed string. Unlike PDAs (Program Derived Addresses), these addresses are publicly derivable and don't require the program to sign for them.

## Usage

```bash
solana create-address-with-seed [FLAGS] [OPTIONS] <SEED_STRING> <PROGRAM_ID>
```

## Flags

| Flag | Description |
|------|-------------|
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
| `--from <FROM_PUBKEY>` | From (base) key (default: cli config keypair)<br>See address formats below |
| `-u, --url <URL_OR_MONIKER>` | URL for Solana's JSON RPC or moniker (or their first letter)<br>**Options:** `mainnet-beta`, `testnet`, `devnet`, `localhost` |
| `-k, --keypair <KEYPAIR>` | Filepath or URL to a keypair |
| `--output <FORMAT>` | Return information in specified output format<br>**Possible values:** `json`, `json-compact` |
| `--ws <URL>` | WebSocket URL for the solana cluster |

### `--from` Address Formats

The base address can be one of:
- **Base58-encoded public key** - Standard Solana public key format
- **Path to a keypair file** - Local file path to a keypair
- **Hyphen (`-`)** - Signals a JSON-encoded keypair on stdin
- **`ASK` keyword** - To recover a keypair via its seed phrase
- **Hardware wallet keypair URL** - e.g., `usb://ledger`

## Arguments

### `<SEED_STRING>` (required)

The seed string used to derive the address.

**Constraints:**
- Must not take more than 32 bytes to encode as UTF-8
- Deterministic: same seed + base key = same derived address
- Can be any UTF-8 string within the size limit

### `<PROGRAM_ID>` (required)

The program ID that the address will ultimately be used for. Can be:

- **Program public key** - Base58-encoded program address
- **`NONCE`** - Keyword for nonce accounts
- **`STAKE`** - Keyword for stake accounts
- **`VOTE`** - Keyword for vote accounts

## Examples

### Create address with seed for a stake account
```bash
solana create-address-with-seed "my-stake-1" STAKE
```

### Create address with seed for a nonce account
```bash
solana create-address-with-seed "my-nonce" NONCE
```

### Create address with seed for a vote account
```bash
solana create-address-with-seed "validator-vote-1" VOTE
```

### Create address with custom program ID
```bash
solana create-address-with-seed "my-seed" TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA
```

### Create address from specific base key
```bash
solana create-address-with-seed --from 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p "my-seed" STAKE
```

### Create address from keypair file
```bash
solana create-address-with-seed --from ~/my-wallet.json "custom-seed" NONCE
```

### Create address in JSON format
```bash
solana create-address-with-seed --output json "my-seed" STAKE
```

### Create address with verbose output
```bash
solana create-address-with-seed -v "my-seed" STAKE
```

## How It Works

The derived address is calculated using:

```
derived_address = hash(base_pubkey + seed + program_id)
```

Where:
- **base_pubkey** - The base public key (from `--from` or default keypair)
- **seed** - Your seed string (max 32 bytes UTF-8)
- **program_id** - The program that will own the account

## Use Cases

### Multiple Accounts from Single Keypair

Create multiple deterministic accounts from a single base keypair:

```bash
solana create-address-with-seed "stake-1" STAKE
solana create-address-with-seed "stake-2" STAKE
solana create-address-with-seed "stake-3" STAKE
```

Each generates a unique address while using the same base key.

### Organized Account Management

Use descriptive seeds for better account organization:

```bash
solana create-address-with-seed "mainnet-validator-2024" VOTE
solana create-address-with-seed "emergency-nonce" NONCE
solana create-address-with-seed "staking-pool-winter" STAKE
```

### Recoverable Addresses

Since addresses are deterministically derived, you can recreate them anytime with the same inputs:
- Same base key + same seed + same program = same address
- No need to save the derived address separately

## Derived Addresses vs PDAs

| Feature | Derived Address (this command) | Program Derived Address (PDA) |
|---------|-------------------------------|-------------------------------|
| **Command** | `create-address-with-seed` | `find-program-derived-address` |
| **On-curve** | Yes (has private key theoretically) | No (guaranteed no private key) |
| **Signing** | User signs with base key | Program signs via CPI |
| **Use case** | User-controlled derived accounts | Program-controlled accounts |
| **Discovery** | Publicly derivable | Requires bump seed search |

## When to Use This Command

✅ **Use `create-address-with-seed` when:**
- You want multiple accounts derived from one keypair
- You need deterministic, publicly derivable addresses
- Creating stake/vote/nonce accounts with organized naming
- You control the base keypair and will sign transactions

❌ **Don't use this for PDAs - use `find-program-derived-address` when:**
- You need program-controlled accounts
- The program needs to sign (via CPI)
- Building smart contract state accounts

## Notes

- The derived address is calculated locally and doesn't require network connectivity
- The same seed + base key + program ID will always produce the same derived address
- The seed string is case-sensitive
- Maximum seed length: 32 bytes when UTF-8 encoded
- The derived address doesn't automatically exist on-chain; you still need to create/fund the account
- You maintain control through the base keypair; you'll sign with it to interact with derived accounts

## Creating the Account

After generating a derived address, you need to create the actual account on-chain:

```bash
# 1. Generate the address
DERIVED_ADDR=$(solana create-address-with-seed "my-stake" STAKE)

# 2. Create the stake account at that address
solana create-stake-account-checked $DERIVED_ADDR 1.0 --seed "my-stake"
```

## Related Commands

- `solana find-program-derived-address` - Generate PDAs for program-controlled accounts
- `solana create-stake-account` - Create a stake account
- `solana create-nonce-account` - Create a nonce account
- `solana create-vote-account` - Create a vote account
- `solana address` - Get your public key

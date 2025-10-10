# solana address-lookup-table

Address lookup table management

## Overview

Address Lookup Tables (ALTs) are a Solana feature that allows transactions to reference more accounts efficiently by storing frequently-used addresses in an on-chain table. This reduces transaction size and costs when interacting with multiple accounts.

## Usage

```bash
solana address-lookup-table [FLAGS] [OPTIONS] <SUBCOMMAND>
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
| `-u, --url <URL_OR_MONIKER>` | URL for Solana's JSON RPC or moniker (or their first letter)<br>**Options:** `mainnet-beta`, `testnet`, `devnet`, `localhost` |
| `-k, --keypair <KEYPAIR>` | Filepath or URL to a keypair |
| `--output <FORMAT>` | Return information in specified output format<br>**Possible values:** `json`, `json-compact` |
| `--ws <URL>` | WebSocket URL for the solana cluster |

## Subcommands

### `create`
Create a new lookup table

```bash
solana address-lookup-table create [OPTIONS]
```

### `extend`
Append more addresses to an existing lookup table

```bash
solana address-lookup-table extend <LOOKUP_TABLE_ADDRESS> <ADDRESSES>...
```

### `get`
Display information about a lookup table

```bash
solana address-lookup-table get <LOOKUP_TABLE_ADDRESS>
```

### `freeze`
Permanently freezes a lookup table (prevents further modifications)

```bash
solana address-lookup-table freeze <LOOKUP_TABLE_ADDRESS>
```

### `deactivate`
Permanently deactivates a lookup table (must be done before closing)

```bash
solana address-lookup-table deactivate <LOOKUP_TABLE_ADDRESS>
```

### `close`
Permanently closes a lookup table and recovers rent

```bash
solana address-lookup-table close <LOOKUP_TABLE_ADDRESS>
```

### `help`
Prints help message or the help of the given subcommand(s)

```bash
solana address-lookup-table help [SUBCOMMAND]
```

## Workflow

### Typical Lifecycle of an Address Lookup Table

1. **Create** - Initialize a new lookup table
2. **Extend** - Add addresses to the table (can be done multiple times)
3. **Use** - Reference the table in transactions to save space
4. **Freeze** (optional) - Make the table immutable
5. **Deactivate** - Mark the table for closure
6. **Close** - Reclaim rent after deactivation period

## Examples

### Create a new lookup table
```bash
solana address-lookup-table create
```

### Add addresses to a lookup table
```bash
solana address-lookup-table extend <LOOKUP_TABLE_ADDRESS> \
  7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p \
  9aE476sH92Vz7DMPyq5WLPkrKWivxeuTKEFKd2sZZcde
```

### View lookup table contents
```bash
solana address-lookup-table get <LOOKUP_TABLE_ADDRESS>
```

### View lookup table in JSON format
```bash
solana address-lookup-table get <LOOKUP_TABLE_ADDRESS> --output json
```

### Freeze a lookup table
```bash
solana address-lookup-table freeze <LOOKUP_TABLE_ADDRESS>
```

### Deactivate a lookup table
```bash
solana address-lookup-table deactivate <LOOKUP_TABLE_ADDRESS>
```

### Close a lookup table and recover rent
```bash
solana address-lookup-table close <LOOKUP_TABLE_ADDRESS>
```

## Notes

- **Address Lookup Tables** reduce transaction size by allowing you to reference up to 256 addresses using a single byte index
- **Rent recovery**: You can recover the rent paid for the lookup table by closing it after deactivation
- **Deactivation period**: After deactivating a table, you must wait for a deactivation period before you can close it
- **Frozen tables**: Once frozen, a lookup table cannot be extended or modified, but can still be used in transactions
- **Authority**: You must be the authority of the lookup table to extend, freeze, deactivate, or close it
- **Transaction efficiency**: Using lookup tables can significantly reduce transaction costs for programs that interact with many accounts

## Benefits

- **Reduced transaction size** - Reference many accounts with minimal bytes
- **Lower fees** - Smaller transactions cost less
- **Improved composability** - Enables more complex DeFi interactions within transaction limits
- **Gas optimization** - Particularly useful for programs with many account dependencies

## Related Commands

- `solana account` - View account details
- `solana transfer` - Transfer SOL between accounts
- `solana program` - Program management commands

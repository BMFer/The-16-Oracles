# solana find-program-derived-address

Generate a program derived account address with a seed

## Overview

Program Derived Addresses (PDAs) are special addresses that are controlled by programs rather than users. PDAs are "off-curve" addresses (they don't have corresponding private keys), which allows programs to programmatically sign for them using Cross-Program Invocation (CPI).

## Usage

```bash
solana find-program-derived-address [FLAGS] [OPTIONS] <PROGRAM_ID> [SEED]...
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

## Arguments

### `<PROGRAM_ID>` (required)

The program ID that will control the PDA. Can be:

- **Program public key** - Base58-encoded program address
- **`NONCE`** - Keyword for nonce program
- **`STAKE`** - Keyword for stake program
- **`VOTE`** - Keyword for vote program

### `<SEED>...` (optional, multiple allowed)

Seeds used to derive the PDA. Each seed must match the pattern `PREFIX:VALUE`.

#### Seed Prefixes

| Prefix | Description | Example |
|--------|-------------|---------|
| `string` | UTF-8 string | `string:metadata` |
| `pubkey` | Base58-encoded public key | `pubkey:7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p` |
| `hex` | Hexadecimal bytes | `hex:deadbeef` |
| `u8` | Unsigned 8-bit integer | `u8:42` |

#### Number Seed Prefixes

Format: `[u,i][16,32,64,128][le,be]`

- **`[u,i]`** - Unsigned or signed integer
- **`[16,32,64,128]`** - Bit length
- **`[le,be]`** - Little endian or big endian byte order

**Examples:**
- `u64le:1000` - Unsigned 64-bit little-endian
- `i32be:42` - Signed 32-bit big-endian
- `u16le:255` - Unsigned 16-bit little-endian
- `u128le:999999` - Unsigned 128-bit little-endian

## Examples

### Find PDA with string seed
```bash
solana find-program-derived-address TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA string:metadata
```

### Find PDA with multiple seeds
```bash
solana find-program-derived-address TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  string:metadata \
  pubkey:7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p
```

### Find PDA with number seed
```bash
solana find-program-derived-address TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  string:vault \
  u64le:1
```

### Find PDA for stake program
```bash
solana find-program-derived-address STAKE string:my-stake-account
```

### Find PDA with hex seed
```bash
solana find-program-derived-address TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  hex:deadbeef \
  string:config
```

### Find PDA with mixed seed types
```bash
solana find-program-derived-address TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  string:escrow \
  pubkey:7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p \
  u64le:42 \
  u8:1
```

### Find PDA in JSON format
```bash
solana find-program-derived-address --output json \
  TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  string:metadata
```

### Find PDA with verbose output
```bash
solana find-program-derived-address -v \
  TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  string:config
```

## How PDAs Work

### Address Derivation

PDAs are derived using:

```
pda_address = hash(seeds + program_id + bump_seed)
```

The algorithm:
1. Starts with bump seed = 255
2. Hashes seeds + program_id + bump
3. If resulting address is on-curve (has a private key), decrements bump and tries again
4. Returns first off-curve address found (guaranteed to have no private key)

### Bump Seed

The command automatically finds the canonical bump seed (the first valid one, starting from 255 and counting down). This bump seed is crucial for the program to later sign for the PDA.

## Common Use Cases

### Token Metadata Accounts

```bash
# Metaplex metadata PDA for an NFT
solana find-program-derived-address metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s \
  string:metadata \
  pubkey:TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  pubkey:<MINT_ADDRESS>
```

### Associated Token Accounts

```bash
# ATA for a specific owner and mint
solana find-program-derived-address ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL \
  pubkey:<OWNER_ADDRESS> \
  pubkey:TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA \
  pubkey:<MINT_ADDRESS>
```

### Program State/Config Accounts

```bash
# Program configuration PDA
solana find-program-derived-address <YOUR_PROGRAM_ID> \
  string:config
```

### Vault/Escrow Accounts

```bash
# Escrow vault PDA
solana find-program-derived-address <YOUR_PROGRAM_ID> \
  string:vault \
  pubkey:<USER_ADDRESS> \
  u64le:123
```

## PDAs vs Derived Addresses

| Feature | PDA (this command) | Derived Address |
|---------|-------------------|-----------------|
| **Command** | `find-program-derived-address` | `create-address-with-seed` |
| **Has private key** | No (off-curve) | Theoretically yes (on-curve) |
| **Who signs** | Program (via CPI) | User (with base keypair) |
| **Bump seed** | Required (found automatically) | Not applicable |
| **Control** | Program-controlled | User-controlled |
| **Use case** | Smart contract state | User's derived accounts |
| **Seed format** | Typed (string, pubkey, numbers, hex) | Simple UTF-8 string |

## When to Use PDAs

✅ **Use PDAs when:**
- Building program state accounts
- Creating accounts the program needs to sign for
- Implementing program-controlled vaults or escrows
- Storing per-user data in your program
- Creating deterministic program-owned accounts
- Building DeFi protocols, NFT programs, or DAOs

❌ **Don't use PDAs for:**
- Simple user-controlled accounts (use `create-address-with-seed` instead)
- Accounts that don't need program signing

## Understanding the Output

The command returns:
1. **PDA Address** - The derived address
2. **Bump Seed** - The canonical bump (usually needed in your program code)

Example output:
```
PDA: 8VwKYvSfCKr7f9WbKdj8FprZGjVwPHzVKb4b5qK8aFqQ
Bump: 254
```

## Notes

- PDAs are calculated locally and don't require network connectivity
- The same seeds + program ID will always produce the same PDA (deterministic)
- Seeds are case-sensitive and order-dependent
- Programs must use the same seeds to sign for the PDA via `invoke_signed`
- The bump seed returned is the canonical bump (highest valid bump, starting from 255)
- PDAs are guaranteed to not have a private key, making them safe for program control
- Maximum total seed length: 32 bytes per seed, multiple seeds allowed

## Program Integration

When using PDAs in your Solana program (Rust):

```rust
// Finding PDA in program code
let (pda, bump) = Pubkey::find_program_address(
    &[
        b"metadata",
        mint_pubkey.as_ref(),
    ],
    program_id,
);

// Signing with PDA
invoke_signed(
    &instruction,
    &account_infos,
    &[&[b"metadata", mint_pubkey.as_ref(), &[bump]]],
)?;
```

## Related Commands

- `solana create-address-with-seed` - Create user-controlled derived addresses
- `solana account` - View account details
- `solana address` - Get your public key
- `solana program` - Program management commands

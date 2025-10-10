# Solana CLI Reference

**Version:** solana-cli 2.2.21 (src:23e01995; feat:3073396398, client:Agave)

> Blockchain, Rebuilt for Scale

## Usage

```bash
solana [FLAGS] [OPTIONS] <SUBCOMMAND>
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

### Account & Balance Management

- **`account`** - Show the contents of an account
- **`address`** - Get your public key
- **`address-lookup-table`** - Address lookup table management
- **`balance`** - Get your balance
- **`transfer`** - Transfer funds between system accounts
- **`create-address-with-seed`** - Generate a derived account address with a seed. For program derived addresses (PDAs), use the `find-program-derived-address` command instead
- **`find-program-derived-address`** - Generate a program derived account address with a seed

### Airdrop & Faucet

- **`airdrop`** - Request SOL from a faucet

### Nonce Account Management

- **`create-nonce-account`** - Create a nonce account
- **`nonce`** - Get the current nonce value
- **`nonce-account`** - Show the contents of a nonce account
- **`new-nonce`** - Generate a new nonce, rendering the existing nonce useless
- **`authorize-nonce-account`** - Assign account authority to a new entity
- **`withdraw-from-nonce-account`** - Withdraw SOL from the nonce account
- **`upgrade-nonce-account`** - One-time idempotent upgrade of legacy nonce versions in order to bump them out of chain blockhash domain

### Stake Account Management

- **`create-stake-account`** - Create a stake account
- **`create-stake-account-checked`** - Create a stake account, checking the withdraw authority as a signer
- **`stake-account`** - Show the contents of a stake account
- **`delegate-stake`** - Delegate stake to a vote account
- **`deactivate-stake`** - Deactivate the delegated stake from the stake account
- **`split-stake`** - Duplicate a stake account, splitting the tokens between the two
- **`merge-stake`** - Merges one stake account into another
- **`withdraw-stake`** - Withdraw the unstaked SOL from the stake account
- **`stake-authorize`** - Authorize a new signing keypair for the given stake account
- **`stake-authorize-checked`** - Authorize a new signing keypair for the given stake account, checking the authority as a signer
- **`stake-set-lockup`** - Set Lockup for the stake account
- **`stake-set-lockup-checked`** - Set Lockup for the stake account, checking the new authority as a signer
- **`stake-history`** - Show the stake history
- **`stake-minimum-delegation`** - Get the stake minimum delegation amount
- **`stakes`** - Show stake account information

### Vote Account Management

- **`create-vote-account`** - Create a vote account
- **`vote-account`** - Show the contents of a vote account
- **`close-vote-account`** - Close a vote account and withdraw all funds remaining
- **`vote-authorize-voter`** - Authorize a new vote signing keypair for the given vote account
- **`vote-authorize-voter-checked`** - Authorize a new vote signing keypair for the given vote account, checking the new authority as a signer
- **`vote-authorize-withdrawer`** - Authorize a new withdraw signing keypair for the given vote account
- **`vote-authorize-withdrawer-checked`** - Authorize a new withdraw signing keypair for the given vote account, checking the new authority as a signer
- **`vote-update-commission`** - Update the vote account's commission
- **`vote-update-validator`** - Update the vote account's validator identity
- **`withdraw-from-vote-account`** - Withdraw lamports from a vote account into a specified account

### Program Management

- **`program`** - Program management
- **`program-v4`** - Program V4 management

### Block & Slot Information

- **`block`** - Get a confirmed block
- **`block-height`** - Get current block height
- **`block-production`** - Show information about block production
- **`block-time`** - Get estimated production time of a block
- **`slot`** - Get current slot
- **`first-available-block`** - Get the first available block in the storage
- **`live-slots`** - Show information about the current slot progression

### Epoch Information

- **`epoch`** - Get current epoch
- **`epoch-info`** - Get information about the current epoch

### Cluster Information

- **`cluster-date`** - Get current cluster date, computed from genesis creation time and network time
- **`cluster-version`** - Get the version of the cluster entrypoint
- **`genesis-hash`** - Get the genesis hash
- **`gossip`** - Show the current gossip network nodes
- **`supply`** - Get information about the cluster supply of SOL
- **`inflation`** - Show inflation information
- **`largest-accounts`** - Get addresses of largest cluster accounts

### Validator Information

- **`validators`** - Show summary information about the current validators
- **`validator-info`** - Publish/get Validator info on Solana
- **`catchup`** - Wait for a validator to catch up to the cluster
- **`wait-for-max-stake`** - Wait for the max stake of any one node to drop below a percentage of total
- **`leader-schedule`** - Display leader schedule

### Transaction Management

- **`confirm`** - Confirm transaction by signature
- **`transaction-count`** - Get current transaction count
- **`transaction-history`** - Show historical transactions affecting the given address from newest to oldest
- **`decode-transaction`** - Decode a serialized transaction
- **`logs`** - Stream transaction logs
- **`ping`** - Submit transactions sequentially
- **`recent-prioritization-fees`** - Get recent prioritization fees

### Cryptographic Operations

- **`sign-offchain-message`** - Sign off-chain message
- **`verify-offchain-signature`** - Verify off-chain message signature
- **`resolve-signer`** - Checks that a signer is valid, and returns its specific path; useful for signers that may be specified generally, eg. usb://ledger

### Utilities

- **`config`** - Solana command-line tool configuration settings
- **`completion`** - Generate completion scripts for various shells
- **`feature`** - Runtime feature management
- **`rent`** - Calculate rent-exempt-minimum value for a given account data field length
- **`help`** - Prints this message or the help of the given subcommand(s)

## Common Examples

### Check balance
```bash
solana balance
```

### Transfer SOL
```bash
solana transfer <RECIPIENT_ADDRESS> <AMOUNT>
```

### Check cluster
```bash
solana cluster-version
```

### Get current epoch
```bash
solana epoch-info
```

### View validator information
```bash
solana validators
```

### Request airdrop (testnet/devnet only)
```bash
solana airdrop 1
```

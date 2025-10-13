# spl-token gc

Cleanup unnecessary token accounts

## Usage

```bash
spl-token gc [OPTIONS]
```

## Options

### `-C, --config <PATH>`

Configuration file to use

### `--close-empty-associated-accounts`

Close all empty associated token accounts (to get SOL back)

### `--fee-payer <KEYPAIR>`

Specify the fee-payer account. This may be a keypair file, the ASK keyword or the pubkey of an offline signer, provided an appropriate --signer argument is also passed. Defaults to the client keypair.

### `-h, --help`

Print help information

### `--output <FORMAT>`

Return information in specified output format

**Possible values:** `json`, `json-compact`

### `--owner <OWNER_KEYPAIR>`

Keypair of the primary authority controlling a mint or account. Defaults to the client keypair.

### `-p, --program-id <ADDRESS>`

SPL Token program id

### `--program-2022`

Use token extension program token 2022 with program id:
`TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb`

### `-u, --url <URL_OR_MONIKER>`

URL for Solana's JSON RPC or moniker (or their first letter): `[mainnet-beta, testnet, devnet, localhost]`

Default from the configuration file.

### `-v, --verbose`

Show additional information

### `--with-compute-unit-limit <COMPUTE-UNIT-LIMIT>`

Set compute unit limit for transaction, in compute units.

### `--with-compute-unit-price <COMPUTE-UNIT-PRICE>`

Set compute unit price for transaction, in increments of 0.000001 lamports per compute unit.

## Examples

```bash
# Close all empty associated token accounts for the owner
spl-token gc --close-empty-associated-accounts

# Cleanup accounts for a specific owner
spl-token gc --close-empty-associated-accounts --owner <OWNER_KEYPAIR>

# Run with verbose output to see which accounts are being closed
spl-token gc --close-empty-associated-accounts --verbose

# Output as JSON
spl-token gc --close-empty-associated-accounts --output json
```

## Notes

- This command helps recover rent-exempt SOL from empty token accounts
- Only closes accounts with zero balance
- Requires the `--close-empty-associated-accounts` flag to perform actual cleanup
- Without this flag, the command may only show what would be cleaned up
- The rent SOL is returned to the account owner

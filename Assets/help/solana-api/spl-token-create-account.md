# spl-token create-account

Create a new token account

## Usage

```bash
spl-token create-account [OPTIONS] <TOKEN_MINT_ADDRESS> [ACCOUNT_KEYPAIR]
```

## Arguments

### `<TOKEN_MINT_ADDRESS>`

The token that the account will hold

### `<ACCOUNT_KEYPAIR>`

Specify the account keypair. This may be a keypair file or the ASK keyword. [default: associated token account for --owner]

## Options

### `-C, --config <PATH>`

Configuration file to use

### `--fee-payer <KEYPAIR>`

Specify the fee-payer account. This may be a keypair file, the ASK keyword or the pubkey of an offline signer, provided an appropriate --signer argument is also passed. Defaults to the client keypair.

### `-h, --help`

Print help information

### `--immutable`

Lock the owner of this token account from ever being changed

### `--nonce <PUBKEY>`

Provide the nonce account to use when creating a nonced transaction. Nonced transactions are useful when a transaction requires a lengthy signing process. Learn more about nonced transactions at https://docs.solanalabs.com/cli/examples/durable-nonce

### `--nonce-authority <KEYPAIR>`

Provide the nonce authority keypair to use when signing a nonced transaction

### `--output <FORMAT>`

Return information in specified output format

**Possible values:** `json`, `json-compact`

### `--owner <OWNER_ADDRESS>`

Address of the primary authority controlling a mint or account. Defaults to the client keypair address.

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
# Create an associated token account for the owner
spl-token create-account <TOKEN_MINT_ADDRESS>

# Create a token account with a specific keypair
spl-token create-account <TOKEN_MINT_ADDRESS> <ACCOUNT_KEYPAIR>

# Create an immutable token account (owner cannot be changed)
spl-token create-account <TOKEN_MINT_ADDRESS> --immutable

# Create a token account for a different owner
spl-token create-account <TOKEN_MINT_ADDRESS> --owner <OWNER_ADDRESS>

# Output as JSON
spl-token create-account <TOKEN_MINT_ADDRESS> --output json
```

## Notes

- If no account keypair is specified, an associated token account (ATA) will be created
- Associated token accounts are deterministically derived from the owner and mint addresses
- Creating an account requires SOL for rent exemption
- Use `--immutable` to prevent the account owner from ever being changed

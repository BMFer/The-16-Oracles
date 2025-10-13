# spl-token balance

Get token account balance

## Usage

```bash
spl-token balance [OPTIONS] [TOKEN_MINT_ADDRESS]
```

## Arguments

### `<TOKEN_MINT_ADDRESS>`

Token of associated account. To query a specific account, use the `--address` parameter instead

## Options

### `--address <TOKEN_ACCOUNT_ADDRESS>`

Specify the token account to query [default: owner's associated token account]

### `-C, --config <PATH>`

Configuration file to use

### `--fee-payer <KEYPAIR>`

Specify the fee-payer account. This may be a keypair file, the ASK keyword or the pubkey of an offline signer, provided an appropriate --signer argument is also passed. Defaults to the client keypair.

### `-h, --help`

Print help information

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
# Get balance of owner's associated token account for a specific token
spl-token balance <TOKEN_MINT_ADDRESS>

# Get balance of a specific token account address
spl-token balance --address <TOKEN_ACCOUNT_ADDRESS>

# Get balance for a different owner's associated token account
spl-token balance <TOKEN_MINT_ADDRESS> --owner <OWNER_ADDRESS>

# Output as JSON
spl-token balance <TOKEN_MINT_ADDRESS> --output json
```

# spl-token address

Get wallet address

## Usage

```bash
spl-token address [OPTIONS]
```

## Options

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

Return the associated token address for the given owner. [Default: return the associated token address for the client keypair]

### `-p, --program-id <ADDRESS>`

SPL Token program id

### `--program-2022`

Use token extension program token 2022 with program id:
`TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb`

### `--token <TOKEN_MINT_ADDRESS>`

Return the associated token address for the given token. [Default: return the client keypair address]

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
# Get the client keypair address
spl-token address

# Get the associated token address for a specific token
spl-token address --token <TOKEN_MINT_ADDRESS>

# Get the associated token address for a specific owner and token
spl-token address --owner <OWNER_ADDRESS> --token <TOKEN_MINT_ADDRESS>

# Output as JSON
spl-token address --output json
```

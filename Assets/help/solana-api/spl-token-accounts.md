# spl-token accounts

List all token accounts by owner

## Usage

```bash
spl-token accounts [OPTIONS] [TOKEN_MINT_ADDRESS]
```

## Arguments

### `<TOKEN_MINT_ADDRESS>`

Limit results to the given token. [Default: list accounts for all tokens]

## Options

### `--addresses-only`

Print token account addresses only

### `-C, --config <PATH>`

Configuration file to use

### `--delegated`

Limit results to accounts with transfer delegations

### `--externally-closeable`

Limit results to accounts with external close authorities

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
# List all token accounts for the current owner
spl-token accounts

# List accounts for a specific token mint
spl-token accounts <TOKEN_MINT_ADDRESS>

# List only addresses (no balances or details)
spl-token accounts --addresses-only

# List only delegated accounts
spl-token accounts --delegated

# List accounts for a specific owner
spl-token accounts --owner <OWNER_ADDRESS>

# Output as JSON
spl-token accounts --output json
```

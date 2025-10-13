# spl-token close

Close a token account

## Usage

```bash
spl-token close [OPTIONS] [--] [TOKEN_MINT_ADDRESS]
```

## Arguments

### `<TOKEN_MINT_ADDRESS>`

Token of the associated account to close. To close a specific account, use the `--address` parameter instead

## Options

### `--address <TOKEN_ACCOUNT_ADDRESS>`

Specify the token account to close [default: owner's associated token account]

### `--blockhash <BLOCKHASH>`

Use the supplied blockhash

### `-C, --config <PATH>`

Configuration file to use

### `--close-authority <KEYPAIR>`

Specify the token's close authority if it has one, otherwise specify the token's owner keypair. This may be a keypair file or the ASK keyword. Defaults to the client keypair.

### `--dump-transaction-message`

Display the base64 encoded binary transaction message in sign-only mode

### `--fee-payer <KEYPAIR>`

Specify the fee-payer account. This may be a keypair file, the ASK keyword or the pubkey of an offline signer, provided an appropriate --signer argument is also passed. Defaults to the client keypair.

### `-h, --help`

Print help information

### `--multisig-signer [<MULTISIG_SIGNER>...]`

Member signer of a multisig account

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

### `--recipient <REFUND_ACCOUNT_ADDRESS>`

The address of the account to receive remaining SOL [default: --owner]

### `--sign-only`

Sign the transaction offline

### `--signer <PUBKEY=SIGNATURE>`

Provide a public-key/signature pair for the transaction

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
# Close owner's associated token account for a specific token
spl-token close <TOKEN_MINT_ADDRESS>

# Close a specific token account
spl-token close --address <TOKEN_ACCOUNT_ADDRESS>

# Close account and send remaining SOL to a different recipient
spl-token close <TOKEN_MINT_ADDRESS> --recipient <RECIPIENT_ADDRESS>

# Close account with a close authority
spl-token close <TOKEN_MINT_ADDRESS> --close-authority <KEYPAIR>

# Output as JSON
spl-token close <TOKEN_MINT_ADDRESS> --output json
```

## Notes

- Closing an account returns the rent-exempt SOL to the recipient (defaults to the owner)
- The account must have a zero balance before it can be closed
- If the account has a close authority set, you must provide it with `--close-authority`

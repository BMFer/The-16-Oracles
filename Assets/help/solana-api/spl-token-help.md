# spl-token CLI Reference

**Version:** 5.3.0

SPL-Token Command-line Utility

## Usage

```bash
spl-token [OPTIONS] <SUBCOMMAND>
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

### `-V, --version`

Print version information

### `--with-compute-unit-limit <COMPUTE-UNIT-LIMIT>`

Set compute unit limit for transaction, in compute units.

### `--with-compute-unit-price <COMPUTE-UNIT-PRICE>`

Set compute unit price for transaction, in increments of 0.000001 lamports per compute unit.

## Subcommands

### Account Management

#### `accounts`

List all token accounts by owner

#### `address`

Get wallet address

#### `balance`

Get token account balance

#### `create-account`

Create a new token account

#### `close`

Close a token account

#### `gc`

Cleanup unnecessary token accounts

### Token Operations

#### `create-token`

Create a new token

#### `mint`

Mint new tokens

#### `burn`

Burn tokens from an account

#### `transfer`

Transfer tokens between accounts

#### `supply`

Get token supply

#### `close-mint`

Close a token mint

### Token Delegation

#### `approve`

Approve a delegate for a token account

#### `revoke`

Revoke a delegate's authority

### Token Authority

#### `authorize`

Authorize a new signing keypair to a token or token account

### Multisig

#### `create-multisig`

Create a new account describing an M:N multisignature

### Freeze/Thaw Operations

#### `freeze`

Freeze a token account

#### `thaw`

Thaw a token account

#### `pause`

Pause mint, burn, and transfer

#### `resume`

Resume mint, burn, and transfer

### Confidential Transfers

#### `configure-confidential-transfer-account`

Configure confidential transfers for token account

#### `deposit-confidential-tokens`

Deposit amounts for confidential transfers

#### `withdraw-confidential-tokens`

Withdraw amounts for confidential transfers

#### `apply-pending-balance`

Collect confidential tokens from pending to available balance

#### `enable-confidential-credits`

Enable confidential transfers for token account. To enable confidential transfers for the first time, use `configure-confidential-transfer-account` instead.

#### `disable-confidential-credits`

Disable confidential transfers for token account

#### `enable-non-confidential-credits`

Enable non-confidential transfers for token account.

#### `disable-non-confidential-credits`

Disable non-confidential transfers for token account

#### `update-confidential-transfer-settings`

Update confidential transfer configuration for a token

### Native SOL Wrapping

#### `wrap`

Wrap native SOL in a SOL token account

#### `unwrap`

Unwrap a SOL token account

#### `sync-native`

Sync a native SOL token account to its underlying lamports

### Extensions & Features

#### `enable-required-transfer-memos`

Enable required transfer memos for token account

#### `disable-required-transfer-memos`

Disable required transfer memos for token account

#### `enable-cpi-guard`

Enable CPI Guard for token account

#### `disable-cpi-guard`

Disable CPI Guard for token account

#### `set-transfer-fee`

Set the transfer fee for a token with a configured transfer fee

#### `withdraw-withheld-tokens`

Withdraw withheld transfer fee tokens from mint and / or account(s)

#### `withdraw-excess-lamports`

Withdraw lamports from a Token Program owned account

#### `set-interest-rate`

Set the interest rate for an interest-bearing token

#### `set-transfer-hook`

Set the transfer hook program id for a token

### Metadata

#### `initialize-metadata`

Initialize metadata extension on a token mint

#### `update-metadata`

Update metadata on a token mint that has the extension

#### `update-metadata-address`

Updates metadata pointer address for the mint. Requires the metadata pointer extension.

### Group Management

#### `initialize-group`

Initialize group extension on a token mint

#### `initialize-member`

Initialize group member extension on a token mint

#### `update-group-address`

Updates group pointer address for the mint. Requires the group pointer extension.

#### `update-group-max-size`

Updates the maximum number of members for a group.

#### `update-member-address`

Updates group member pointer address for the mint. Requires the group member pointer extension.

### Other

#### `display`

Query details of an SPL Token mint, account, or multisig by address

#### `update-default-account-state`

Updates default account state for the mint. Requires the default account state extension.

#### `update-ui-amount-multiplier`

Update UI multiplier

#### `bench`

Token benchmarking facilities

#### `help`

Print this message or the help of the given subcommand(s)

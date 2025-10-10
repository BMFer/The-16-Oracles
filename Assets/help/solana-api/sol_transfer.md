# solana transfer

Transfer funds between system accounts

## Usage

```bash
solana transfer [FLAGS] [OPTIONS] <RECIPIENT_ADDRESS> <AMOUNT>
```

## Flags

| Flag | Description |
|------|-------------|
| `--allow-unfunded-recipient` | Complete the transfer even if the recipient address is not funded |
| `--dump-transaction-message` | Display the base64 encoded binary transaction message in sign-only mode |
| `-h, --help` | Prints help information |
| `--no-address-labels` | Do not use address labels in the output |
| `--no-wait` | Return signature immediately after submitting the transaction, instead of waiting for confirmations |
| `--sign-only` | Sign the transaction offline |
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
| `--blockhash <BLOCKHASH>` | Use the supplied blockhash |
| `--commitment <COMMITMENT_LEVEL>` | Return information at the selected commitment level<br>**Possible values:** `processed`, `confirmed`, `finalized` |
| `--with-compute-unit-price <COMPUTE-UNIT-PRICE>` | Set compute unit price for transaction, in increments of 0.000001 lamports per compute unit |
| `-C, --config <FILEPATH>` | Configuration file to use<br>**Default:** `/home/jhertz/.config/solana/cli/config.yml` |
| `--fee-payer <KEYPAIR>` | Specify the fee-payer account. This may be a keypair file, the ASK keyword or the pubkey of an offline signer, provided an appropriate `--signer` argument is also passed. Defaults to the client keypair |
| `--from <FROM_ADDRESS>` | Source account of funds (default: cli config keypair)<br>See address formats below |
| `-u, --url <URL_OR_MONIKER>` | URL for Solana's JSON RPC or moniker (or their first letter)<br>**Options:** `mainnet-beta`, `testnet`, `devnet`, `localhost` |
| `-k, --keypair <KEYPAIR>` | Filepath or URL to a keypair |
| `--with-memo <MEMO>` | Specify a memo string to include in the transaction |
| `--nonce <PUBKEY>` | Provide the nonce account to use when creating a nonced transaction. Nonced transactions are useful when a transaction requires a lengthy signing process. Learn more at [Solana Docs](https://docs.solanalabs.com/cli/examples/durable-nonce) |
| `--nonce-authority <KEYPAIR>` | Provide the nonce authority keypair to use when signing a nonced transaction |
| `--output <FORMAT>` | Return information in specified output format<br>**Possible values:** `json`, `json-compact` |
| `--signer <PUBKEY=SIGNATURE>...` | Provide a public-key/signature pair for the transaction |
| `--ws <URL>` | WebSocket URL for the solana cluster |

## Arguments

### `<RECIPIENT_ADDRESS>` (required)

Account of recipient. Address can be one of:

- **Base58-encoded public key** - Standard Solana public key format
- **Path to a keypair file** - Local file path to a keypair
- **Hyphen (`-`)** - Signals a JSON-encoded keypair on stdin
- **`ASK` keyword** - To recover a keypair via its seed phrase
- **Hardware wallet keypair URL** - e.g., `usb://ledger`

### `<AMOUNT>` (required)

The amount to send, in SOL. Accepts the keyword `ALL` to transfer the entire balance.

## Examples

### Transfer SOL to an address
```bash
solana transfer 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 1.5
```

### Transfer all SOL from your wallet
```bash
solana transfer 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p ALL
```

### Transfer from a specific keypair
```bash
solana transfer --from ~/my-wallet.json 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 2.0
```

### Transfer with a memo
```bash
solana transfer --with-memo "Payment for services" 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 5.0
```

### Transfer to an unfunded recipient
```bash
solana transfer --allow-unfunded-recipient 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 0.1
```

### Transfer without waiting for confirmation
```bash
solana transfer --no-wait 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 1.0
```

### Transfer with custom fee payer
```bash
solana transfer --fee-payer ~/fee-payer.json 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 3.0
```

### Transfer with priority fee (compute unit price)
```bash
solana transfer --with-compute-unit-price 1000 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 2.5
```

### Transfer on testnet
```bash
solana transfer -u testnet 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 0.5
```

### Offline signing (sign-only mode)
```bash
solana transfer --sign-only --blockhash <RECENT_BLOCKHASH> 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 1.0
```

### Transfer with verbose output
```bash
solana transfer -v 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 1.0
```

### Transfer in JSON format
```bash
solana transfer --output json 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p 1.0
```

## Advanced Features

### Using the ALL Keyword

The `ALL` keyword transfers your entire balance, accounting for transaction fees:

```bash
solana transfer 7EqQdEUC3hHPbEJXc6Jq3xLPxPvXZvFxGzXKBMZ3wS8p ALL
```

**Note:** This will leave a small amount for rent exemption if needed.

### Memos

Memos are on-chain messages attached to transactions. They're useful for:
- Payment references
- Invoice numbers
- Notes or descriptions
- Exchange deposit identifiers

```bash
solana transfer --with-memo "Invoice #12345" <ADDRESS> 10.0
```

### Priority Fees (Compute Unit Price)

During network congestion, you can increase transaction priority by paying higher compute unit prices:

```bash
solana transfer --with-compute-unit-price 5000 <ADDRESS> 1.0
```

- Higher values = higher priority
- Values are in micro-lamports per compute unit
- Useful during high network activity

### Durable Nonces

Durable nonces allow transactions to be signed now and submitted later without expiration:

```bash
solana transfer --nonce <NONCE_ACCOUNT> --nonce-authority ~/nonce-authority.json <ADDRESS> 1.0
```

Benefits:
- No blockhash expiration (transactions don't expire after ~2 minutes)
- Useful for multi-signature workflows
- Enables offline/cold wallet signing with flexible submission timing

### Offline Signing

For cold wallets or air-gapped systems:

1. Create and sign the transaction offline:
```bash
solana transfer --sign-only --blockhash <BLOCKHASH> <ADDRESS> 1.0
```

2. Submit the signed transaction from an online machine:
```bash
solana transfer --signer <PUBKEY=SIGNATURE> <ADDRESS> 1.0
```

## Transaction Fees

- **Base fee**: ~0.000005 SOL (5,000 lamports) per signature
- **Priority fees**: Optional, set with `--with-compute-unit-price`
- **Rent exemption**: Minimum balance required for new accounts (~0.00089088 SOL)

## Notes

- By default, transfers use the keypair configured in your Solana CLI config as the sender
- Use `--from` to specify a different source account
- The `--allow-unfunded-recipient` flag is needed when transferring to a brand new address that has never received SOL
- Without `--no-wait`, the command will wait for transaction confirmation before returning
- For production transfers, consider using `--commitment finalized` to ensure the transaction is fully confirmed
- Memos are publicly visible on-chain and cannot be encrypted or deleted

## Safety Tips

⚠️ **Before transferring large amounts:**
1. Test with a small amount first
2. Verify the recipient address carefully (double-check every character)
3. Consider using `--verbose` to see detailed transaction information
4. Use `--commitment finalized` for critical transfers
5. Save the transaction signature for your records

## Related Commands

- `solana balance` - Check account balance
- `solana account` - View account details
- `solana confirm` - Confirm a transaction by signature
- `solana address` - Get your public key
- `solana airdrop` - Request SOL from a faucet (testnet/devnet only)

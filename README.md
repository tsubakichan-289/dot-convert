# dot-convert

A small Avalonia UI app written in F# that converts between plaintext and an AES-encrypted string encoded as Unicode Braille dot characters.

## Features
- Live conversion between plaintext and encrypted dot text.
- AES-CBC encryption/decryption with a user-supplied key.
- Import/export for both plaintext and encrypted text.
- Automatic line breaks every 16 dot characters for readability.

## Requirements
- .NET SDK 9.0

## Build and run
```bash
dotnet run --project dot-convert.fsproj
```

## Usage
1. Enter a key and click Confirm to lock it in.
2. Type into Plaintext to generate encrypted dot text.
3. Paste dot text into Encrypted Output to decrypt back to plaintext.
4. Use Import/Save to load or write files for either field.

## Notes
- Conversion is disabled until the key is confirmed.
- The encrypted output uses Unicode Braille dot characters and may not render properly in all fonts.
- This app does not provide a CUI/CLI workflow.
- This project is intended as a sample/utility; review the cryptography choices if you plan to use it for sensitive data.

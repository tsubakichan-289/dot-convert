# dot-convert

A small CLI tool that encrypts/decrypts text by converting it to Braille dot strings and using AES-CBC.

## Requirements

- .NET SDK 9.0

## Usage

```
# Encrypt: text -> dot
 dotnet run -- enc input.txt output.dot

# Decrypt: dot -> text
 dotnet run -- dec input.dot output.txt
```

## Example

```
 dotnet run -- enc sample.txt sample.dot
 dotnet run -- dec sample.dot sample.dec.txt
```

## Notes

- Input/output are UTF-8.
- Dot strings insert a newline every 16 characters.
- The key is a fixed string in `Convert.fs`, hashed with SHA-256.
- The IV is fixed; AES-CBC with PKCS7 padding is used.

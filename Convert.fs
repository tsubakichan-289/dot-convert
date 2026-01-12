namespace Cryptosystem

open System
open System.IO
open System.Text

open AES
module Convert =

    // Hex character to decimal value; expects uppercase input.
    let hexToDec : char -> int
        = fun c ->
            match c with
            | '0' -> 0
            | '1' -> 1
            | '2' -> 2
            | '3' -> 3
            | '4' -> 4
            | '5' -> 5
            | '6' -> 6
            | '7' -> 7
            | '8' -> 8
            | '9' -> 9
            | 'A' -> 10
            | 'B' -> 11
            | 'C' -> 12
            | 'D' -> 13
            | 'E' -> 14
            | 'F' -> 15
            | _   -> 0

    // Convert two hex characters into a single value used by dot encoding.
    let twoHexDec : char -> char -> int
        = fun hex1 hex2 ->
            let fst = hexToDec hex1
            let snd = hexToDec hex2
            let part1 = if snd < 8 then snd * 8 else ((snd - 8) * 8 + 128)
            let part2 = if fst < 8 then fst else (fst + 56)
            part1 + part2

    // Insert newlines every 16 characters for readability/storage.
    let insertNewlinesEvery16 : string -> string
        = fun s ->
            let rec chunks acc i =
                if i >= s.Length then List.rev acc
                else 
                    let len = if i + 16 <= s.Length then 16 else s.Length - i
                    let chunk = s.Substring(i, len)
                    chunks (chunk :: acc) (i + 16)
            String.concat "\n" (chunks [] 0)

    // Convert a byte array to an uppercase hex string.
    let byteArrayToHexString : byte[] -> string
        = fun bytes ->
            bytes |> Array.map (fun b -> sprintf "%02X" b) |> String.concat ""

    // Convert a hex string to a byte array.
    let hexStringToByteArray : string -> byte[] 
        = fun hex ->
            let length = hex.Length / 2
            Array.init length (fun i ->
                let substring = hex.Substring(i * 2, 2)
                Byte.Parse(substring, Globalization.NumberStyles.HexNumber)
            )

    // Convert a hex string to a Braille dot string.
    let hexStringToDotString : string -> string
        = fun hex ->
            let zero = '\u2800'
            let sb = StringBuilder()
            for i in 0 .. 2 .. (hex.Length - 2) do
                let a = hex.[i]
                let b = hex.[i + 1]
                let value = twoHexDec b a
                let c = char ((int zero) + value)
                sb.Append(c) |> ignore
            sb.ToString()

    // Encrypt and encode a string as Braille dots.
    let stringToDotString : string -> string
        = fun s ->
            let keyString = "289CH4n_"
            let encrypted = AES.encryptMessage s keyString
            let hexStr = byteArrayToHexString encrypted
            let dotStr = hexStringToDotString hexStr
            insertNewlinesEvery16 dotStr

    // Compute a weighted sum for dot decoding.
    let weightedSum : int list -> int
        = fun lst ->
            let weights = [1; 2; 4; 16; 32; 64; 8; 128]
            let effectiveWeights = weights |> List.take (List.length lst)
            List.zip lst effectiveWeights
            |> List.sumBy (fun (a, w) -> a * w)

    // Convert an integer to a list of binary digits.
    let intToBinList : int -> int list
        = fun n ->
            Convert.ToString(n, 2)
            |> Seq.map (fun c -> if c = '1' then 1 else 0)
            |> Seq.toList

    // Convert a dot string into a list of numeric values.
    let dotStringToNumList : string -> int list
        = fun s ->
            let zero = '\u2800'
            s |> Seq.map (fun c -> int c - int zero) |> Seq.toList

    // Remove newline separators from a string.
    let removeNewlines : string -> string
        = fun s ->
            s.Replace("\n", "")

    // Convert a list of integers to an uppercase hex string.
    let intListToHexString : int list -> string
        = fun intList ->
            intList |> List.map (fun i -> sprintf "%02X" i) |> String.concat ""

    // Convert a dot string back to a hex string.
    let dotStringToHexString : string -> string
        = fun dotStr ->
            let clean = removeNewlines dotStr
            let numList = dotStringToNumList clean
            let hexVals =
                numList |> List.map (fun num ->
                    let binList = intToBinList num |> List.rev
                    weightedSum binList
                )
            intListToHexString hexVals

    // Decode a dot string to the original plaintext.
    let dotStringToString (dotStr: string) : string =
        let hexStr = dotStringToHexString dotStr
        let keyString = "289CH4n_"
        let byteArray = hexStringToByteArray hexStr
        AES.decryptMessage byteArray keyString

    // Write a string to disk using dot encoding.
    let writeStrToFile (path: string) (content: string) : unit =
        let dotContent = stringToDotString content
        let fileInfo = FileInfo(path)
        let dir = fileInfo.Directory
        if not (dir.Exists) then
            dir.Create()
        File.WriteAllText(path, dotContent, Encoding.UTF8)

    // Read a dot-encoded file and return plaintext.
    let readStrFromFile (path: string) : string =
        let content = File.ReadAllText(path, Encoding.UTF8)
        dotStringToString content

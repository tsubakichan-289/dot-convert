open System
open System.IO
open System.Text
open Cryptosystem.Convert

let private printUsage () =
    printfn "Usage:"
    printfn "  dot-convert enc <input> <output>"
    printfn "  dot-convert dec <input> <output>"

[<EntryPoint>]
let main args =
    match args with
    | [| "enc"; inputPath; outputPath |] ->
        let content = File.ReadAllText(inputPath, Encoding.UTF8)
        writeStrToFile outputPath content
        0
    | [| "dec"; inputPath; outputPath |] ->
        let content = readStrFromFile inputPath
        File.WriteAllText(outputPath, content, Encoding.UTF8)
        0
    | _ ->
        printUsage ()
        1

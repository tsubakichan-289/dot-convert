namespace DotConvert

open System
open System.IO
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage
open Cryptosystem

type MainWindow () as this = 
    inherit Window ()

    let mutable isSyncing = false
    let mutable lastEdited = ""
    let mutable confirmedKey = ""
    let mutable isKeyConfirmed = false

    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)

    member private this.PlainTextChanged(_sender: obj, _args: TextChangedEventArgs) =
        if isSyncing then
            ()
        else
            let input = this.FindControl<TextBox>("PlainInput")
            let output = this.FindControl<TextBox>("EncryptedOutput")
            if isNull input || isNull output then
                ()
            else
                lastEdited <- "plain"
                let text = if isNull input.Text then "" else input.Text
                let encoded =
                    if String.IsNullOrEmpty text || String.IsNullOrEmpty confirmedKey || not isKeyConfirmed then ""
                    else
                        let encrypted = AES.encryptMessage text confirmedKey
                        let hexStr = Cryptosystem.Convert.byteArrayToHexString encrypted
                        let dotStr = Cryptosystem.Convert.hexStringToDotString hexStr
                        Cryptosystem.Convert.insertNewlinesEvery16 dotStr
                isSyncing <- true
                output.Text <- encoded
                isSyncing <- false

    member private this.EncryptedTextChanged(_sender: obj, _args: TextChangedEventArgs) =
        if isSyncing then
            ()
        else
            let input = this.FindControl<TextBox>("EncryptedOutput")
            let output = this.FindControl<TextBox>("PlainInput")
            if isNull input || isNull output then
                ()
            else
                lastEdited <- "encrypted"
                let text = if isNull input.Text then "" else input.Text
                if String.IsNullOrWhiteSpace text then
                    isSyncing <- true
                    output.Text <- ""
                    isSyncing <- false
                else
                    try
                        if not (String.IsNullOrEmpty confirmedKey) && isKeyConfirmed then
                            let hexStr = Cryptosystem.Convert.dotStringToHexString text
                            let bytes = Cryptosystem.Convert.hexStringToByteArray hexStr
                            let decoded = AES.decryptMessage bytes confirmedKey
                            isSyncing <- true
                            output.Text <- decoded
                            isSyncing <- false
                    with
                    | _ -> ()

    member private this.KeyConfirmChecked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        if isSyncing then
            ()
        else
            let plainInput = this.FindControl<TextBox>("PlainInput")
            let encryptedInput = this.FindControl<TextBox>("EncryptedOutput")
            let keyInput = this.FindControl<TextBox>("KeyInput")
            if isNull plainInput || isNull encryptedInput || isNull keyInput then
                ()
            else
                confirmedKey <- if isNull keyInput.Text then "" else keyInput.Text
                isKeyConfirmed <- not (String.IsNullOrEmpty confirmedKey)
                keyInput.IsEnabled <- not isKeyConfirmed
                if String.IsNullOrEmpty confirmedKey then
                    ()
                else if lastEdited = "encrypted" then
                    let text = if isNull encryptedInput.Text then "" else encryptedInput.Text
                    if not (String.IsNullOrWhiteSpace text) then
                        try
                            let hexStr = Cryptosystem.Convert.dotStringToHexString text
                            let bytes = Cryptosystem.Convert.hexStringToByteArray hexStr
                            let decoded = AES.decryptMessage bytes confirmedKey
                            isSyncing <- true
                            plainInput.Text <- decoded
                            isSyncing <- false
                        with
                        | _ -> ()
                else
                    let text = if isNull plainInput.Text then "" else plainInput.Text
                    let encoded =
                        if String.IsNullOrEmpty text then ""
                        else
                            let encrypted = AES.encryptMessage text confirmedKey
                            let hexStr = Cryptosystem.Convert.byteArrayToHexString encrypted
                            let dotStr = Cryptosystem.Convert.hexStringToDotString hexStr
                            Cryptosystem.Convert.insertNewlinesEvery16 dotStr
                    isSyncing <- true
                    encryptedInput.Text <- encoded
                    isSyncing <- false

    member private this.KeyConfirmUnchecked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        let keyInput = this.FindControl<TextBox>("KeyInput")
        if not (isNull keyInput) then
            isKeyConfirmed <- false
            keyInput.IsEnabled <- true

    member private this.PlainImportClicked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        async {
            let options = FilePickerOpenOptions()
            options.AllowMultiple <- false
            let! result = this.StorageProvider.OpenFilePickerAsync(options) |> Async.AwaitTask
            if not (isNull result) && result.Count > 0 then
                let! content = File.ReadAllTextAsync(result.[0].Path.LocalPath) |> Async.AwaitTask
                let input = this.FindControl<TextBox>("PlainInput")
                if not (isNull input) then
                    isSyncing <- true
                    input.Text <- content
                    isSyncing <- false
        } |> Async.StartImmediate

    member private this.PlainSaveClicked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        async {
            let options = FilePickerSaveOptions()
            let! result = this.StorageProvider.SaveFilePickerAsync(options) |> Async.AwaitTask
            if not (isNull result) then
                let input = this.FindControl<TextBox>("PlainInput")
                let text = if isNull input || isNull input.Text then "" else input.Text
                do! File.WriteAllTextAsync(result.Path.LocalPath, text) |> Async.AwaitTask
        } |> Async.StartImmediate

    member private this.EncryptedImportClicked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        async {
            let options = FilePickerOpenOptions()
            options.AllowMultiple <- false
            let! result = this.StorageProvider.OpenFilePickerAsync(options) |> Async.AwaitTask
            if not (isNull result) && result.Count > 0 then
                let! content = File.ReadAllTextAsync(result.[0].Path.LocalPath) |> Async.AwaitTask
                let output = this.FindControl<TextBox>("EncryptedOutput")
                if not (isNull output) then
                    isSyncing <- true
                    output.Text <- content
                    isSyncing <- false
        } |> Async.StartImmediate

    member private this.EncryptedSaveClicked(_sender: obj, _args: Avalonia.Interactivity.RoutedEventArgs) =
        async {
            let options = FilePickerSaveOptions()
            let! result = this.StorageProvider.SaveFilePickerAsync(options) |> Async.AwaitTask
            if not (isNull result) then
                let output = this.FindControl<TextBox>("EncryptedOutput")
                let text = if isNull output || isNull output.Text then "" else output.Text
                do! File.WriteAllTextAsync(result.Path.LocalPath, text) |> Async.AwaitTask
        } |> Async.StartImmediate

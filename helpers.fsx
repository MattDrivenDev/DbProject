#r @"tools/FAKE/tools/FakeLib.dll"

open System
open System.IO
open System.Text
open Fake

/// <summary>
/// Context for Success/Failure of activities.
/// </summary>
type Result<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure
    
/// <summary>
/// Binds the output of one Success/Failure to another.
/// </summary>
let bind f =
    function input ->
             match input with
             | Success a -> f a
             | Failure b -> Failure b
    
/// <summary>
/// 'bind' as an operator.
/// </summary>
let (>>=) x f = bind f x

/// <summary>
/// Wrapped-String-Type that represents a valid semantic version.
/// </summary>
type SemanticVersion = SemanticVersion of string
    
/// <summary>
/// True if a given char exists in an array of valid chars.
/// </summary>
let ExpectedChar validChars c = validChars |> Array.exists ((=)c)

/// <summary>
/// Converts an array into an optional array, based on if it is empty or not.
/// </summary>
let MaybeArray xs = if Array.isEmpty xs then None else Some xs

/// <summary>
/// Try's to open and read a file as raw text (string)
/// </summary>
let TryReadFile filename = 
    match File.Exists(filename) with
    | true ->
        let reader = FileInfo(filename).OpenText()
        Success (reader.ReadToEnd())
    | false -> Failure (sprintf "Error, '%s' does not exist" filename)

/// <summary>
/// Try's to get name of all the sub-directories, provided a path
/// </summary>
let TrySubDirectories path = 
    match Directory.Exists(path) with
    | true -> 
        DirectoryInfo(path).GetDirectories()
        |> Array.map (fun dir -> dir.Name)
        |> MaybeArray
        |> Success
    | false -> Failure (sprintf "Error, '%s' does not exist" path)

/// <summary>
/// True if a given char exists in an array of valid chars.
/// </summary>
let ContainsOnlyExpectedChars validChars str = 
    (str:string).ToCharArray() 
    |> Array.map (ExpectedChar validChars)
    |> Array.exists ((=)false)
    |> not

/// <summary>
/// Try's to create a SemanticVersion from a given string.
/// </summary>
let TrySemanticVersion str = 
    let validChars = [|'0';'1';'2';'3';'4';'5';'6';'7';'8';'9';'.';|]
    match ContainsOnlyExpectedChars validChars str with
    | true -> Success (SemanticVersion str)
    | false -> Failure (sprintf "Error, '%s' is not a valid semantic version" str)
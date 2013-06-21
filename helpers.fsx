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
/// Prefixes a given string with a specified prefix to a maximum length.
/// </summary>
let rec PrefixString prefix max str = 
    if (str:string).Length >= max then str
    else
        PrefixString prefix max (sprintf "%s%s" prefix str)
    
/// <summary>
/// True if a given char exists in an array of valid chars.
/// </summary>
let ExpectedChar validChars c = validChars |> Array.exists ((=)c)

/// <summary>
/// Converts an array into an optional array, based on if it is empty or not.
/// </summary>
let MaybeArray xs = if Array.isEmpty xs then None else Some xs

/// <summary>
/// Try's to create a new directory at a specified path.
/// </summary>
let TryMakeDirectory path = 
    try
        match Directory.Exists(path) with
        | false ->
            ignore (Directory.CreateDirectory(path))
            Success path
        | true ->
            Failure (sprintf "Error, '%s' already exists" path)
    with
    | ex -> Failure ex.Message

/// <summary>
/// Try's to open and read a file as raw text (string)
/// </summary>
let TryReadFile filename = 
    match File.Exists(filename) with
    | true ->
        let reader = FileInfo(filename).OpenText()
        Success (reader.ReadToEnd())
    | false -> Failure (sprintf "Error, '%s' does not exist" filename)

let TryWriteFile filename contents = 
    try
        failwith "Error, 'TryWriteFile' isn't implemented yet."
    with
    | ex -> Failure ex.Message

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

/// <summary>
/// Try's to make a string from a number with a specified length.
/// </summary>
let TryMakeNumberString length n =
    let nStr = n.ToString()
    match nStr.Length <= length with
    | true -> 
        Success (PrefixString "0" 4 nStr)
    | false -> 
        Failure (sprintf "Error, '%i' is too large a number to make into a '%i' length string" n length)

type SqlTemplate = { TemplateContent: string; ScriptBlock: string }

type SqlScript = { ScriptContent: string; ScriptName: string; }

type SqlTransform = {
    DbName      : string
    Version     : SemanticVersion
    Timestamp   : DateTime
    MachineName : string
    UserName    : string
    Scripts     : SqlScript array
}

let GetScriptBlockIndices (templateContent:string) = 
    try
        let scriptBlockBeginIndex   = templateContent.IndexOf("$foreach_script_begin$")
        let scriptBlockContentIndex = templateContent.IndexOf("$script_content$")
        let scriptBlockEndIndex     = templateContent.IndexOf("$foreach_script_end$")
        Success (scriptBlockBeginIndex, scriptBlockContentIndex, scriptBlockEndIndex)
    with
    | ex -> Failure ex.Message

let EnsureScriptBlockExists templateContent = 
    let help = "Error, SqlTemplate does not contain all 3 '$foreach_script_begin$', '$foreach_script_end$' with an '$script_content$' inbetween."
    let ensure (beginIndex, contentIndex, endIndex) =        
        if beginIndex >=0 && contentIndex >= 0 && endIndex >= 0 
        then Success templateContent else Failure help

    GetScriptBlockIndices templateContent >>= ensure

let EnsureScriptBlocksInCorrectOrder templateContent =     
    let ensure (beginIndex, contentIndex, endIndex) = 
        let map = beginIndex < contentIndex, contentIndex < endIndex
        match map with
        | true, true   -> Success templateContent
        | false, true  -> Failure "Error, '$foreach_script_begin$' must come before '$script_content$'"
        | true, false  -> Failure "Error, '$script_content$' must come before '$foreach_script_end$'"
        | false, false -> Failure "Error, you've really messed up the template!"

    GetScriptBlockIndices templateContent >>= ensure

let ValidateSqlTemplate templateContent = 
    EnsureScriptBlockExists templateContent
    >>= EnsureScriptBlocksInCorrectOrder 

let ParseSqlTemplate templateContent = 
    Failure "Error, 'ParseSqlTemplate' isn't implemented yet."

let ApplySqlTransform transform template = 
    Failure "Error, 'ApplySqlTransform' isn't implemented yet."
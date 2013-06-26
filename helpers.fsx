#r @"tools/FAKE/tools/FakeLib.dll"

open System
open System.IO
open System.Text
open Fake

/// <summary>
/// Context for Success/Failure of activities.
/// </summary>
let failStepWithTrace step error =
    traceError error
    trace ""
    failwith (sprintf "Error in '%s'" step)

/// <summary>
/// Context for Success/Failure of activities.
/// </summary>
type Result<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure
    
/// <summary>
/// Binds a ('a -> Result<'b, 'c>) -> ('a -> Result<'b, 'c>)
/// </summary>
let bind f =
    function input ->
             match input with
             | Success a -> f a
             | Failure b -> Failure b

let replace oldValue newValue str = (str:string).Replace(oldValue=oldValue, newValue=newValue)
    
/// <summary>
/// 'bind' as an operator.
/// </summary>
let (>>=) x f = bind f x

/// <summary>
/// Wrapped-String-Type that represents a valid semantic version.
/// </summary>
type SemanticVersion = SemanticVersion of string

let SemanticVersionStr (SemanticVersion version) = version

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

let doesntExist path = Failure (sprintf "Error, '%s' doesn't exist" path)    

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
        let content = reader.ReadToEnd()
        reader.Close()
        Success (content)
    | false -> filename |> doesntExist

let TryWriteFile filename (contents:string) = 
    try
        use writer = new StreamWriter(path=filename)
        writer.Write(contents, true)
        writer.Flush()
        writer.Close() 

        Success filename
    with
    | ex -> Failure ex.Message

/// <summary>
/// Try's to get a filtered list of sub-directories at a given path.
/// </summary>
let TrySubDirectoriesWithFilter (filter: DirectoryInfo -> bool) path = 
    match Directory.Exists(path) with
    | true -> 
        DirectoryInfo(path).GetDirectories()
        |> Array.filter filter
        |> Array.map (fun dir -> dir.Name)        
        |> MaybeArray
        |> Success
    | false -> path |> doesntExist

/// <summary>
/// Try's to get a list of all sub-directories at a given path.
/// </summary>
let TrySubDirectories = TrySubDirectoriesWithFilter (fun d -> true)

/// <summary>
/// Gets all files in a directory (non-recursive) that match a pattern
/// </summary>
let Files pattern path = 
    match Directory.Exists(path) with
    | true -> 
        DirectoryInfo(path).GetFiles(searchPattern=pattern)
        |> Array.map (fun file -> file.Name)
        |> MaybeArray
        |> Success
    | false -> path |> doesntExist

/// <summary>
/// Deletes **all** files in a given path.
/// </summary>
let DeleteAllFiles path =
    match Files "*.*" path with
    | Success maybeFiles ->
        match maybeFiles with
        | Some files ->
            files |> Array.iter (fun file -> Fake.FileHelper.DeleteFile (sprintf "%s/%s" path file))
            Success (sprintf "Done, %i files deleted." (files |> Array.length))
        | None ->
            Success "Done, but there were no files to be deleted."
    | Failure error -> Failure error

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

/// <summary>
/// Type to represent a SQL deploy script template.
/// </summary>
type SqlTemplate = { TemplateContent: string; ScriptBlock: string }

/// <summary>
/// Type to represent a developed SQL script ready for deployment.
/// </summary>
type SqlScript = { ScriptContent: string; ScriptName: string; }

/// <summary>
/// Type to represent details of a transformation from a SQL template -> SQL deployment script.
/// </summary>
type SqlTransform = {
    DbName      : string
    Version     : SemanticVersion
    Timestamp   : DateTime
    MachineName : string
    UserName    : string
    Scripts     : SqlScript list
}

/// <summary>
/// Gets each index of each part of the repeatable script block from a SQL template.
/// </summary>
let GetScriptBlockIndices (templateContent:string) = 
    try
        let scriptBlockBeginIndex   = templateContent.IndexOf("$foreach_script_begin$")
        let scriptBlockContentIndex = templateContent.IndexOf("$script_content$")
        let scriptBlockEndIndex     = templateContent.IndexOf("$foreach_script_end$")
        Success (scriptBlockBeginIndex, scriptBlockContentIndex, scriptBlockEndIndex)
    with
    | ex -> Failure ex.Message

/// <summary>
/// Ensures that a SQL template contains all 3 required markers for the repeatable script block.
/// </summary>
let EnsureScriptBlockExists templateContent = 
    let help = "Error, SqlTemplate does not contain all 3 '$foreach_script_begin$', '$foreach_script_end$' with an '$script_content$' inbetween."
    let ensure (beginIndex, contentIndex, endIndex) =        
        if beginIndex >=0 && contentIndex >= 0 && endIndex >= 0 
        then Success templateContent else Failure help

    GetScriptBlockIndices templateContent >>= ensure

/// <summary>
/// Ensures that a SQL template has each of the 3 required markers in the correct order.
/// </summary>
let EnsureScriptBlocksInCorrectOrder templateContent =     
    let ensure (beginIndex, contentIndex, endIndex) = 
        let map = beginIndex < contentIndex, contentIndex < endIndex
        match map with
        | true, true   -> Success templateContent
        | false, true  -> Failure "Error, '$foreach_script_begin$' must come before '$script_content$'"
        | true, false  -> Failure "Error, '$script_content$' must come before '$foreach_script_end$'"
        | false, false -> Failure "Error, you've really messed up the template!"

    GetScriptBlockIndices templateContent >>= ensure

/// <summary>
/// Performs some basic validation on a SQL template.
/// </summary>
let ValidateSqlTemplate templateContent = 
    EnsureScriptBlockExists templateContent
    >>= EnsureScriptBlocksInCorrectOrder 

/// <summary>
/// Turns a raw SQL template (string) into the SqlTemplate type.
/// </summary>
let ParseSqlTemplate templateContent = 
    let scriptBlock (beginIndex, contentIndex, endIndex) =                 
        try
            (templateContent:string).Substring(beginIndex, (endIndex + 20 - beginIndex))
            |> Success
        with
        | ex -> Failure ex.Message        

    match GetScriptBlockIndices templateContent >>= scriptBlock with
    | Success block -> Success ({ TemplateContent = templateContent; ScriptBlock = block; })
    | Failure error -> Failure error

/// <summary>
/// Applies a SqlTransform to a SqlTemplate.
/// </summary>
let ApplySqlTransform transform template = 

    let applyToScript sql = 
        template.ScriptBlock
        |> replace "$foreach_script_begin$" ""
        |> replace "$script_name$" sql.ScriptName
        |> replace "$script_content$" sql.ScriptContent
        |> replace "$foreach_script_end$" ""

    let mergedSql = 
        transform.Scripts
        |> List.map applyToScript
        |> List.fold (fun fullSql sql -> sql + fullSql) ""
            
    let applyTransformToTemplate =     
        replace template.ScriptBlock mergedSql
        >> replace "$DbName$" transform.DbName
        >> replace "$Version$" (SemanticVersionStr transform.Version)
        >> replace "$ReleaseDate$" (transform.Timestamp.ToShortDateString())
        >> replace "$ReleaseMachine$" transform.MachineName
        >> replace "$ReleaseUser$" transform.UserName

    applyTransformToTemplate template.TemplateContent |> Success
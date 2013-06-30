namespace au.id.cxd.Math.Http

open System
open System.IO
open System.Web
open Handler
open AsyncHelper
open Json
open CookieHelper
open Filesystem

module UploadFile =

    /// read the file into the project directory
    let readFile (context:HttpContext) =
        let project = context.Request.["project"]
        let file = context.Request.Files.["dataFile"]
        match file = null with
        | false -> 
            let name = file.FileName
            let dir = projectDir project
            let path = Path.Combine(dir.FullName, name)
            file.SaveAs(path)            
            Some path
        | true -> None 

    /// handle the upload request.
    let processRequest (context:HttpContext) = 
        match context.Request.Files.Count > 0 with
        | true -> 
            let result = readFile context
            match result with
            | Some path ->
                setCookie context "dataFile" path
                makeSuccess "Saved file" 
                |> toString
                |> (writeJsonToContext context)
             | None ->
                makeError "A file must be supplied"
                |> toString
                |> (writeJsonToContext context)
        | false -> 
            makeError "A file must be supplied"
            |> toString
            |> (writeJsonToContext context)
        context.Response.Close()             

/// separate type outside of module.
type UploadFileHandler() =
    inherit HandlerInstance(UploadFile.processRequest)
            
             
    

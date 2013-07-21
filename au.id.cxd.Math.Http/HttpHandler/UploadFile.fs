namespace au.id.cxd.Math.Http

open System
open System.IO
open System.Web
open Handler
open AsyncHelper
open Json
open CookieHelper
open Filesystem
open DataState

module UploadFile =

    type Either<'a> = 
        | Success of 'a
        | Error of string
        | NoResult
        

    /// copy the file into the project directory
    let copyFile (context:HttpContext) =
        let projectName = context.Request.["project"]
        ProjectState.load projectName |> ignore
        let projectOption = ProjectState.currentProject ()
        match projectOption with
        | Some project ->
            match (project.Application.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase)) with
            | false -> Error "The project name does not match"
            | true ->
                let containsHeader = Boolean.Parse( context.Request.["containsHeader"] )
                let file = context.Request.Files.["dataFile"]
                match file = null with
                | false -> 
                    let path = DataState.saveWorkingFile {
                                IncludesHeader = containsHeader;
                                Filename = file.FileName
                                } file.InputStream           
                    Success path
                | true -> Error "The file is not supplied"
        | None -> Error "There is no current project. Please select or create a project"
         

    /// handle the upload request.
    let runProcess (context:HttpContext) (writer:HttpContext -> string -> unit) = 
        match context.Request.Files.Count > 0 with
        | true -> 
            let result = copyFile context
            match result with
            | Success path ->
                setCookie context "dataFile" path
                makeSuccess "Saved file" 
                |> toString
                |> (writer context)
             | Error msg ->
                makeError ("Could not upload file: " + msg)
                |> toString
                |> (writer context)
             | NoResult ->
                makeError "Could not upload file"
                |> toString
                |> (writer context)
        | false -> 
            makeError "A file must be supplied"
            |> toString
            |> (writer context)
        context.Response.Close() 
        
    let processRequest (context:HttpContext) = 
        // specific to dojo framework cross posting.
        let sendWrappedResponse (context:HttpContext) (item:string) =
            writeStringToContext context "text/html" (String.Format("<html><body><textarea>{0}</textarea></body></html>", item))
        let wrapInTextArea = context.Request.["wrapInTextArea"]
        if (not (String.IsNullOrEmpty(wrapInTextArea)) && Boolean.TrueString = wrapInTextArea) then
            runProcess context sendWrappedResponse
        else
            runProcess context writeJsonToContext

/// separate type outside of module.
type UploadFileHandler() =
    inherit HandlerInstance(UploadFile.processRequest)
            
             
    

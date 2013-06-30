namespace au.id.cxd.Math.Http

open System
open System.Web

open Handler
open Json
open AsyncHelper
open ProjectState

module SaveProject =

    /// save the current project that is in the state
    let processRequest(context:HttpContext) =
        let respond = AsyncHelper.writeJsonToContext context
        let name = context.Request.["project"]
        match name with
        | null -> Json.makeError("Project name not provided") |> Json.toString |> respond
        | _ -> 
            let project = ProjectState.currentProject ()
            match project with 
            | None ->
                Json.makeError(String.Format("There is no current project loaded."))
            | Some item ->
                
                Json.makeSuccess(String.Format("Created Project {0}", name)) |> Json.toString |> respond
                
        
/// save current projects
type SaveProjectHandler() =
    inherit HandlerInstance(SaveProject.processRequest)


namespace au.id.cxd.Math.Http

open System
open System.Web

open Handler
open Json
open AsyncHelper
open ProjectState

module LoadProject =

    /// load the project into the current context.
    let loadProject name = 
        ProjectState.load name
        

    /// save the current project that is in the state
    let processRequest(context:HttpContext) =
        let respond = AsyncHelper.writeJsonToContext context
        let name = context.Request.["project"]
        let project = ProjectState.currentProject ()
        match project with 
        | None ->
            let nextproject = loadProject name
            let nextname = nextproject.Application.ProjectName
            Json.makeSuccess(String.Format("Loaded Project {0}", nextname)) |> Json.toString |> respond
        | Some item ->
            let project = ProjectState.saveCurrentProject ()
            match project with
            | None -> Json.makeError("Could not save the current project.") |> Json.toString |> respond
            | Some item ->
                let nextproject = loadProject name
                let nextname = nextproject.Application.ProjectName
                Json.makeSuccess(String.Format("Loaded Project {0}", nextname)) |> Json.toString |> respond
                
        
/// save current projects
type LoadProjectHandler() =
    inherit HandlerInstance(LoadProject.processRequest)



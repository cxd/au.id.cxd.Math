namespace au.id.cxd.Math.Http

open System

open au.id.cxd.Math.Http.Filesystem
open au.id.cxd.Math.Http.Cache
open au.id.cxd.Math.Http.Project
open au.id.cxd.Math.Application

(** application module **)

module ProjectState = 

    let cacheName = "current_project"
    
    let projectFilename = "project.ser"
    
    let cacheProjectList = "project_list"

    (* internal *)
    /// save project to filesystem
    let saveToFilesystem name project = 
        Filesystem.saveToFilesystem name projectFilename project Project.writeRecord
    
    (* internal *)
    let storeInCache name project = 
        Cache.store cacheName project
        project
    
    (* internal *)
    let readFromCache name =
        match Cache.read name with
        | null -> None
        | item -> Some (item.Value :?> ProjectRecordState) 
    
    (* external *)
    let currentProject () = readFromCache cacheName
    
    (* external *)
    /// create a new project
    let create name = 
        let project = new ProjectRecordState()
        project.ProjectName <- name
        saveToFilesystem name project |> ignore
        Cache.remove cacheProjectList |> ignore
        storeInCache cacheName project
       
    (* external *)
    /// delete a project from the filesystem and remove it from cache.
    let delete name =
        Filesystem.removeProjectDir name
        Cache.invalidate ()
          
    /// save the current project  
    let saveCurrentProject () =
        let project = currentProject()
        match project with
        | None -> None
        | Some item ->
            saveToFilesystem item.ProjectName item |> ignore
            Some item
    
    (* internal *)
    let enumerateDirectories () = 
        Filesystem.enumerateProjectDirectories ()
    
    (* internal *)
    let storeNamesInCache names = 
        Cache.store cacheProjectList names
        names
    
    (* external *)
    let retrieveNames () = 
        match Cache.read cacheProjectList with
        | null ->
            enumerateDirectories () 
            |> storeNamesInCache 
        | names -> names.Value :?> string []
            
    
    (* internal *)
    /// load a serialized project record from the filesystem
    let loadFromFilesystem name = 
        Filesystem.readFromFilesystem name projectFilename Project.readRecord
    
    (* external *)
    let load name = 
        let project = loadFromFilesystem name 
        storeInCache cacheName project 

    
    ()

namespace au.id.cxd.Math.Http

open System
open System.IO


(** application module **)

module Filesystem = 

    /// the base working directory.
    let basePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default"), "data")

    /// get the project path
    let projectPath (name:string) = 
        Path.Combine(basePath, name)
        
    /// get the directory for the project
    let projectDir (name:string) =
        let path = projectPath name
        if Directory.Exists(path) then
            new DirectoryInfo(path)
        else 
            Directory.CreateDirectory(path)

    /// remove the project directory and all files beneath it.
    let removeProjectDir (name:string) =
        let path = projectPath name
        if Directory.Exists(path) then
            Directory.Delete(path,true)
        else ()
        

    /// get the path to the file beneath the project directory
    let filePath name filename = Path.Combine((projectDir name).FullName, filename)

    /// save a project to the filesystem
    let saveToFilesystem name filename item (writer:string -> 'a -> unit)  = 
        let fullpath = filePath name filename
        writer fullpath item
    
    /// read a project from the filesystem
    let readFromFilesystem name filename (reader:string -> 'a) = 
        let fullpath = filePath name filename
        reader fullpath
    
    /// enumerate the list of project directories beneath the base directory
    let enumerateProjectDirectories () = 
        (new DirectoryInfo(basePath)).GetDirectories()
        |> Array.map (fun info -> info.Name)
        
    /// process each file with a map function.
    let mapFiles name baseDirectory mapFn =
        (new DirectoryInfo(Path.Combine(Path.Combine(basePath, name), baseDirectory))).GetFiles()
        |> Array.map mapFn  


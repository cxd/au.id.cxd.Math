namespace au.id.cxd.Math.Http

open System.IO
open au.id.cxd.Math.Http.Project
open au.id.cxd.Math

/// the data module handles data assets for projects
/// there is only one data asset per project.
module DataState =

    /// a record that is used to describe the data.
    type DataDescriptor = 
        { 
        IncludesHeader: bool;
        Filename: string;
        TotalRows:int;
        }
        
    /// the data base directory
    let dataBaseDir = "data"
    
    /// the temporary base directory
    let tempBaseDir = "temp"
    
    /// the name used in the cache to store the working file.
    let workingFileNameCache = "WORKING_FILE_NAME"
    
    /// get the temporary file path.
    let tempFilePath filename =
        Filesystem.projectPath tempBaseDir
        |> fun parent -> Path.Combine(parent, filename)
    
    /// save the file as the current working file to the filesystem
    let saveWorkingFile (descriptor:DataDescriptor) (file:Stream) = 
        let writer (path:string) (stream:Stream) = 
            try
                let reader = new StreamReader(stream)
                let data = reader.ReadToEnd()
                let write = new StreamWriter(path)
                write.Write(data)
                write.Flush()
                write.Close()
                true
             with
             | e -> false
        let (flag, path) = Filesystem.saveToFilesystem tempBaseDir descriptor.Filename file writer
        Cache.store workingFileNameCache descriptor
        path
        
    
    /// update the data descriptor.
    let updateWorkingFileDescription (descriptor:DataDescriptor) =
        Cache.store workingFileNameCache descriptor
    
    (* external *)
    /// retrieve the working file from the temporary 
    /// working directory
    let readWorkingFile () = 
        match (Cache.maybeRead workingFileNameCache) with
        | None -> None
        | Some obj ->
            let descriptor = obj :?> DataDescriptor 
            Some (descriptor, (RawData.readFromCsv [|','|] (tempFilePath descriptor.Filename)))
    
    /// read the paged data for n number of records starting from the start page.
    let readPagedFile startpage recordsPerPage =
        match readWorkingFile () with
        | None -> None
        | Some (descriptor, rawData) -> 
            //rawData.
            try 
                let len = Seq.length rawData.RawData
                let data = Seq.skip (startpage*recordsPerPage) rawData.RawData
                Some ({Filename = descriptor.Filename; IncludesHeader = descriptor.IncludesHeader; TotalRows = len}, Seq.take recordsPerPage data)
            with
            // not enough data.
            | e -> None
            
    (* external *)
    /// assign the current working file to the project
    /// this causes the data to be stored in the project instance
    let assignWorkingFileToProject (project:ProjectRecordState) projectWriterFn = 
        let data = readWorkingFile ()
        match data with 
        | None -> false
        | Some (descriptor, data) -> 
            project.Data <- data
            // default the attributes
            project.ClearAttributes() 
            // if the first row contains headers extract the attributes.
            match Cache.maybeRead workingFileNameCache with
            | None -> ()
            | Some obj ->
                let descriptor = obj :?> DataDescriptor
                if (descriptor.IncludesHeader) then
                    project.AssignAttributesFromRawDataHeader ()
            projectWriterFn project
            
    
    
        
    
    ()

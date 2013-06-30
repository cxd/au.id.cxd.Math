namespace au.id.cxd.Math.Http


open System
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters.Binary
open au.id.cxd.Math.Application

module Project =

    /// the container for the project
    /// and any application specific data
    [<Serializable>]
    type ProjectRecord  = {
        Application: ApplicationRecord;
        } 
    
    /// a wrapper class around the record for internal use
    type ProjectRecordState () = 
        
        let application = new ApplicationState()
        
        let mutable project = {
                Application = application.State
            }
        
        member m.Application with get() = application
            
        member m.Project 
            with get() = project
            and set(p) = 
                project <- p
                application.State <- p.Application
        
    // write the project record to filesystem
    let writeRecord filename project =
        let writer = File.OpenWrite(filename)
        let serializer = new BinaryFormatter()
        serializer.Serialize(writer, project)
        writer.Flush()
        writer.Close()
    
    
    /// read the project record from filesystem
    let readRecord filename =
        let reader = File.OpenRead(filename)
        let serializer = new BinaryFormatter()
        let data = serializer.Deserialize(reader)
        reader.Close()
        data :?> ProjectRecord
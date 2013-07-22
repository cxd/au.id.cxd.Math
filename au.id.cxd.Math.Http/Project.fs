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
    [<Serializable>]
    type ProjectRecordState = 
        inherit ApplicationState
        
        
       
        /// access the project 
        member m.Project 
            with get() = { Application = m.State }
            
        
        new() = { inherit ApplicationState() }
        
        new(app:ApplicationRecord, file:String) = { inherit ApplicationState(app,file) }
        
        override b.StoreData(info:SerializationInfo, context:StreamingContext) =
                    base.StoreData(info, context)
        
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
        data :?> ProjectRecordState
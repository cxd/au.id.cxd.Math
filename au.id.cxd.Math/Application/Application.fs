namespace au.id.cxd.Math


open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open au.id.cxd.Math.MathProject
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData

(** application module **)

/// <summary>
/// The state of the ui
/// this is a mutable object 
/// </summary>
module Application = 
        
    type DataPair = option<(Attribute * Attribute)>

        
    /// <summary>
    /// UI application state data
    /// </summary>
    type ApplicationRecord = { 
        ProjectName:string;
        Data: RawDataSet; 
        Attributes: AttributeList;
        ClassColumn: ClassificationAttribute;
        TrainPercent:double;
        IgnoreColumns:int List;
        ReviewPair: DataPair;
        }

    /// <summary>
    /// Save an application state to a binary file
    /// </summary>
    let saveApplicationState (fname:string) appState =
        let writer = File.OpenWrite(fname)
        let serializer = new BinaryFormatter()
        serializer.Serialize(writer, appState)
        writer.Flush()
        writer.Close()

    /// <summary>
    /// Read an application state from a binary file
    /// </summary>
    let readApplicationState file =
        let reader = File.OpenRead(file)
        let serializer = new BinaryFormatter()
        let data = serializer.Deserialize(reader)
        reader.Close()
        data :?> ApplicationRecord
        
          
    /// ui state instance
    type ApplicationState() = 

       let mutable appState = { 
           ProjectName = String.Empty;
           Data = emptyDataSet; 
           Attributes = List.empty;
           ClassColumn = { Label = ""; Column = -1; };
           TrainPercent = 1.0;
           IgnoreColumns = List.empty;
           ReviewPair = None;
           }

       /// <summary>
       /// The file resource associated with the ui.
       /// </summary>
       let mutable fileResource = System.String.Empty
     
       /// <summary>
       /// Save the application state to file.
       /// </summary>
       member b.Save(file) = 
          saveApplicationState file appState
          fileResource <- file

       /// <summary>
       /// Save the application state to the current file.
       /// </summary>
       member b.Save() = 
          if (b.HasFile) then 
             saveApplicationState fileResource appState
          else ()

       /// <summary>
       /// Load the application state from file.
       /// </summary>
       member b.Load(file) =
           appState <- readApplicationState file
           fileResource <- file

       /// <summary>
       /// Load the application state from file.
       /// </summary>
       member b.ReLoad() =
           if (b.HasFile) then
               appState <- readApplicationState fileResource
           else ()


       /// <summary>
       /// determine whether the state has a file associated with it.
       /// </summary>
       member b.HasFile 
           with get() = not (System.String.IsNullOrEmpty(fileResource))
                    
       member b.ProjectName 
        with get() = appState.ProjectName
        and set(n) = appState <- {
               ProjectName = n;
               Data = appState.Data; 
               Attributes = appState.Attributes;
               ClassColumn = appState.ClassColumn;
               TrainPercent = appState.TrainPercent;
               IgnoreColumns = appState.IgnoreColumns;
               ReviewPair = appState.ReviewPair;
               }
            
       /// <summary>
       /// Modify the dual data.
       /// </summary>
       member b.ReviewPair
           with get() = appState.ReviewPair
           and set(p) = appState <- { 
               ProjectName = appState.ProjectName;
               Data = appState.Data; 
               Attributes = appState.Attributes;
               ClassColumn = appState.ClassColumn;
               TrainPercent = appState.TrainPercent;
               IgnoreColumns = appState.IgnoreColumns;
               ReviewPair = p;
               }

    
       /// <summary>
       /// State property.
       /// </summary>
       member b.State with get() = appState and set(v) = appState <- v

        
       /// <summary>
       /// Modify the app state data record.
       /// </summary>
       member b.Data
          with get() = appState.Data 
          and set(d) = appState <- { 
              ProjectName = appState.ProjectName;
              Data = d; 
              Attributes = appState.Attributes;
              ClassColumn = appState.ClassColumn;
              TrainPercent = appState.TrainPercent;
              IgnoreColumns = appState.IgnoreColumns;
              ReviewPair = appState.ReviewPair;
              }
            
       /// <summary>
       /// Modify the attributes.
       /// </summary>
       member b.Attributes
          with get() = appState.Attributes
          and set(a) = appState <- { 
              ProjectName = appState.ProjectName;
              Data = appState.Data; 
              Attributes = a;
              ClassColumn = appState.ClassColumn;
              TrainPercent = appState.TrainPercent;
              IgnoreColumns = appState.IgnoreColumns;
              ReviewPair = appState.ReviewPair;
              }

       /// <summary>
       /// Clear the ignore columns.
       /// </summary>
       member b.ClearIgnoreColumns() = appState <- { 
           ProjectName = appState.ProjectName;
           Data = appState.Data; 
           Attributes = appState.Attributes;
           ClassColumn = appState.ClassColumn;
           TrainPercent = appState.TrainPercent;
           IgnoreColumns = List.empty;
           ReviewPair = appState.ReviewPair;
           }
    
       /// <summary>
       /// Add a column to ignore.
       /// </summary>
       member b.AddIgnoreColumn(c) = appState <- { 
           ProjectName = appState.ProjectName;
           Data = appState.Data; 
           Attributes = appState.Attributes;
           ClassColumn = appState.ClassColumn;
           TrainPercent = appState.TrainPercent;
           IgnoreColumns = c :: appState.IgnoreColumns;
           ReviewPair = appState.ReviewPair;
           }

       /// <summary>
       /// Remove a column from the ignore list.
       /// </summary>
       member b.RemoveIgnoreColumn(c) = appState <- { 
           ProjectName = appState.ProjectName;
           Data = appState.Data; 
           Attributes = appState.Attributes;
           ClassColumn = appState.ClassColumn;
           TrainPercent = appState.TrainPercent;
           IgnoreColumns = List.filter (fun a -> a <> c) appState.IgnoreColumns;
           ReviewPair = appState.ReviewPair;
           }

       /// <summary>
       /// Determine whether the column is found in the ignore list.
       /// </summary>
       member b.IsIgnored(c) = List.exists (fun a -> a = c) appState.IgnoreColumns
            

       /// <summary>
       /// Accessor for ignore columns.
       /// </summary>
       member b.IgnoreColumns with get() = appState.IgnoreColumns            
            
       /// <summary>
       /// Accessor modifier for class column
       /// </summary>
       member b.ClassColumn
          with get() = appState.ClassColumn
          and set(c) = appState <- { 
              ProjectName = appState.ProjectName;
              Data = appState.Data; 
              Attributes = appState.Attributes;
              ClassColumn = c;
              TrainPercent = appState.TrainPercent;
              IgnoreColumns = appState.IgnoreColumns;
              ReviewPair = appState.ReviewPair;
              }
       /// <summary>
       /// Modify the class column.
       /// </summary>
       member b.SetClassColumn(attr:Attribute) = appState <- { 
           ProjectName = appState.ProjectName;
           Data = appState.Data; 
           Attributes = appState.Attributes;
           ClassColumn = { Label = attr.AttributeLabel; Column = attr.Column; }
           TrainPercent = appState.TrainPercent;
           IgnoreColumns = appState.IgnoreColumns;
           ReviewPair = appState.ReviewPair;
           }

       /// <summary>
       /// Accessor modifier for percentage of training data.
       /// </summary>
       member b.TrainPercent
          with get() = appState.TrainPercent
          and set(p) = appState <- { 
              ProjectName = appState.ProjectName;
              Data = appState.Data; 
              Attributes = appState.Attributes;
              ClassColumn = appState.ClassColumn;
              TrainPercent = p;
              IgnoreColumns = appState.IgnoreColumns;
              ReviewPair = appState.ReviewPair;
              }

       /// clear the attributes
       /// for the state using the current data set.
       member b.ClearAttributes () =
            if (b.Data <> emptyDataSet) then
                let attList = List.map (fun i -> { AttributeLabel = "Unnamed"; Column = i; AttributeType = String; }) [0..(b.Data.Columns - 1)]
                b.Attributes <- attList
            else 
                b.Attributes <- emptyAttributes
        
       /// assign the attribute names from the raw data header.
       member b.AssignAttributesFromRawDataHeader () =
            let assignAttrNames names attributes =
                let attList = List.mapi (fun i n -> 
                                             let attr = attributeAt i attributes
                                             let attr' =
                                                        { AttributeLabel = n;
                                                          Column = i;
                                                          AttributeType = attr.AttributeType }
                                             attr') names
                attList
            let firstRow = b.Data.RawData |> Seq.head |> Seq.toList
            b.Attributes <- assignAttrNames firstRow b.Attributes
                                                                                                            

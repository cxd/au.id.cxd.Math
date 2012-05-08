namespace au.id.cxd.Math.UI 

    open System
    open System.IO
    open System.Runtime.Serialization.Formatters.Binary
    open System.Windows
    open MathUIProject
    open StateM
    open au.id.cxd.Math.RawData
    open au.id.cxd.Math.TrainingData
    open au.id.cxd.Math.UI.DualChartTypes



    module UIApplication = 
        
        type DataPair = option<(Attribute * Attribute)>

        
        /// <summary>
        /// UI application state data
        /// </summary>
        type UIApplicationState = { 
                                   Activities: ProjectActivity List; 
                                   Data: RawDataSet; 
                                   Attributes: AttributeList;
                                   ClassColumn: ClassificationAttribute;
                                   TrainPercent:double;
                                   IgnoreColumns:int List;
                                   ReviewPair: DataPair;
                                   DualChart: DualChartName;
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
              data :?> UIApplicationState
          

        /// ui state instance
        type UIState(a:Application) = 

            /// <summary>
            /// Application instance.
            /// </summary>
            let mutable instance = a

            let mutable appState = { 
                                     Activities = List.empty; 
                                     Data = emptyDataSet; 
                                     Attributes = List.empty;
                                     ClassColumn = { Label = ""; Column = -1; };
                                     TrainPercent = 1.0;
                                     IgnoreColumns = List.empty;
                                     ReviewPair = None;
                                     DualChart = Scatter;
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
                    
            
            /// <summary>
            /// Modify the dual data.
            /// </summary>
            member b.ReviewPair with get() = appState.ReviewPair
                                  and set(p) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = p;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// Accessor for application instance reference.
            /// </summary>
            member b.Instance with get() = instance

            /// <summary>
            /// State property.
            /// </summary>
            member b.State with get() = appState and set(v) = appState <- v

            /// <summary>
            /// An accessor for activities.
            /// </summary>
            member b.Activities with get() = appState.Activities

            /// <summary>
            /// Add an activity to the state.
            /// </summary>
            member b.AddActivity act = appState <- { 
                                                     Activities = List.append appState.Activities [act]; 
                                                     Data = appState.Data; 
                                                     Attributes = appState.Attributes;
                                                     ClassColumn = appState.ClassColumn;
                                                     TrainPercent = appState.TrainPercent;
                                                     IgnoreColumns = appState.IgnoreColumns;
                                                     ReviewPair = appState.ReviewPair;
                                                     DualChart = appState.DualChart;
                                                    }
        
            /// <summary>
            /// Clear all activities of the state.
            /// </summary>
            member b.ClearActivities() = appState <- { 
                                                       Activities = List.empty; 
                                                       Data = emptyDataSet; 
                                                       Attributes = List.empty;
                                                       ClassColumn = { Label = ""; Column = -1; };
                                                       TrainPercent = 0.7;
                                                       IgnoreColumns = appState.IgnoreColumns;
                                                       ReviewPair = appState.ReviewPair;
                                                       DualChart = appState.DualChart;
                                                    }
            /// <summary>
            /// Modify the app state data record.
            /// </summary>
            member b.Data with get() = appState.Data 
                          and set(d) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = d; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }
            
            /// <summary>
            /// Modify the attributes.
            /// </summary>
            member b.Attributes with get() = appState.Attributes
                                and set(a) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = a;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// Clear the ignore columns.
            /// </summary>
            member b.ClearIgnoreColumns() = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = List.empty;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// Add a column to ignore.
            /// </summary>
            member b.AddIgnoreColumn(c) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = c :: appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// Remove a column from the ignore list.
            /// </summary>
            member b.RemoveIgnoreColumn(c) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = List.filter (fun a -> a <> c) appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
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
            member b.ClassColumn with get() = appState.ClassColumn
                                 and set(c) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = c;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }
            /// <summary>
            /// Modify the class column.
            /// </summary>
            member b.SetClassColumn(attr:Attribute) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = { Label = attr.AttributeLabel; Column = attr.Column; }
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// Accessor modifier for percentage of training data.
            /// </summary>
            member b.TrainPercent with get() = appState.TrainPercent
                                  and set(p) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = p;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = appState.DualChart;
                                                    }

            /// <summary>
            /// The type of dual chart to review.
            /// </summary>
            member b.SelectedDualChart with get() = appState.DualChart
                                       and set(c) = appState <- { 
                                                    Activities = appState.Activities; 
                                                    Data = appState.Data; 
                                                    Attributes = appState.Attributes;
                                                    ClassColumn = appState.ClassColumn;
                                                    TrainPercent = appState.TrainPercent;
                                                    IgnoreColumns = appState.IgnoreColumns;
                                                    ReviewPair = appState.ReviewPair;
                                                    DualChart = c;
                                                    }
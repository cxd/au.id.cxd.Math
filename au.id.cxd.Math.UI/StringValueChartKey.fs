namespace au.id.cxd.Math.UI 

open System
open System.Data
open System.Windows
open System.Windows.Controls
open System.Windows.Forms.Integration
open Microsoft.Win32
open au.id.cxd.Math.TextIO

open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.DataDescription
open au.id.cxd.Math.DataHistogram
open au.id.cxd.Math.DataDistance

open au.id.cxd.Math.UIBuilder.MathUIBuilder
open MathUIProject
open StateM
open UIApplication
open au.id.cxd.Math.DataSummary
open System.Drawing
open System.Windows
open System.Windows.Forms.DataVisualization
open au.id.cxd.Math.MaybeBuilder
open au.id.cxd.Math.UI

module StringValueChartKey =


    let gridKeyForStringValues (appState:UIState) (attrA:Attribute) (data:LearningData) gridFn =
        childui (new DataGrid()) 
                (fun parent (child:UIElement) ->
                    let dataGrid = (child :?> DataGrid)
                    // select the data
                    let colA = columnAt attrA.Column data.TrainData
                            
                    let datalabels =
                        match attrA.AttributeType with
                        | String ->
                            uniqueAttributeValues attrA data.TrainData
                                |> Set.toList
                                |> List.mapi (fun i datum -> 
                                                    (datum.StringVal, i+1))
                        | _ -> List.empty
                    let dataSet = new DataSet()
                    let dataTable = new System.Data.DataTable()
                    let col1 = new System.Data.DataColumn("Label")
                    let col2 = new System.Data.DataColumn("Index")
                    dataTable.Columns.Add(col1)
                    dataTable.Columns.Add(col2)
                    List.iter (fun (name, idx) ->
                                let row = dataTable.NewRow()
                                row.[0] <- name
                                row.[1] <- idx
                                dataTable.Rows.Add(row)
                                ) datalabels
                    dataSet.Tables.Add(dataTable)
                    dataGrid.DataContext <- dataSet.Tables.[0]
                    dataGrid.ItemsSource <- dataSet.Tables.[0].DefaultView     
                    
                    gridFn dataGrid

                    parent)


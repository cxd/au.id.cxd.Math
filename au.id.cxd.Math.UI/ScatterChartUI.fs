namespace au.id.cxd.Math.UI 

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Forms.Integration
open Microsoft.Win32
open au.id.cxd.Math.TextIO
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.UIBuilder.MathUIBuilder
open MathUIProject
open StateM
open UIApplication
open au.id.cxd.Math.DataSummary
open System.Drawing
open System.Windows
open System.Windows.Forms.DataVisualization
open FSChart
open FSChart.Builder
open ChartUI
open au.id.cxd.Math.MaybeBuilder
open au.id.cxd.Math.UI.DualChartTypes

module ScatterChartUI = 

    /// <summary>
    /// Draw the scatter chart for the current application pair of review attributes.
    /// </summary>
    let drawScatterChart (appState:UIState) (data:LearningData) =
        childui (new Grid()) 
                (fun parent (child:UIElement) ->

                let gridBox = (child :?> Grid)
                let addgrid = add gridBox

                gridBox.ColumnDefinitions.Add(new ColumnDefinition())
                gridBox.RowDefinitions.Add(new RowDefinition())
                gridBox.RowDefinitions.Add(new RowDefinition())

                appState.ReviewPair <- 
                    maybe {
                        let! (attrA, attrB) = appState.ReviewPair

                        let label = childui (new Label(Content = String.Format("Scatter Plot of Column {0} - {1} and Column {2} - {3}", attrA.Column, attrA.AttributeLabel, attrB.Column, attrB.AttributeLabel)))
                                            (fun parent child ->
                                                Grid.SetColumn(child, 0)
                                                Grid.SetRow(child, 0)
                                                parent)

                        
                        // determine the attribute type for columnA and normalise.
                        let dataA = match attrA.AttributeType with
                                    | String -> stringAttributeToNumericKey attrA data.TrainData |> snd
                                    | Bool -> List.empty
                                    | NumericOrdinal
                                    | Continuous -> columnFloatValues attrA data.TrainData
                        
                        // determine the attribute type for columnB and normalise.
                        let dataB = match attrB.AttributeType with
                                    | String -> stringAttributeToNumericKey attrB data.TrainData |> snd
                                    | Bool -> List.empty
                                    | NumericOrdinal
                                    | Continuous -> columnFloatValues attrB data.TrainData

                        // generate the chart.
                        let host = new WindowsFormsHost()
                             
                        let ax = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
               
                        let area = 
                                areaF |> axisX ( ax >> title (text (String.Format("{0}", attrA.AttributeLabel))) >> linear 1.0 ) 
                                      |> axisY ( ax >> title (text (String.Format("{0}", attrB.AttributeLabel))) >> linear 1.0 ) 
                                      |> backColor aliceBlue
                                      |> legend ( top >> near >> transparent >> arialR 8 )
                                      |> title  ( text (String.Format("Scatter Plot of {0} and {1}", attrA.AttributeLabel, attrB.AttributeLabel)) >> arialR 14 >> color steelBlue )
                        let chart = single area 
                                     [scatter 
                                      |> marker (circle >> color (alpha 70 blue) ), SeriesData.XY (dataA |> List.toArray, dataB |> List.toArray)  ]

                        let winChart = new Charting.Chart()

                        MSChart.displayIn winChart chart
                        host.Child <- winChart
                        let hostui = childui host 
                                             (fun parent child -> 
                                                    Grid.SetColumn(child, 0)
                                                    Grid.SetRow(child, 1)
                                                    parent)

                        label addgrid 
                        |> 
                        (appendUI
                        >> hostui
                        >> appendUI)
                        |> ignore

                        return (attrA, attrB)}

                parent)

    ()


namespace au.id.cxd.Math.UI 

open System
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
open FSChart
open FSChart.Builder
open ChartUI
open au.id.cxd.Math.MaybeBuilder
open au.id.cxd.Math.UI.DualChartTypes

module CdfComparisonUI =
    

    type CdfSeries = string * Color * float array * float array

    /// <summary>
    /// create a cdf chart for a given series of cdf pairs
    /// this allows charts to be created for a collection of series instead of only two.
    /// </summary>
    let createCdfChart (cdfpairs:CdfSeries list) =
        let axis = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
                
        let area = 
            areaF |> axisX ( axis >> linear 1.0 ) 
                    |> axisY ( axis >> linear 1.0 ) 
                    |> backColor aliceBlue
                    |> legend ( top >> near >> transparent >> arialR 8 )

        let charts =
            List.map (fun (label, col, (x:float array), (y:float array)) ->
                        line |> color col |> text label, SeriesData.XY(x, y)) cdfpairs

        let chart = single area charts
                     
        let winChart = new Charting.Chart()

        MSChart.displayIn winChart chart   
        winChart

    /// <summary>
    /// given the two attributes
    /// draw two cdf curves in different colours
    /// compare the two curves.
    /// </summary>
    let drawCdfCompareChart (appState:UIState) (data:LearningData) =
        
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
                        match (attrA.AttributeType, attrB.AttributeType) with
                            | (_, Bool)
                            | (_, String)
                            | (Bool, _)
                            | (String, _) -> 
                                 let label = 
                                     childui (new Label(Content = String.Format("Column {0} - {1} or Column {2} - {3} Cannot be used in this type of chart.", attrA.Column, attrA.AttributeLabel, attrB.Column, attrB.AttributeLabel)))
                                             (fun parent child ->
                                                Grid.SetColumn(child, 0)
                                                Grid.SetRow(child, 0)
                                                parent)
                                 
                                 label addgrid 
                                    |> appendUI
                                    |> ignore
                            | (NumericOrdinal, NumericOrdinal)
                            | (NumericOrdinal, Continuous)
                            | (Continuous, NumericOrdinal)
                            | (Continuous, Continuous) ->
                                let (ax, ay) = cdfFromHistogramCount attrA data.TrainData
                                let (bx, by) = cdfFromHistogramCount attrB data.TrainData    
                                let label = 
                                        childui (new Label(Content = String.Format("Column {0} - {1} CDF compared to Column {2}-{3} CDF - domains f(x{0}) and f((x{2}) may differ",
                                                                                    attrA.Column, 
                                                                                    attrA.AttributeLabel,
                                                                                    attrB.Column,
                                                                                    attrB.AttributeLabel)))
                                                (fun parent child ->
                                                     Grid.SetColumn(child, 0)
                                                     Grid.SetRow(child, 0)
                                                     parent)

                                // graph of CDF 1 overlayed on CDF2
                                // generate the chart.
                                let host = new WindowsFormsHost()
                                let axis = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
                
                                let area = 
                                    areaF |> axisX ( axis >> linear 1.0 ) 
                                          |> axisY ( axis >> linear 1.0 ) 
                                          |> backColor aliceBlue
                                          |> legend ( top >> near >> transparent >> arialR 8 )

                                let chartLabel (att:Attribute) = String.Format("CDF({0}-{1})", att.Column, att.AttributeLabel)

                                let series =
                                    [(chartLabel attrA, red, ax |> List.toArray, ay |> List.toArray);
                                     (chartLabel attrB, blue, bx |> List.toArray, by |> List.toArray)]

                                let winChart = createCdfChart series
                                
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
                                 >> appendUI) |> ignore
                        |> ignore

                        return (attrA, attrB)}

                parent)


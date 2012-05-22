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

module RankOrderPlotUI = 

    /// <summary>
    /// Draw the rank order chart for the current application pair of review attributes.
    /// </summary>
    let drawRankOrderPlot (appState:UIState) (data:LearningData) =
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
                            // define a label
                            let label = childui (new Label(Content = String.Format("Rank Order Plot of Column {0} - {1} and Column {2} - {3}", attrA.Column, attrA.AttributeLabel, attrB.Column, attrB.AttributeLabel)))
                                            (fun parent child ->
                                                Grid.SetColumn(child, 0)
                                                Grid.SetRow(child, 0)
                                                parent)

                            // make the chart
                            let colA = columnAt attrA.Column data.TrainData
                            let colB = columnAt attrB.Column data.TrainData

                            // use colA as the independent variable.
                            let datalabels =
                                match attrA.AttributeType with
                                | String ->
                                    uniqueAttributeValues attrA data.TrainData
                                     |> Set.toList
                                     |> List.map (fun datum -> 
                                                            (datum.StringVal, 
                                                              (fun i -> 
                                                                let ldatum = List.nth colA.ProcessedData i
                                                                ldatum.StringVal.ToLower().Equals(datum.StringVal.ToLower())))
                                                                 )
                                   
                                | NumericOrdinal
                                | Continuous ->
                                    histogram attrA data.TrainData
                                    |> List.map (fun (low, high) -> 
                                                      (String.Format("{0} <= n <= {1}", low.ToString("0.000"), high.ToString("0.000")), 
                                                       (fun i -> 
                                                        let ldatum = List.nth colA.ProcessedData i
                                                        match attrA.AttributeType with
                                                        | Continuous -> (ldatum.FloatVal) >= low &&  (ldatum.FloatVal) <= high
                                                        | NumericOrdinal -> low <= Convert.ToDouble(ldatum.IntVal) &&  Convert.ToDouble(ldatum.IntVal) <= high
                                                        | _ -> false))) 
                                | Bool ->
                                    [("true", 
                                        (fun i -> 
                                            let ldatum = List.nth colA.ProcessedData i
                                            ldatum.BoolVal = true)); 
                                        ("false",  
                                         (fun i -> 
                                            let ldatum = List.nth colA.ProcessedData i
                                            ldatum.BoolVal = false))]
                                    
                            // use colB as the dependent variable.
                            // sum values of colB where they correspond to constraint of colA
                            let counts = 
                                
                                let sum = List.fold (fun n datum ->
                                                        match attrB.AttributeType with
                                                        | NumericOrdinal ->
                                                            n + Convert.ToDouble(datum.IntVal)
                                                        | Continuous -> n + datum.FloatVal
                                                        | _ -> n+1.0) 0.0 colB.ProcessedData
                                List.map
                                    (fun (l, fn) -> 
                                        let cnts = List.fold 
                                                    (fun (n, i) datum ->
                                                        if (fn i) then
                                                            match attrB.AttributeType with
                                                            | NumericOrdinal ->
                                                                (n + Convert.ToDouble(datum.IntVal), i+1)
                                                            | Continuous ->
                                                                (n + datum.FloatVal, i+1)
                                                            | _ -> (n+1.0, i+1)
                                                        else (n, i+1)) (0.0, 0) colB.ProcessedData    
                                        (l, ((fst cnts)/sum)*100.0))
                                    datalabels

                            let serieslabels = 
                                List.map (fun (l, f) -> l) counts |> List.toArray
                            let seriesvals =
                                List.map (fun (l, f) -> f) counts |> List.toArray

                            let cdfseriesvals =
                                List.map (fun (l, f) -> f) counts |> cdfFromSeries |> List.toArray

                            // so the counts contains a unique label with a count
                            // for each "label" that is the sum of continuous values.
                            // it contains 0 where attribute B is either a string or bool as 
                            // this makes no sense to count either of those categories.
                            // if "A" is a string and "B" is a string it might be better
                            // to show a "contingency" table instead.
                            // a string or bool is essentially counted as "1" if the corresponding
                            // class label for "A" matches the row.
                            // in this case the chart shows a singular value relation
                            // and is not affected by the string label value of variable B.

                            let host = new WindowsFormsHost()
                            let ax = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
                            let area = 
                                areaS |> axisX ( labels (arialR 8) >> title (text attrA.AttributeLabel)) 
                                      |> axisY ( ax >> title (text (String.Format("Percentage of {0}", attrB.AttributeLabel))) >> linear 1.0 ) 
                                      |> backColor aliceBlue
                                      |> legend ( top >> near >> transparent >> arialR 8 )
                                      |> title  ( text (String.Format("Rank Order Plot of {0} and {1}", attrA.AttributeLabel, attrB.AttributeLabel)) >> arialR 14 >> color steelBlue )
                            
                            let chart = C.create <|
                                            (C.plot area
                                                //[bar 1.0 |> border darkBlue 1
                                                [line |> color darkBlue |> marker (circle >> color (alpha 70 blue)) |> text (String.Format("Percentage of {0}", attrB.AttributeLabel))
                                                    , SeriesData.SY(serieslabels, seriesvals) 
                                                 line |> color red |> text (String.Format("CDF ({0}-{1})", attrB.Column, attrB.AttributeLabel))
                                                    , SeriesData.SY(serieslabels, cdfseriesvals)])
                            let winChart = new Charting.Chart()
                            MSChart.displayIn winChart chart
                            host.Child <- winChart
                            winChart.Update()
                            let hostui = childui host
                                           (fun parent child ->
                                                Grid.SetColumn(child, 0)
                                                Grid.SetRow(child, 1)
                                                parent)
                            label addgrid |> 
                            (appendUI 
                            >> hostui
                            >> appendUI) |> ignore

                            return (attrA, attrB)}

                    parent)


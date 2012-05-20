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

open au.id.cxd.Math.UI.ScatterChartUI
open au.id.cxd.Math.UI.RankOrderPlotUI
open au.id.cxd.Math.UI.ScatterLoessCurveUI
open au.id.cxd.Math.Regression
open au.id.cxd.Math.UI.CdfComparisonUI

module DualChartUI =

    /// <summary>
    /// Draw the chart UI into the supplied parent container.
    /// </summary>
    let drawChartUI (appState:UIState) (parent:UIElement) =
        clear parent
        let addin = add parent
        let append n = n addin |> appendUI |> ignore
        // generate the data.
        let data = if (not appState.HasProcessedData) then
                        appState.ProcessedData <- convertFromRawData appState.TrainPercent appState.ClassColumn.Column appState.Attributes appState.Data appState.DataWithColumnHeaders
                        appState.ProcessedData
                   else appState.ProcessedData

        match appState.SelectedDualChart with
        | Scatter -> 
            (drawScatterChart appState data)
            |> append
        | ScatterAndRegression -> 
            (drawScatterRegressionChart "Regression Curve" straightline appState data) 
            |> append
        | ScatterAndLoessCurve -> 
            ()
        | LoessCurve -> ()
        | SingleLogarithm -> ()
        | DoubleLogarithm -> ()
        | RankOrderPlot ->
            (drawRankOrderPlot appState data)
            |> append 
        | CdfComparisonPlot ->
            (drawCdfCompareChart appState data)
            |> append


    /// <summary>
    /// Define two combo boxes to select the variables
    /// that can be compared.
    /// </summary>
    let defineOptions (appState:UIState) (chartParent:UIElement) =
        let gridui = childui (new Grid())
                             (fun parent (child:UIElement) ->
                                let grid = (child :?> Grid)
                                grid.ColumnDefinitions.Add(new ColumnDefinition())
                                grid.ColumnDefinitions.Add(new ColumnDefinition())
                                grid.ColumnDefinitions.Add(new ColumnDefinition())
                                grid.RowDefinitions.Add(new RowDefinition())
                                grid.RowDefinitions.Add(new RowDefinition())
                                
                                let addin = add grid

                                // initialise the review pair if undefined.
                                match appState.ReviewPair with
                                | None -> 
                                    appState.ReviewPair <- Some (List.head appState.Attributes, List.tail appState.Attributes |> List.head)
                                | _ -> ()

                                let labelA = childui (new Label(Content = "Select Variable A"))
                                                        (fun parent (child:UIElement) ->
                                                            Grid.SetColumn(child, 0)
                                                            Grid.SetRow(child, 0)
                                                            parent)
                                let labelB = childui (new Label(Content = "Select Variable B"))
                                                     (fun parent (child:UIElement) ->
                                                          Grid.SetColumn(child, 1)
                                                          Grid.SetRow(child, 0)
                                                          parent)
                                let chartLabel = childui (new Label(Content = "Select Chart Type"))
                                                         (fun parent (child:UIElement) ->
                                                            Grid.SetColumn(child, 2)
                                                            Grid.SetRow(child, 0)
                                                            parent)

                                let optionsA = childui (new ComboBox())
                                                       (fun parent (child:UIElement) ->
                                                          Grid.SetColumn(child, 0)
                                                          Grid.SetRow(child, 1)
                                                          List.iter(fun (attr:Attribute) ->
                                                                    let item = new ComboBoxItem(Content = String.Format("Col {0} {1}",attr.Column.ToString(), attr.AttributeLabel))
                                                                    (child :?> ComboBox).Items.Add(item) |> ignore
                                                                    // modify the data selected for review.
                                                                    item.Selected.AddHandler(fun sender evt ->
                                                                                                appState.ReviewPair 
                                                                                                    <- maybe {
                                                                                                                let! (a, b) = appState.ReviewPair
                                                                                                                return (attr, b)
                                                                                                            }
                                                                                                // redraw the plot
                                                                                                drawChartUI appState chartParent
                                                                                                ())
                                                                    
                                                                    ()) appState.Attributes
                                                          parent)
                                let optionsB = childui (new ComboBox())
                                                       (fun parent (child:UIElement) ->
                                                          Grid.SetColumn(child, 1)
                                                          Grid.SetRow(child, 1)
                                                          List.iter(fun (attr:Attribute) ->
                                                                    let item = new ComboBoxItem(Content = String.Format("Col {0} {1}",attr.Column.ToString(), attr.AttributeLabel))
                                                                    (child :?> ComboBox).Items.Add(item) |> ignore
                                                                    // modify the data selected for review.
                                                                    item.Selected.AddHandler(fun sender evt ->
                                                                                                appState.ReviewPair 
                                                                                                    <- maybe {
                                                                                                                let! (a, b) = appState.ReviewPair
                                                                                                                return (a, attr)
                                                                                                            }
                                                                                                // redraw the plot
                                                                                                drawChartUI appState chartParent
                                                                                                ())
                                                                    ()) appState.Attributes
                                                          parent)

                                let chartOptions = childui (new ComboBox())
                                                           (fun parent (child:UIElement) ->
                                                            Grid.SetColumn(child, 2)
                                                            Grid.SetRow(child, 1)

                                                            let plots = [Scatter;
                                                                            ScatterAndRegression;
                                                                            ScatterAndLoessCurve;
                                                                            LoessCurve;
                                                                            SingleLogarithm;
                                                                            DoubleLogarithm;
                                                                            RankOrderPlot;
                                                                            CdfComparisonPlot]
                                                            
                                                            List.iter (fun plot ->
                                                                        let item = new ComboBoxItem(Content = plotName plot)
                                                                        (child :?> ComboBox).Items.Add(item) |> ignore
                                                                        item.Selected.AddHandler(fun sender evt ->
                                                                                                // allow the user to select the type of plot.
                                                                                                appState.SelectedDualChart <- plot
                                                                                                // activate the plot
                                                                                                drawChartUI appState chartParent)
                                                                        ()) plots
                                                            parent)

                                labelA addin
                                |> (appendUI
                                    >> labelB
                                    >> appendUI
                                    >> chartLabel
                                    >> appendUI
                                    >> optionsA
                                    >> appendUI
                                    >> optionsB
                                    >> appendUI
                                    >> chartOptions
                                    >> appendUI) |> ignore
                                parent)
        gridui
    
    /// <summary>
    /// Build the multichart control.
    /// </summary>
    let dualchart (appState:UIState) parent =
        clear parent
        let stack = childui (new StackPanel())
                            (fun parent child ->
                                
                                 let addchild = add child

                                 let chartpanel = new StackPanel()
                                 let chartpanelui = childui chartpanel nochange

                                 (defineOptions appState chartpanel) addchild 
                                 |> 
                                 (appendUI
                                 >> chartpanelui
                                 >> appendUI)
                                 |> ignore

                                 parent)
        
        
        let addin = add parent
        stack addin |> appendUI |> ignore
        update parent
    

    ()


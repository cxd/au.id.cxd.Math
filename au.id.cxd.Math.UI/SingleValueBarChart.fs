namespace au.id.cxd.Math.UI

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Forms.Integration
open System.IO.Packaging
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
open System.Windows.Markup
open System.Windows.Forms.DataVisualization
open System.Windows.Media.Imaging
open System.Windows.Documents
open System.Reflection
open System.IO
open FSChart
open FSChart.Builder
open au.id.cxd.Math.UIBuilder.DocUI

module SingleValueBarChart =

    /// <summary>
    /// Draw a single value bar chart.
    /// return a series def.
    /// </summary>
    let singleValueBarChart (attr:Attribute) (data:DataTable) =
        match attr.AttributeType with
        | String -> 
            let uniqueCounts = stringValueCount attr data
            let cnt = List.length uniqueCounts
            
            let g   = grid ( solid lightSteelBlue 1 )
            let lowest = min (minOfPairs uniqueCounts) 0.0
            let highest = maxOfPairs uniqueCounts
            
            let ax = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
           
            let labels1 = List.map(fun (n, s) -> s) uniqueCounts
            let counts = List.map(fun (n, s) -> n) uniqueCounts

            barH 1.0 |> border darkBlue 1 |> text attr.AttributeLabel, SeriesData.SY("" :: labels1 |> List.toArray, 0.0 :: counts |> List.toArray)
        | Continuous
        | NumericOrdinal -> 
            let histcnts = histogramCount attr data
            // plot an x y curve of volume for each point in the histogram.
            // y - the count of values within each low and high pair of the range.
            let y = List.map(fun ((n:int), (low, high)) -> Convert.ToDouble(n)) histcnts |> List.toArray
            
            let total = Array.fold (+) 0.0 y

            let y2 = Array.map(fun n -> 100.0 * (n/total)) y
            
            // the high value of each pair in the range
            let x = List.map(fun (n, (low, high)) -> high) histcnts |> List.toArray
            
            bar 1.0
            |> border darkBlue 1 
            |> text ( ((new System.Text.StringBuilder()).AppendFormat("Count of {0}", attr.AttributeLabel)).ToString() )
            , SeriesData.XY (x, y2)
        | _ -> 
            barH 1.0 |> border darkBlue 1 |> text attr.AttributeLabel, SeriesData.SY([|""|], [|0.0|])



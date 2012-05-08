namespace au.id.cxd.Math.UI

open System
open au.id.cxd.Math.TrainingData

module DualChartTypes =
    /// <summary>
    /// The types of charts available for analysis.
    /// </summary>
    type DualChartName =
            | Scatter
            | ScatterAndRegression
            | ScatterAndLoessCurve
            | LoessCurve
            | SingleLogarithm
            | DoubleLogarithm
            | RankOrderPlot

    /// <summary>
    /// The name of the plot for the
    /// corresponding chart type.
    /// </summary>
    let plotName plot =
        match plot with
        | Scatter -> "Scatter Plot"
        | ScatterAndRegression -> "Scatter and Linear Separator"
        | ScatterAndLoessCurve -> "Scatter Loess Curve"
        | LoessCurve -> "Loess Curve"
        | SingleLogarithm -> "Single Logarithm"
        | DoubleLogarithm -> "Double Logarithm"
        | RankOrderPlot -> "Rank Order Plot"


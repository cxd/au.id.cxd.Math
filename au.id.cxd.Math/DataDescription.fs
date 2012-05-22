namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open RawData
open TrainingData

/// <summary>
/// Summary attributes for the data set
/// </summary>
module DataDescription =

    /// <summary>
    /// Get the minimum value of the data set.
    /// </summary>
    let dataMin (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        List.min vals

    /// <summary>
    /// Get the maximum value of the data set.
    /// </summary>
    let dataMax (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        List.max vals

    /// <summary>
    /// Calculate the mean of the set of data.
    /// </summary>
    let mean (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let (total, cnt) = 
            match attr.AttributeType with
            | Continuous -> List.fold (fun (t, c) data -> (t + data.FloatVal, c + 1.0)) (0.0, 0.0) col.ProcessedData
            | NumericOrdinal -> List.fold (fun (t, c) data -> (t + Convert.ToDouble(data.IntVal), c + 1.0)) (0.0, 0.0) col.ProcessedData
            | _ -> (1.0, 1.0)
        total / cnt

    /// <summary>
    /// Calculate the variance of the data set.
    /// Var(X) = ( sum of (x - u)^2 ) / (N - 1)
    /// where u = mean(X)
    /// </summary>
    let variance (attr:Attribute) (data:DataTable) = 
        let u = mean attr data
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let len = Convert.ToDouble(List.length vals)
        let n =
            vals
            |> List.map (fun v -> ((v - u)**2.0) )
            |> List.fold (fun n v -> n + v) 0.0
        n / (len - 1.0)

    /// <summary>
    /// Estimate the covariance for the data column 
    /// Cov(X,Y) = (sum of (x - xu)(y - yu)) / N
    /// where xu is mean X
    /// yu is mean Y
    /// N is length X
    /// </summary>
    let covariance (attrA:Attribute) (dataA:DataTable) (attrB:Attribute) (dataB:DataTable) = 
        let uA = mean attrA dataA
        let colA = columnAt attrA.Column dataA
        let uB = mean attrB dataB
        let colB = columnAt attrB.Column dataB
        let vals attr col = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let valsA = vals attrA colA
        let valsB = vals attrB colB
        let XY = List.zip valsA valsB
        let cnt = List.length valsA
        let total = List.fold (fun n (x,y) -> n + (x - uA)*(y - uB)) 0.0 XY
        total / Convert.ToDouble(cnt)

    /// <summary>
    /// Compute the standard deviation of the data set
    /// by the 
    /// sqrt of variance
    /// </summary>
    let stddev (attr:Attribute) (data:DataTable) =
        variance attr data |> sqrt

    


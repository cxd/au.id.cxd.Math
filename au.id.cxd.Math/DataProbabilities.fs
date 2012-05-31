namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open RawData
open TrainingData
open DataDescription
open DataHistogram


/// <summary>
/// Probabilities for the data set
/// </summary>
module DataProbabilities = 

    /// <summary>
    /// Calculate the probability of an instance in the data set based on the attribute type
    /// </summary>
    let probabilityAttributeValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            let data = (columnAt attr.Column examples).ProcessedData
            let cnt = Convert.ToDouble(List.length data)
            (countAttributeValue instance attr examples) / cnt

    /// <summary>
    /// Calculate the probabilities of each sample in the set
    /// </summary>
    let probabilities (attr:Attribute) (examples:DataTable) = 
        let hist = histogram attr examples
        let data = (columnAt attr.Column examples).ProcessedData
        let countAttrValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            match attr.AttributeType with
                | Continuous 
                | NumericOrdinal -> 
                    // continuous and numeric ordinal are processed in the same way.
                    // 
                    // bin the values in the data set X into [(X.1, X.2) .. (X.n-1, X.n)]
                    // then find the pair that bounds the instance value
                    // then approximate the probability by counting the number of samples that fall inside the same
                    // range and divide by the total number of samples
                    // (a, b) = [(i,j) | i <= x <= j, (i,j) <- histogram]
                    // K = [k | a <= k <= b, k <- X]
                    // P(x.i) ~= |K| / |X|
                    // find the pair that contains the instance
                    let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.FloatVal >= lt && datum.FloatVal <= gt then n + 1.0
                                                       else n) 0.0 data)
                | _ -> 0.0
        let len = Convert.ToDouble(List.length data)
        data |>
        List.map (fun item -> (countAttrValue item attr examples) / len)
        
   

    /// <summary>
    /// Calculate the variance of the data set.
    /// Var(X) = sum of P(x) * (x - u)
    /// where u = mean(X)
    /// P(x) is approximated using a histogram.
    /// </summary>
    let probablevariance (attr:Attribute) (data:DataTable) = 
        let u = mean attr data
        let col = columnAt attr.Column data
        let hist = histogram attr data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let len = Convert.ToDouble(List.length vals)
        probabilities attr data
        |> List.zip vals
        |> List.map (fun (v, p) -> p * ((v - u)**2.0) )
        |> List.fold (fun n v -> n + v) 0.0

    /// <summary>
    /// Compute the standard deviation.
    /// std = square root of variance
    /// </summary>
    let probablestddev (attr:Attribute) (data:DataTable) =
        probablevariance attr data |> sqrt
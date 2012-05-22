namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open RawData
open TrainingData
open DataDescription

/// <summary>
/// Histogram operations on the data set
/// </summary>
module DataHistogram = 
    
    /// <summary>
    /// Generate the N-1 histogram of sorted X where histogram is [(X.1, X.2), (X.2, X.3), (X.3, X.4) ... (X.n-1, X.n) ]
    /// This can then be fed into a function to approximate probability of a continuous value
    /// by measuring the count (X.n-1 >= x <= X.n) / |X|
    /// </summary>
    let histogram (attr:Attribute) (data:DataTable) =
           let vals = 
                match attr.AttributeType with
                | Continuous ->  List.map (fun (datum:Datum) -> datum.FloatVal) ((columnAt attr.Column data).ProcessedData)
                | NumericOrdinal -> List.map (fun (datum:Datum) -> Convert.ToDouble(datum.IntVal)) ((columnAt attr.Column data).ProcessedData)
                | _ -> List.empty
           let sortedA = (List.sort vals) |> Set.ofList |> Set.toSeq
           let sortedB = (List.sort vals) |> Set.ofList |> Set.toSeq
           let skip = Seq.append (Seq.skip 1 sortedB) [0.0]
           let test = Seq.length skip
           let hist = Seq.map2 (fun v1 v2 -> (v1, v2) ) sortedA skip 
                      |> Seq.take ((Seq.length sortedA) - 1) 
                      |> Seq.toList
           if (List.length hist).Equals(0) then [(0.0, Seq.head sortedA)]
           else hist

    /// <summary>
    /// Count the frequency of each histgram range in the data set.
    /// </summary>
    let histogramCount (attr:Attribute) (data:DataTable) =
        let histcnts = histogram attr data |> List.map (fun h -> (0, h))
        let col = columnAt attr.Column data
        let vals = 
                match attr.AttributeType with
                | Continuous ->  List.map (fun (datum:Datum) -> datum.FloatVal) ((columnAt attr.Column data).ProcessedData)
                | NumericOrdinal -> List.map (fun (datum:Datum) -> Convert.ToDouble(datum.IntVal)) ((columnAt attr.Column data).ProcessedData)
                | _ -> List.empty
        List.map(fun (n, (low, high)) -> 
                        let cnt = List.fold(fun n f -> 
                                                if (f >= low && f <= high) then n + 1
                                                else n) 0 vals
                        (cnt, (low, high))) histcnts

    /// <summary>
    /// Calculate the continuous distribution function
    /// from the population.
    /// This gives the same size of x in the population of n..N where
    /// x = n
    /// Returns the (x series, y series) of the binned data
    /// </summary>
    let cdfFromHistogramCount (attr:Attribute) (data:DataTable) =
        let histcnts = histogramCount attr data
        let len = List.length histcnts
        // y - the count of values within each low and high pair of the range.
        let y = List.map(fun ((n:int), (low, high)) -> Convert.ToDouble(n)) histcnts
        let total = List.fold (+) 0.0 y
        //
        (
        // compute the x series 
         List.map (fun (n, (low, high)) -> high) histcnts,
         // compute the CDF curve percent of the population.
         List.mapi (fun i n -> List.fold (fun (index, total) k -> if (index <= i) then
                                                                    (index + 1, total + k)
                                                                  else (index, total)) (0, 0.0) y 
                               |> snd 
                               |> (fun r -> 100.0 * (r / total))) y)
    /// <summary>
    /// Calculate the continuous distribution function
    /// from the population.
    /// This gives the same size of x in the population of n..N where
    /// x = n
    /// Series is a population of values.
    /// Returns the y series of the binned data
    /// for an unknown X series
    /// </summary>
    let cdfFromSeries series =
        let total = List.fold (+) 0.0 series
        List.mapi (fun i n -> List.fold( fun (index, total) k -> 
                                                    if (index <= i) then
                                                      (index + 1, total + k)
                                                    else (index, total)) (0, 0.0) series
                                       |> snd
                                       |> (fun result -> 100.0 * (result / total)) ) series

        
    /// <summary>
    /// Count the occurance of an instance in the data set based on the attribute type
    /// </summary>
    let countAttributeValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            let data = (columnAt attr.Column examples).ProcessedData
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
                    let hist = histogram attr examples
                    // find the pair that contains the instance
                    let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.FloatVal >= lt && datum.FloatVal <= gt then n + 1.0
                                                       else n) 0.0 data)
                | String -> 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.StringVal.Equals(instance.StringVal) then n + 1.0
                                                       else n) 0.0 data)
                | Bool -> 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.BoolVal.Equals(instance.BoolVal) then n + 1.0
                                                       else n) 0.0 data)
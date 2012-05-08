namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open TrainingData

module DataSummary =

    /// <summary>
    /// Select the set of unique values for the supplied from the sample data.
    /// </summary>
    let uniqueAttributeValues (attr:Attribute) (data:DataTable) = 
        let dataList = (List.nth (snd data) attr.Column).ProcessedData
        List.fold (fun set item -> Set.add item set) Set.empty dataList
    
    /// <summary>
    /// Count the unique values for the given attribute.
    /// </summary>
    let stringValueCount (attr:Attribute) (data:DataTable) =
        let unique = uniqueAttributeValues attr data
        let dataCol = (List.nth (snd data) attr.Column).ProcessedData
        List.map (fun dataVal ->
                        let cnt = List.sumBy (fun (row:Datum) -> if (row.StringVal.Equals(dataVal.StringVal)) then 1.0
                                                                 else 0.0) dataCol
                        (cnt, dataVal.StringVal)
                        ) (Set.toList unique |> List.sortWith (fun (dataA:Datum) (dataB:Datum) -> dataA.StringVal.CompareTo(dataB.StringVal)) )

    /// <summary>
    /// Find the minimum value of pairs
    /// </summary>
    let minOfPairs pairs = 
        List.fold (fun min item -> if (fst item) < min then (fst item) 
                                   else min) System.Double.MaxValue pairs

    /// <summary>
    /// Find the minimum value of pairs
    /// </summary>
    let maxOfPairs pairs = 
        List.fold (fun max item -> if (fst item) > max then (fst item) 
                                   else max) System.Double.MinValue pairs


    /// <summary>
    /// Calculate the euclidean vector length.
    /// </summary>
    let vectorLength vec =
        List.fold (fun n v -> n + (v**2.0)) 0.0 vec
        |> Math.Sqrt

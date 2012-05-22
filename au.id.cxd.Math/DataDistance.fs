namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open RawData
open TrainingData

/// <summary>
/// Summary attributes for the data set
/// </summary>
module DataDistance = 
    
    /// <summary>
    /// Calculate the euclidean norm which is the sqrt of sum of squares of the vector
    /// </summary>
    let euclidnorm (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let total = List.fold (fun n x -> n + x*x) 0.0 vals
        sqrt total

    /// <summary>
    /// Use euclidean norm to normalise the vector matching the attribute.
    /// Applies only to continuous or numeric types.
    /// </summary>
    let euclidnormalisation (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let (total, lst) = List.fold (fun (n, ss) x -> ((n + x*x), x :: ss)) (0.0, []) vals
        let enorm = sqrt total
        List.map (fun n -> n / total) lst

    /// <summary>
    /// Compute the guassian distribution of the vector matching the attribute.
    /// The guassian distribution is calculated as
    /// 1/(sqrt 2pi std) exp (- (x - u)^2/var(X) )
    /// where u is mean X
    /// Since the input data is normalised then 1/(sqrt 2pi) should equal 1
    /// </summary>
    let gaussiandist (attr:Attribute) (data:DataTable) =
        
        let vals = euclidnormalisation attr data
        let len = Convert.ToDouble(List.length vals)
        let total = List.fold (fun t x -> t + x) 0.0 vals
        let u = total / len
        let n =
            vals
            |> List.map (fun v -> ((v - u)**2.0) )
            |> List.fold (fun n v -> n + v) 0.0
        let varx = n / (len - 1.0)
        let std = sqrt varx
        //let prob = 1.0 / (sqrt (2.0* Math.PI * varx) )
        List.sort vals |> List.map ( fun x -> (exp (-1.0*((x - u)**2.0) / varx ) ) )


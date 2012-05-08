namespace au.id.cxd.Text


open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System;
open au.id.cxd.Math


module LevensteinDistance =

    /// <summary>
    /// Align strings at right hand side.
    /// simplest alignment without processing
    /// </summary>
    let alignLast (textA:string) (textB:string) =
        match textA.Length > textB.Length with
            | true -> (textA, textB.PadRight(textA.Length))
            | _ -> (textA.PadRight(textB.Length), textB)

    /// <summary>
    /// calculate the cost of substitution
    /// </summary>
    let cost i j (textA:string) (textB:string) = 
            let charA = textA.[i]
            let charB = textB.[j]
            match charA = charB with
                | true -> 0.0
                | false -> 2.0


    /// <summary>
    /// Compute the distance between a pair of strings using the supplied matrix
    /// </summary>
    let computeDistanceFn (pair:string * string) (mat:Matrix<float>) = 
        /// compute distance    
        let rec distance i j v =
            if mat.[i,j] > -1.0 then
                mat.[i,j]
            else
            match i = 0 with
            | true -> v
            | _ ->
                match j = 0 with
                | true -> v
                | _ -> let min = [ (distance (i-1) j 0.0) + 1.0; // insertion
                                   (distance i (j-1) 0.0) + 1.0; // deletion
                                   (distance (i-1) (j-1) 0.0) + (cost (i-1) (j-1) (fst pair) (snd pair)) // substitution
                                    ]
                                 |> List.min
                       let min' = Convert.ToDouble(min)          
                       Matrix.set mat i j min'
                       min'
        Matrix.mapi distance mat

    /// <summary>
    /// A parameterised method that will compute distance using the supplied
    /// distance function
    /// </summary>
    let computeDistance (textA:string) (textB:string) (distanceFn:string * string -> Matrix<float> -> matrix) =
        let max =
            match textA.Length > textB.Length with
                | true -> textA.Length
                | _ -> textB.Length

        let pair = alignLast textA textB
        
        let mat = Matrix.init (max+1) (max+1) (fun i j ->
                                                   if i = 0 then Convert.ToDouble(j)
                                                   else if j = 0 then Convert.ToDouble(i)
                                                   else -1.0)

        let mat' = distanceFn pair mat
        (mat'.[max,max], mat')

    /// <summary>
    /// An implementation of the levenstein distance
    /// algorithm that computes distance but currently
    /// does not provide backtracking.
    /// The output of the function is a tuple
    /// (total Distance x Matrix)
    /// </summary>
    let computeDistanceMatrix (textA:string) (textB:string) =
        computeDistance textA textB computeDistanceFn
        
        
            

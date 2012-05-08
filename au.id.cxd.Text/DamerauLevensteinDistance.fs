namespace au.id.cxd.Text


open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System;
open au.id.cxd.Math
open au.id.cxd.Text.LevensteinDistance

/// <summary>
/// This procedure computes distance
/// including transposition in the distance calculation
/// </summary>
module DamerauLevensteinDistance =

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
                       // the additional transposition check.
                       let n = i-1
                       let k = j-1
                       let min' =
                            if (i > 1 && j > 1 && ((fst pair).[n] = (snd pair).[k-1]) && ((fst pair).[n-1] = (snd pair).[k])) then
                                let min'' = [
                                            min;
                                            mat.[i-2, j-2] + (cost (i-1) (j-1) (fst pair) (snd pair))
                                            ] |> List.min
                                min''
                            else 
                                min

                       Matrix.set mat i j min'
                       min'
        Matrix.mapi distance mat

    /// <summary>
    /// Compute distance including transpositions
    /// </summary>
    let computeDistanceMatrix (textA:string) (textB:string) =
        computeDistance textA textB computeDistanceFn


namespace au.id.cxd.Text


open System
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Tokens

module LanguageModelUnigramLaplace = 
    
    type Count = float
    type InstanceCount = float

    /// <summary>
    /// A model representing a unigram
    /// </summary>
    type UnigramModel =
        Model of 
                Count * 
                InstanceCount * 
                Dictionary<string,Token>

    /// <summary>
    /// Lift the unigram tuple out of the UnigramModel type
    /// </summary>
    let extractModel (Model (cnt, inst, dataGram)) =
        (cnt, inst, dataGram)

    /// <summary>
    /// Update probabilities using the discount and the discount totals
    /// where for each gram
    /// P(w) = C(w) + 1.0 / N + V
    /// The grams are already counted and the cnt and instances
    /// are already determined
    /// </summary>
    let laplaceSmoothing cnt instances (grams:Dictionary<string, Token>) =
            seq { yield! new List<string>(grams.Keys) } |>
            Seq.iter (fun key -> 
                      let token = lookup grams key
                      match token with
                      | NGram gram -> 
                            let w = (gram.Count + 1.0) / (cnt + instances)
                            let gram' = setWeight w gram 
                            update grams key (NGram gram')
                      | _ -> ()
                      ()) |> ignore
            (cnt, instances, grams)

    /// <summary>
    /// Train the unigram model 
    /// </summary>
    let train (data:Sentences) =
        // mutable store for ngrams
        let grams = new Dictionary<string, Token>()


        let processWord (cnt, instances, dataGrams) word = 
            let instanceCnt' = instances + 1.0
            let token = lookup dataGrams word
            let cnt' = 
                match token with
                | NoToken -> 
                    let token' = makeGram [| word |] |> setCount 1.0 
                    update dataGrams word (NGram token')
                    cnt + 1.0
                | NGram token ->
                    let token' = setCount (token.Count + 1.0) token
                    update dataGrams word (NGram token')
                    cnt
            (cnt', instanceCnt', dataGrams)

        data |>
        Seq.fold 
            (fun (cnt, instanceCnt, dataGrams) words -> 
                 
                 let (cnt', instanceCnt', dataGrams') = Array.fold processWord (cnt, instanceCnt, dataGrams) words
        
                 (cnt', instanceCnt', dataGrams')) (0.0, 0.0, grams)
        
        |> (fun (cnt, instances, dataGrams) ->
            Model (laplaceSmoothing cnt instances dataGrams))
        
    /// <summary>
    /// Calculate the likelihood of the supplied sequence based
    /// on the current language model
    /// </summary>
    let likelihood (model:UnigramModel) (sentence:TokenList) =
        let (cnt, instances, dataGram) = extractModel model
        Array.fold(
            fun total word ->
                let token = lookup dataGram word
                match token with
                | NGram gram ->
                    total + System.Math.Log(gram.Weight)
                | _ -> total + System.Math.Log(1.0 / (cnt + instances)) ) 
                0.0 sentence

    /// <summary>
    /// Extract the vocabulary from the unigram model
    /// </summary>
    let vocab (model:UnigramModel) =
        let (cnt, instances, dataGram) = extractModel model
        seq { for key in dataGram.Keys do
                yield key }

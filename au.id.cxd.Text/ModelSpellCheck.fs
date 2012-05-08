namespace au.id.cxd.Text

open System
open System.Collections.Generic
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open System.Runtime.Serialization.Formatters
open System.Runtime.Serialization
open PorterStemmerAlgorithm
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text

module ModelSpellCheck =

    type Spelling =
        Word of string * float

    type SpellingCorrection =
        Correction of int * Spelling * (float * TokenList)

    /// <summary>
    /// Given the sentence 
    /// the error data list
    /// and the probability function
    /// compute the best match for corrections.
    /// </summary>
    let spellcheck (sentence:TokenList) (vocab:string list) (data:Dictionary<string,float>) (probabilityFn:TokenList -> float) =
        // for all items in the sentence
        // generate a list of candidate corrections
        // return a set of probable sentences
        // select the probable sentences with the highest likelihood
        // ------------------------------------------------------- //
        // generate a list of candidate corrections
        // return a set of probable sentences
        sentence |> Array.toList
        |>
        List.mapi(
                fun i (word:string) ->
                    let candidates = 
                        Edit1Counts.computeProbabilitiesWith word data vocab 
                        |>
                        List.fold 
                            (fun accum (replacement, prob) ->
                                
                                    let newlist =
                                        List.mapi (fun n (word':string) ->
                                                    match n = i with
                                                    | true -> replacement
                                                    | _ -> word') (sentence |> Array.toList) 
                                    ( ((replacement, prob), newlist)::accum ) |> List.rev )
                            List.empty

                    (i, word, candidates))
        // select the probable sentences with the highest likelihood
        |>
        List.fold (
                    fun ranked (i, word, candidates) ->
                            
                            let replaceCorrection (Correction (idx, word, updated)) sentence = 
                                    Array.mapi (fun i s ->
                                                match i = idx with
                                                | true -> 
                                                    let (Word (txt, prob)) = word
                                                    txt
                                                | _ -> s) sentence

                            List.fold (fun (accum, prevsentence) ((replacement, prob), newsentence) ->
                                            let sentenceprob = probabilityFn (newsentence |> List.toArray)
                                            
                                            let sentence' = 
                                                List.fold (fun ss candidate -> replaceCorrection candidate ss) prevsentence accum

                                            let originalprob = probabilityFn sentence'


                                            if sentenceprob > originalprob then
                                                ( (Correction (i, Word (replacement, prob), (sentenceprob, newsentence |> List.toArray))) :: accum, newsentence |> List.toArray ) 
                                            else 
                                               ( (Correction (i, Word (word, prob), (originalprob, sentence))) ::accum, sentence' ) ) (List.empty, sentence) candidates
                            
                            |> (fun collect -> seq { for item in (fst collect) do yield item } ) |> Seq.toList                      
                            // include only replacements that differ from the original word.
                            |> List.filter (fun (Correction (i, Word (replacement, prob), (sentenceprob, newsentence)):SpellingCorrection) ->
                                            word <> replacement )
                            // argmax P(w | W')
                            |> (fun list ->
                                    if (List.length list > 0) then
                                        let rank' =
                                            list |> 
                                            List.maxBy (
                                                fun (Correction (i, Word (replacement, prob), (sentenceprob, newsentence)):SpellingCorrection) ->
                                                    Math.Log(prob) + sentenceprob) 
                                        rank'::ranked
                                    else ranked)
                        ) List.empty
        // order by index of word in original sentence
        |> List.sortBy ( fun (Correction (i, Word (replacement, prob), (sentenceprob, newsentence)):SpellingCorrection) -> i)
        

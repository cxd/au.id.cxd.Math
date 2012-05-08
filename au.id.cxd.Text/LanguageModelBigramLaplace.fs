namespace au.id.cxd.Text

open System
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Tokens
open au.id.cxd.Text

module LanguageModelBigramLaplace = 

    type Count = float
    type InstanceCount = float

    /// <summary>
    /// A model representing bigram
    /// </summary>
    type BigramModel =
        Model of 
               Count * 
               InstanceCount * 
               Dictionary<string, Token> * 
               LanguageModelUnigramLaplace.UnigramModel

    
    /// <summary>
    /// Lift the model tuple out of the BigramModel type
    ///</summary>
    let extractModel (Model (cnt, inst, dataGram, unigram)) =
        (cnt, inst, dataGram, unigram)

    /// <summary>
    /// Train the bigram model
    /// includes an internal unigram model
    /// </summary>
    let train (data:Sentences) =
        let grams = new Dictionary<string, Token>()
        let unigram = LanguageModelUnigramLaplace.train data
        /// <summary>
        /// Given the current token list
        /// and a language module. 
        /// Update the counts in the language model for
        /// any bigram pairs appearing in the token list.
        /// </summary>
        let processWords (cnt, instance, grams, unigram) (words:TokenList) =
            List.append ("<s>" :: (words |> Array.toList) ) ["</s>"]
            |> List.map (fun word ->
                             ((cnt, instance, grams, unigram), word))
            |> List.reduce 
                (fun tupleA tupleB ->
                        let wordA = snd tupleA
                        let wordB = snd tupleB
                        
                        let key = String.Format("{0}+{1}",wordA, wordB)
                        let token = lookup grams key
                        let (cnt, instance, grams, unigram) = fst tupleB      
                        let cnt' =
                            match token with
                            | NoToken -> 
                                let token' = makeGram [| wordA; wordB |] 
                                             |> setCount 1.0
                                update grams key (NGram token')    
                                cnt + 1.0
                            | NGram token ->
                                let token' = setCount (token.Count + 1.0) token
                                update grams key (NGram token')
                                cnt
                        ((cnt', instance+1.0, grams, unigram), wordB)) 
            |> fst

        data 
        |> Seq.fold processWords 
                    (0.0, 0.0, new Dictionary<string, Token>(), unigram)
        |> (fun (cnt, instances, grams, unigrams) ->
            let (cnt', inst', grams') = LanguageModelUnigramLaplace.laplaceSmoothing cnt instances grams
            Model (cnt', inst', grams', unigrams))

    /// <summary>
    /// Using the bigrammodel
    /// calculate the probability of a sentence.
    /// Fallback onto the unigram model if the word does not exist
    /// </summary>
    let likelihood (model:BigramModel) (sentence:TokenList) =
        let (cnt, instances, grams, unigram) = extractModel model
        let (unicnt, uniinstances, unigrams) = LanguageModelUnigramLaplace.extractModel unigram
        let (total, grams, unigram, lastWord) =
            List.append ("<s>" :: (Array.toList sentence) ) ["</s>"]
            |> List.map (fun word -> (0.0, grams, unigram, word))
            |> List.reduce
                    (fun (total, grams, unigram, wordA) (total', grams', unigram', wordB) ->
                        let key = String.Format("{0}+{1}",wordA, wordB)
                        let token = lookup grams' key
                        let total' = 
                            match token with
                            | NoToken ->
                                // lookup unigram model
                                // but exclude start and end of sentence.
                                if (wordB <> "</s>") then
                                   let unitotal = LanguageModelUnigramLaplace.likelihood unigram' [|wordB|]
                                   total + unitotal
                                else total
                            | NGram gram ->
                                total + System.Math.Log(gram.Weight)
                        (total', grams', unigram', wordB))
        total

    /// <summary>
    /// Extract the vocabulary in the nested unigram model
    /// </summary>
    let vocab (model:BigramModel) =
        let (cnt, instances, grams, unigram) = extractModel model
        LanguageModelUnigramLaplace.vocab unigram
    
    /// <summary>
    /// Extract the bigram vocabulary 
    /// </summary>
    let bigramvocab (model:BigramModel) =
        let (cnt, instances, grams, unigram) = extractModel model
        seq { for key in grams.Keys do
                yield key }
        
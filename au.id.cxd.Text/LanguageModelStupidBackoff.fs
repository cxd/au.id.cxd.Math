namespace au.id.cxd.Text


open System
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser

module LanguageModelStupidBackoff = 
    
    
    
    /// <summary>
    /// Lift the model tuple out of the BigramModel type
    ///</summary>
    // let extractModel (Model (cnt, inst, dataGram, unigram)) =
    
    
    
    /// <summary>
    /// Train the bigram model
    /// includes an internal unigram model
    /// </summary>
    let train (data:Sentences) = 
        let size = 3
        let scanBuf = 
            (fun a b -> 
                let n = b::a
                if (List.length n > 3) then
                    n |> List.rev
                else n) 
            
        ()
        
    
    /// <summary>
    /// Using the bigrammodel
    /// calculate the probability of a sentence.
    /// Fallback onto the unigram model if the word does not exist
    /// </summary>
    // let likelihood (model:BigramModel) (sentence:TokenList) =
    
    /// <summary>
    /// Extract the vocabulary in the nested unigram model
    /// </summary>
    // let vocab (model:BigramModel) =
    
    /// <summary>
    /// Extract the bigram vocabulary 
    /// </summary>
    // let bigramvocab (model:BigramModel) =
    ()


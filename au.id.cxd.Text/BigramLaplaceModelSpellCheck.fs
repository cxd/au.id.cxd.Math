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

module BigramLaplaceModelSpellCheck =

    /// <summary>
    /// load the supplied model from file.
    /// </summary>
    let loadModel (fileName:string) =
        readData fileName :?> LanguageModelBigramLaplace.BigramModel

    /// <summary>
    /// Given the sentence 
    /// the error data list
    /// and the probability function
    /// compute the best match for corrections.
    /// </summary>
    let spellcheck (sentence:TokenList) (data:Dictionary<string,float>) (modelFn:unit -> LanguageModelBigramLaplace.BigramModel) =
        let model = modelFn()
        let vocab = LanguageModelBigramLaplace.vocab model
        ModelSpellCheck.spellcheck sentence (vocab |> Seq.toList) data (fun (sentence:TokenList) ->
                                                                        LanguageModelBigramLaplace.likelihood model sentence)

    /// <summary>
    /// A type provided to make the process of using a trained spell checker easier from C# 
    /// </summary>
    type BigramSpellChecker(editFile, bigramModelFile) =
        let editModel = Edit1Counts.readEditData editFile
        let bigramModel = loadModel bigramModelFile

        /// <summary>
        /// A single member exported to allow the user to check the spelling of a sentence.
        /// </summary>
        member s.CheckSpelling (sentence:TokenList) =
                let modelFn = (fun () -> bigramModel)
                spellcheck sentence editModel modelFn

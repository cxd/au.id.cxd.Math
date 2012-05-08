#r @"..\lib\PorterStemmerAlgorithm.dll"

#load "WordTokeniser.fs"
#load "Edit1Counts.fs"
#load "Tokens.fs"
#load "LanguageModelUnigramLaplace.fs"
#load "LanguageModelBigramLaplace.fs"
#load "ModelSpellCheck.fs"
#load "BigramLaplaceModelSpellCheck.fs"


open System
open System.IO
open System.Text
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Edit1Counts
open au.id.cxd.Text
open au.id.cxd.Text.Tokens

let dataFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\au.id.cxd.Text\count1edit.bin"
let baseDir = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\text-corpus\raw"
let dictionaryFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\text-spelling\sowpods.txt"

let dictionary = 
        let dict = new SortedDictionary<string,string>()
        seq { for line in defaultReadLine dictionaryFile do
                yield! line }
        |> Seq.fold (fun (dict':SortedDictionary<string,string>) (item:string) -> 
                                                if not (dict'.ContainsKey(item.ToLower())) then dict.[item.ToLower()] <- item
                                                else ()
                                                dict') dict

let cache = new Dictionary<string, bool>()
        

 // only allow words in the dictionary
let dictionaryFilter (line:string array) =
        Array.filter (fun (item:string) ->
                            let test = item.Trim()
                            if (cache.ContainsKey(test)) then cache.[test]
                            else 
                                let result = dictionary.ContainsKey(item.ToLower())
                                cache.[test] <- result
                                result) line

let readDir (dir:string) =
    let dInfo = new DirectoryInfo(dir)
    seq { for file in dInfo.GetFiles("*.txt") do
            yield file.FullName }

let readLines (fileName:string) = readLines emptyFilter dictionaryFilter defaultSplitter fileName

let readAllFiles =
    seq { for file in readDir baseDir do
            yield! readLines file }

let model = LanguageModelBigramLaplace.train readAllFiles    

let vocab = seq { for key in dictionary.Keys do yield key } |> Seq.toList

let editModel = Edit1Counts.readEditData dataFile

let results = List.map (fun item -> 
                            Edit1Counts.computeProbabilitiesWith item editModel vocab) 
                        ["msispelling";
                         "correct";
                         "tsting";
                         "irersponsible"]

let sentence = [|"another"; "exmple"; "of"; "text"; "used"; "to"; "evalate"; "the"; "preformance"; "of"; "an"; "algorithm"|]

let spelling = BigramLaplaceModelSpellCheck.spellcheck sentence editModel (fun () -> model)


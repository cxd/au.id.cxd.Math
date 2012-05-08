#r @"..\lib\PorterStemmerAlgorithm.dll"
#load "WordTokeniser.fs"
#load "Edit1Counts.fs"
#load "Tokens.fs"
#load "LanguageModelUnigramLaplace.fs"
#load "LanguageModelBigramLaplace.fs"


open System
open System.IO
open System.Text
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Edit1Counts
open au.id.cxd.Text
open au.id.cxd.Text.Tokens

let dataFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\au.id.cxd.Text\count1edit.bin"
let modelFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\au.id.cxd.Text\bigrammodel_test2.bin"
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

let (count, instances, bigram, unigram) = LanguageModelBigramLaplace.extractModel model

seq { for key in bigram.Keys do
        let token = bigram.[key]
        match token with
        | NGram gram -> 
            let sb = new StringBuilder()
            let sb' = Array.fold (fun (sb:StringBuilder) (item:string) -> sb.AppendFormat("{0} ", item)) sb gram.Tokens
            Console.WriteLine ("{0} {1}", sb', (gram.Weight * Math.Pow(2.0, 10.0)))
        | _ -> () } |> Seq.toList

WordTokeniser.storeData modelFile model
              
    
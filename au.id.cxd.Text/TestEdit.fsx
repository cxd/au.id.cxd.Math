#r @"..\lib\PorterStemmerAlgorithm.dll"
#load "WordTokeniser.fs"
#load "Edit1Counts.fs"

open System
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Edit1Counts

let file = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\text-spelling\count_1edit.txt"
let spellErrors = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\text-spelling\spell-errors.txt"
let dataFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\au.id.cxd.Text\count1edit.bin"

let splitVocab line = 
    let result = defaultSplitter line |> Array.toList
    [| List.head result |]

let splitErrors line = 
    let result = defaultSplitter line |> Array.toList
    [|(List.head result, List.tail result)|]


let filter (word:string) = 
    let txt = emptyFilter word
    txt.Trim([|':'|])


let noOp data = data

let filterErrors (word, errors) = 
    let word' = filter word
    (word', List.map( fun err -> filter err) errors)


let vocab = seq { for line in readLines filter lineFilter splitVocab spellErrors do 
                    yield! line } |> Seq.toList |> List.sort

let data = readEditData dataFile

// load some spelling mistakes
let errors = 
    seq { for line in readLines filterErrors noOp splitErrors spellErrors do 
                        yield! line }

// estimate some corrections
let corrections = 
    Seq.map (fun (word, (errors':string list)) ->
                List.fold (fun (success, total, result) (error:string) ->
                    let results = computeProbabilitiesWith error data vocab
                    List.iter (fun (newword, prob) ->
                                Console.WriteLine("{0} {1}", newword, prob)
                                ()) results
                    let success' =
                        if (List.length results > 0 && word = fst (List.head results)) then
                            success + 1.0
                        else success
                    (success', total + 1.0, results)) (0.0, 0.0, List.empty) errors'
                    ) errors
                |> Seq.toList         



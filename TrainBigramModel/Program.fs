open System
open Microsoft.FSharp.Text
open System.IO
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Edit1Counts
open au.id.cxd.Text

let args = Environment.GetCommandLineArgs()

let dictionaryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionary.txt")

let outputFile = ref "train.bin"
let inputFile = ref "input.txt"
let inputDir = ref "inputdir"
let mode = ref ""

let specs =
        ["--inputDir", ArgType.String (fun dir -> inputDir := dir), "The input directory to use for training"
         "--inputFile", ArgType.String (fun file -> inputFile := file), "The input file to load the edit model from"
         "--output", ArgType.String (fun file -> outputFile := file), "The output file to write the model into"
         "--mode", ArgType.String (fun m -> mode := m), "The mode use either EDIT or MODEL - this defaults to EDIT and expects an input file"] 
         |> List.map (fun (sh, ty, desc) -> ArgInfo(sh, ty, desc))

let makeedit inputFile outputFile = 
    Console.WriteLine("Building Edit Model")
    let data = parseTextEditData inputFile
    storeEditData outputFile data |> ignore
    Console.WriteLine("Wrote file {0}", outputFile)


let makemodel (inputDir:string) outputFile = 
    Console.WriteLine("Creating Bigram Model from {0} for all *.txt files", inputDir)

    
    let dictionary = 
            let dict = new SortedDictionary<string,string>()
            seq { for line in defaultReadLine dictionaryFile do
                    yield! line }
            |> Seq.fold (fun (dict':SortedDictionary<string,string>) (item:string) -> 
                                                    if not (dict'.ContainsKey(item.ToLower())) then dict.[item.ToLower()] <- item
                                                    else ()
                                                    dict') dict

    let readDir (dir:string) =
        let dInfo = new DirectoryInfo(dir)
        seq { for file in dInfo.GetFiles("*.txt") do
                yield file.FullName }
    
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

    let readLines (fileName:string) = readLines emptyFilter dictionaryFilter defaultSplitter fileName

    let readAllFiles =
        seq { for file in readDir inputDir do
                yield! readLines file }

    let model = LanguageModelBigramLaplace.train readAllFiles            

    WordTokeniser.storeData outputFile model |> ignore

    Console.WriteLine("Wrote Bigram Model to {0}", outputFile)
    ()

let execute (mode:string) = 
    Console.WriteLine("Execute with mode {0}", mode)
    match mode with
    | "EDIT" -> makeedit !inputFile !outputFile
    | "MODEL" -> makemodel !inputDir !outputFile
    | _ -> ArgParser.Usage(specs)
    ()

ArgParser.Parse(specs)
execute !mode 




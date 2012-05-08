open System
open Microsoft.FSharp.Text
open System.IO
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Edit1Counts
open au.id.cxd.Text
open au.id.cxd.Text.NaiveBayesTextClassifier
open au.id.cxd.Math.ClassifierMetrics
open au.id.cxd.Math.TrainingData

let args = Environment.GetCommandLineArgs()

let outputFile = ref "classifier.bin"
let stopwordFile = ref "stopwords.txt"
let inputDir = ref "inputdir"
let shouldStem = ref false
let folds = ref 3

let specs =
        [
         "--inputDir", ArgType.String (fun dir -> inputDir := dir), "The input directory to use for training"
         "--stopWords", ArgType.String (fun file -> stopwordFile := file), "The optional stopword file used to omit stop words from the corpus (line delimited one stopword per line)."
         "--output", ArgType.String (fun file -> outputFile := file), "The output file to write the model into" 
         "--stemTerms", ArgType.Set (shouldStem), "A flag used to train the model with stemmed words, note when the resulting model is used as a classifier the samples should also be stemmed."
         "--folds", ArgType.Int (fun n -> folds := n), "The number of k-folds to perform training"
         ]
         |> List.map (fun (sh, ty, desc) -> ArgInfo(sh, ty, desc))

/// create n-folds of trining data
let createTrainingFolds () =
    let stopWordsFn = stopwordFilter stopwordFile.Value 
    let wordFilter = 
        if (shouldStem.Value) then
            (fun (item:string) -> stopWordsFn item |> stemmerFilter)
        else stopWordsFn
    let (classes, classifiedData) = 
        readClassifiedDocuments inputDir.Value wordFilter lineFilter defaultSplitter
        // randomly permute the data
        |> (fun (cls, data) -> (cls, scramble data))
    
    let partCount = (Seq.length classifiedData) / folds.Value
    let (n, child, folds) =
        Seq.fold ( fun (n, childseq, accum) datum ->
                    if ((n % partCount) > 0) || (n = 0) then
                        let child = Seq.append (seq { yield datum }) childseq
                        (n+1, child, accum)
                    else 
                        let accum' = Seq.append (seq { yield childseq }) accum
                        (0, Seq.empty, accum')
                    ) (0, Seq.empty, Seq.empty) classifiedData
    // create a sequence of sequences 
    (Seq.toList classes, folds)

/// partition the data into training and test data.
let partitionData partition =
    let seqPartition = partition |> Array.toSeq
    let sampleCount = Seq.length seqPartition
    let percent = Convert.ToInt32(0.7 * Convert.ToDouble(sampleCount))
    Console.WriteLine("Partition Size {0} Training Size {1} Test Size {2}", sampleCount, percent, sampleCount - percent)
    
    let trainData = Seq.take percent seqPartition
    let testData = Seq.skip percent seqPartition
    (trainData, testData)

/// train and test the model
let trainAndTest (model:NaiveBayesTextModel) classes (trainData, testData) = 
    let model' = trainModel model trainData classes
    // test the model
    let (metrics, confusionMatrix) = testModel model' testData classes
    (model', metrics)

let runTraining () =
    let (classes, data) = createTrainingFolds () 
    Seq.fold (fun (model, metrics, n) datum ->
                  let partition = partitionData (datum |> Seq.toArray)
                  let (model', metrics') = trainAndTest model classes partition
                  Console.WriteLine("Accuracy: {0}, K: {1}, F-score: {2}", metrics'.Accuracy, metrics'.Kappa, metrics'.FScore)
                  (model', { Accuracy = (metrics.Accuracy+metrics'.Accuracy); Kappa = (metrics.Kappa+metrics'.Kappa); FScore = (metrics.FScore+metrics.FScore); }, n+1.0))
                  ((makeEmptyModel classes), {Accuracy = 0.0; Kappa = 0.0; FScore = 0.0;}, 0.0) data
    |> (fun (model, metrics, n) -> (model, { Accuracy = metrics.Accuracy/n; Kappa = metrics.Kappa/n; FScore = metrics.FScore/n; }, n))

let runTask () =
    if (inputDir.Value = "inputdir") then
        ArgParser.Usage(specs)
    else  
        let (model, metrics, k) = runTraining ()
        WordTokeniser.storeData outputFile.Value model |> ignore
        Console.WriteLine("{0} Training Folds", k)
        Console.WriteLine("Average Accuracy: {0}", metrics.Accuracy)
        Console.WriteLine("Average Kappa: {0}", metrics.Kappa)
        Console.WriteLine("Average FScore: {0}", metrics.FScore)

ArgParser.Parse(specs)
runTask ()





     




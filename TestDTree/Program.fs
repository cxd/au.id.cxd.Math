open System
open System.IO
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.Cart.CartAlgorithm
open au.id.cxd.Math.Cart.DecisionTreeXmlSerializer
open au.id.cxd.Math.Cart.DecisionTreeGraphMLSerializer
open au.id.cxd.Math.Cart.DTreeLearn3

let baseDir = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\telstra_analysis"
let sourceData = "duplicateAnalysis.csv"
let dataFile = "duplicateAnalysis_permute2.csv"
let testXml = "telstradtree.xml"
let telstraForest = "telstraforest.bin"

/// can permute the source data file to create a randomised sample space.
//permuteLinesInFile 1000 (Path.Combine(baseDir, sourceData)) (Path.Combine(baseDir, dataFile))

let attributes = [
{ AttributeLabel = "Fnn"; AttributeType = String; Column = 0; };
{ AttributeLabel = "Info"; AttributeType = Continuous; Column = 1; };
{ AttributeLabel = "HourDIff"; AttributeType = Continuous; Column = 2; };
{ AttributeLabel = "DiffBand"; AttributeType = String; Column = 3; };
{ AttributeLabel = "SameSec"; AttributeType = String; Column = 4; };
{ AttributeLabel = "SecDescription1"; AttributeType = String; Column = 5; };
{ AttributeLabel = "Catmap1"; AttributeType = String; Column = 6; };
{ AttributeLabel = "DataDate2"; AttributeType = String; Column = 7; };
{ AttributeLabel = "Cld2"; AttributeType = String; Column = 8; };
{ AttributeLabel = "Modulename2"; AttributeType = String; Column = 9; };
{ AttributeLabel = "Sec2"; AttributeType = String; Column = 10; };
{ AttributeLabel = "SecDescription2"; AttributeType = String; Column = 11; };
{ AttributeLabel = "Catmap2"; AttributeType = String; Column = 12; };
]
// remove the following columns Fnn (0), DataDate2 (7) and renumber  the attributes

let attributesNew attr = List.fold (fun accum (item:Attribute) -> 
                                        if item.Column <> 0 && item.Column < 7 then { AttributeLabel = item.AttributeLabel; AttributeType = item.AttributeType; Column = (item.Column - 1)} :: accum 
                                        else if item.Column <> 7 && item.Column > 7 then ({ AttributeLabel = item.AttributeLabel; AttributeType = item.AttributeType; Column = (item.Column - 2)} :: accum)
                                        else accum ) [] attr |> List.rev

let newColumns data = List.fold (fun accum (col:DataColumn) -> 
                                    if (col.Column <> 0 && col.Column < 7) then { Column = (col.Column - 1); RawData = col.RawData; ProcessedData = col.ProcessedData; } :: accum
                                    else if (col.Column <> 7 && col.Column > 7) then { Column = (col.Column - 2); RawData = col.RawData; ProcessedData = col.ProcessedData; } :: accum
                                    else accum) [] data |> List.rev



let trainingData = importTrainingData [|','|] 0.7 0 attributes (Path.Combine(baseDir, dataFile))
let trainNew = newColumns (snd trainingData.TrainData)

let testNew = newColumns (snd trainingData.TestData)

let learnNew = { ClassColumn = 3; TrainData = (attributesNew attributes, trainNew); TestData = (attributesNew attributes, testNew); }

let dtree = decisionTreeLearning 3 learnNew.TrainData "0" false

writeTreeToXml (Path.Combine(baseDir, testXml)) dtree

writeTreeToGraphML (Path.Combine(baseDir, "telstratree.graphml"))  "telstra1" dtree

let accuracy = testDTree learnNew dtree
Console.WriteLine("Accuracy: {0}", accuracy)


(*
let trainingSeq = importTrainingDataGroups 200 [|','|] 0.7 0 attributes (Path.Combine(baseDir, dataFile))

let trainNewSeq = Seq.map (fun tdata -> newColumns (snd tdata.TrainData)) trainingSeq

let testNewSeq = Seq.map (fun tdata -> newColumns (snd tdata.TestData)) trainingSeq

let learnNewSeq = 
    Seq.map (fun (train, test) -> { ClassColumn = 3; TrainData = (attributesNew attributes, train); TestData = (attributesNew attributes, train); })
            (Seq.zip trainNewSeq testNewSeq)


let (forest, resultList, avgAccuracy) = trainForest learnNewSeq "0" true (Path.Combine(baseDir, telstraForest))

Console.WriteLine("Accuracy: {0}", avgAccuracy)

let testForest = readSerializedForest (Path.Combine(baseDir, telstraForest))

Console.WriteLine("Accuracy: {0}", avgAccuracy)
*)
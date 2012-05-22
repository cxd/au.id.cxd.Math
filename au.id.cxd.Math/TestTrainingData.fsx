#light

#r @"C:\Program Files\FSharpPowerPack-2.0.0.0\\bin\FSharp.PowerPack.dll"
#r @"C:\Program Files\FSharpPowerPack-2.0.0.0\\bin\FSharp.PowerPack.Compatibility.dll"

#load "TestIO.fs"
#load "TrainingData.fs"


open System
open System.IO
open System.Xml
open System.Xml.Serialization
open au.id.cxd.Math.TrainingData

let baseDir = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\wine2"
let sourceData = "wine.data"
let dataFile = "wine_permute.csv"

/// can permute the source data file to create a randomised sample space.
///permuteLinesInFile (Path.Combine(baseDir, sourceData)) (Path.Combine(baseDir, dataFile))

let attributes = [
{ AttributeLabel = "Class"; AttributeType = String; Column = 0; };
{ AttributeLabel = "Alcohol"; AttributeType = Continuous; Column = 1; };
{ AttributeLabel = "Malic Acid"; AttributeType = Continuous; Column = 2; };
{ AttributeLabel = "Ash"; AttributeType = Continuous; Column = 3; };
{ AttributeLabel = "Alcalinity of ash"; AttributeType = Continuous; Column = 4; };
{ AttributeLabel = "Magnesium"; AttributeType = Continuous; Column = 5; };
{ AttributeLabel = "Total phenols"; AttributeType = Continuous; Column = 6; };
{ AttributeLabel = "Flavanoids"; AttributeType = Continuous; Column = 7; };
{ AttributeLabel = "Nonflavanoid phenols"; AttributeType = Continuous; Column = 8; };
{ AttributeLabel = "Proanthocyanins"; AttributeType = Continuous; Column = 9; };
{ AttributeLabel = "Color intensity"; AttributeType = Continuous; Column = 10; };
{ AttributeLabel = "Hue"; AttributeType = Continuous; Column = 11; };
{ AttributeLabel = "OD280/OD315 of diluted wines"; AttributeType = Continuous; Column = 12; };
{ AttributeLabel = "Proline"; AttributeType = Continuous; Column = 13; };
]

//let trainingData = importTrainingData [|','|] 0.7 0 attributes (Path.Combine(baseDir, dataFile))
//let serializer = new XmlSerializer(typeof<LearningData>)
//serializer.Serialize(new StreamWriter(Path.Combine(baseDir, "test.xml")), trainingData)
// TODO: write parser for customer data LearningData

let data = [{ IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = true; };
            { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = true; };
            { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = false; };
            { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = false; };
            { IntVal = 0; StringVal = "TEST"; FloatVal = 0.0; BoolVal = false; Missing = false; };
            { IntVal = 0; StringVal = "TEST"; FloatVal = 0.0; BoolVal = false; Missing = false; }]

let uniqueSet = List.fold (fun set item -> Set.add item set) Set.empty data


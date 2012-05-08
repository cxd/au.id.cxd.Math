// Learn more about F# at http://fsharp.net
open System
open System.IO
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.Cart.CartAlgorithm
open au.id.cxd.Math.Cart.DecisionTreeXmlSerializer
open System.Runtime.Serialization.Formatters.Binary

let baseDir = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\wine2"
let sourceData = "wine.data"
let dataFile = "wine_permute.csv"
let testXml = "winedtree.xml"
let testbin = "winedtree.bin"

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

let trainingData = importTrainingData [|','|] 0.7 0 attributes (Path.Combine(baseDir, dataFile))


let dtree = decisionTreeLearning 0 trainingData.TrainData "0" true
writeTreeToXml (Path.Combine(baseDir, testXml)) dtree
let writer = File.OpenWrite(Path.Combine(baseDir, testbin))
let serializer = new BinaryFormatter()
serializer.Serialize(writer, dtree)
writer.Flush()



let accuracy = testDTree trainingData dtree

Console.WriteLine("Accuracy: {0}", accuracy)
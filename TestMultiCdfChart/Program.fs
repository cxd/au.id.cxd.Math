open System
open System.Drawing
open System.IO
open System.Windows.Forms
open FSChart
open FSChart.Builder
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.UI.ChartUI
open au.id.cxd.Math.UI.CdfComparisonUI


/// load the file.
let loadData fileName = 
        let filter = (fun s -> s |> Seq.skip 1 |> Seq.filter (fun (item:string list) -> item.Length = 6))
        let data = readAndFilterFromCsv filter [|','|] fileName
        Console.WriteLine("Loaded Raw Data {0}", fileName)
        /// SessionId, SessionDurationMs, FromNode, ToNode, TransitionDuration, ConcurrentSessions
        let attributes = 
            [{ AttributeLabel = "Sessionid"; AttributeType = String; Column = 0; };
             { AttributeLabel = "SessionDurationMs"; AttributeType = Continuous; Column = 1};
             { AttributeLabel = "From"; AttributeType = String; Column = 2; };
             { AttributeLabel = "To"; AttributeType = String; Column = 3; };
             { AttributeLabel = "TransitionDurationMs"; AttributeType = Continuous; Column = 4};
             { AttributeLabel = "ConcurrentSessions"; AttributeType = Continuous; Column = 5};]
        (fileName, convertFromRawData 1.0 0 attributes data false)

let makeCdf column label colour data =
    let attributes = attributesInTable data.TrainData
    let attr = attributeAt column attributes
    let (x, y) = cdfFromHistogramCount attr data.TrainData
    (label, colour, x |> List.toArray, y |> List.toArray)

let drawChart cdfseries =
    createCdfChart cdfseries

let loadFiles fileList =
    Array.map (fun (fileName:string) -> 
                Console.WriteLine("Loading {0}", fileName)
                loadData fileName) fileList

let makeChart (form:Form) =
    let files = [|
                  @"C:\Users\cd\Projects\FSharp\au.id.cxd.Math\donotrelease-data\app_output\example1_inlier_aa.csv";
                  @"C:\Users\cd\Projects\FSharp\au.id.cxd.Math\donotrelease-data\app_output\example3_inlier_aa.csv";
                  @"C:\Users\cd\Projects\FSharp\au.id.cxd.Math\donotrelease-data\app_output\prod1_inlier_aa.csv";
                  |]
    Console.WriteLine("Loading Files")
    let dataSet = loadFiles files
    Console.WriteLine("Creating CDF Series")
    let series = Array.mapi(fun i (file, trainingData) -> 
                                let fInfo = new FileInfo(file)
                                let name = fInfo.Name
                                let colour = colours.[i]
                                makeCdf 4 name colour trainingData) dataSet
    drawChart (series |> Array.toList)




// a test for multiple cdf plots
// this uses a fixed format for the input csv files
// just for testing purposes 

let frm = new Form(ClientSize=Size(600,400))

let chart = makeChart frm
chart.Dock <- DockStyle.Fill
frm.Controls.Add(chart)
frm.Show()
Application.Run(frm)
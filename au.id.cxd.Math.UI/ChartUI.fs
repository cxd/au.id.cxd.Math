namespace au.id.cxd.Math.UI

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Forms.Integration
open System.IO.Packaging
open Microsoft.Win32
open au.id.cxd.Math.TextIO
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.UIBuilder.MathUIBuilder
open MathUIProject
open StateM
open UIApplication
open au.id.cxd.Math.DataSummary
open System.Drawing
open System.Windows
open System.Windows.Markup
open System.Windows.Forms.DataVisualization
open System.Windows.Media.Imaging
open System.Windows.Documents
open System.Reflection
open System.IO
open FSChart
open FSChart.Builder
open au.id.cxd.Math.UIBuilder.DocUI

module ChartUI =
    
    /// <summary>
    /// A set of colours for use in charting.
    /// </summary>
    let colours = [ gray; 
                    lightGray; 
                    gainsboro; 
                    red; 
                    darkRed; 
                    green; 
                    darkGreen; 
                    lightGreen; 
                    seaGreen; 
                    darkSeaGreen; 
                    lightSeaGreen;
                    mintCream;
                    blue;
                    lightBlue;
                    skyBlue;
                    deepSkyBlue;
                    lightSkyBlue;
                    steelBlue;
                    lightSteelBlue;
                    aliceBlue;
                    azure;
                    cyan;
                    darkCyan;
                    lightCyan;
                    magenta;
                    darkMagenta;
                    indigo;
                    blueViolet;
                    yellow;
                    lightYellow;
                    gold;
                    orange;
                    orangeRed;
                    darkOrange; ]
    
    /// <summary>
    /// Select a colour for the nth value.
    /// </summary>
    let selectColour n = 
        if n >= List.length colours then
            List.nth colours (n % (List.length colours)) 
        else List.nth colours n

    /// <summary>
    /// Return the blocks from the description fragment.
    /// </summary>
    let descriptionFragment =
        let stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleValueReport.xaml");
        let doc = XamlReader.Load(stream)
        let rec enumList (enum:System.Collections.IEnumerator) lst = 
            match enum.MoveNext() with
            | true -> enumList enum (enum.Current :: lst)
            | false -> lst
        enumList ((doc :?> FlowDocument).Blocks.GetEnumerator()) [] |> List.rev

    /// <summary>
    /// Make a temporary image.
    /// </summary>
    let makeImage (winChart:Charting.Chart) (attr:Attribute) =
        let filename = Path.Combine(Path.GetTempPath(), "attribute_"+attr.Column.ToString() + ".png")
        winChart.Width <- Convert.ToInt32( Application.Current.MainWindow.Width )
        //winChart.Height <- 600 //Convert.ToInt32(Application.Current.MainWindow.Height)
        winChart.SaveImage(filename, Charting.ChartImageFormat.Png);
        let decoder = BitmapFrame.Create(new Uri(filename), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)
        let img = new Windows.Controls.Image(Name = "attribute_"+attr.Column.ToString())
        img.Source <- decoder
        img

    /// <summary>
    /// Draw a table using the child doc functions.
    /// </summary>
    let drawTable columns data =
        let t = tableBordered 0.0 Media.Brushes.Black 0.0 10.0
        let tableCols = 
            List.map(fun (col:string) ->
                        childDoc (new TableColumn()) nochange)
                    columns
        let headerCells =
            List.map    (fun (col:string) ->
                            let para = para "Arial" 12.0 [(textBold col)]
                            cellBordered Media.Brushes.Black 0.5 10.0 para)
                         columns

        let datarows = List.fold (fun cells ((txt:string), (num:float)) ->
                                    let para = textPara "Arial" 12.0 txt
                                    let datum = textPara "Arial" 12.0 (num.ToString("0.000"))
                                    List.append [row [(cellBordered Media.Brushes.Black 0.5 10.0 para); (cellBordered Media.Brushes.Black 0.5 10.0 datum )] ] cells) [] data

        let rows = List.append 
                        [rowShaded Media.Brushes.LightGray headerCells] 
                        datarows
                        |> rowGroup
        
        t rows headerCells


    let drawTable1 columns rows =
        let table = new Table()
        table.CellSpacing <- 0.0
        table.BorderBrush <- Media.Brushes.Black
        table.BorderThickness <- new Thickness(1.0) 
        let rowgroup = new TableRowGroup()
        let row = new TableRow()
        row.Background <- Media.Brushes.LightGray
        List.iter (fun (col:string) -> 
                    table.Columns.Add(new TableColumn())
                    let cell = new TableCell()
                    cell.BorderBrush <- Media.Brushes.Black
                    cell.BorderThickness <- new Thickness(0.5)
                    let para = new Paragraph(FontFamily = new Media.FontFamily("Arial"), FontSize = 12.0)
                    let bold = new Bold()
                    bold.Inlines.Add(col)
                    para.Inlines.Add(bold)
                    cell.Blocks.Add(para)
                    row.Cells.Add(cell)) columns
        rowgroup.Rows.Add(row)
        List.iter (fun ((txt:string), (num:float)) ->
                        let row = new TableRow()
                        let cell1 = new TableCell()
                        cell1.BorderBrush <- Media.Brushes.Black
                        cell1.BorderThickness <- new Thickness(0.5)
                        let para1 = new Paragraph(FontFamily = new Media.FontFamily("Arial"), FontSize = 10.0)
                        para1.Inlines.Add(txt)
                        cell1.Blocks.Add(para1)
                        row.Cells.Add(cell1)
                        let cell2 = new TableCell()
                        cell2.BorderBrush <- Media.Brushes.Black
                        cell2.BorderThickness <- new Thickness(1.0)
                        let para2 = new Paragraph(FontFamily = new Media.FontFamily("Arial"), FontSize = 10.0)
                        para2.Inlines.Add(num.ToString("0.0000"))
                        cell2.Blocks.Add(para2)
                        row.Cells.Add(cell2)
                        rowgroup.Rows.Add(row)
                        ()) rows
        table.RowGroups.Add(rowgroup)
        table

    /// <summary>
    /// Build the chart for the given type.
    /// </summary>
    let makeChart (attr:Attribute) (data:DataTable) printFlag (parent:ContentElement) =
        
        let addIn = addDoc parent

        let host = new WindowsFormsHost()
        match attr.AttributeType with
        | String -> 
            
            let g   = grid ( solid lightSteelBlue 1 )
            
            let ax = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
            
            let area2 = 
                areaS |> axisX (labels ( arialR 8 ) ) 
                      |> axisY (ax >> linear 1.0)
                      |> backColor aliceBlue |> border steelBlue 1
                      |> legend ( bottom >> center >> transparent >> arialR 8 )
                      |> title  ( text attr.AttributeLabel >> arialR 14 >> color steelBlue )
           
            let chart = single
                             area2 
                             [SingleValueBarChart.singleValueBarChart attr data]


            let winChart = new Charting.Chart()

            MSChart.displayIn winChart chart

            if (printFlag) then
                winChart.Update()
                let img = makeImage winChart attr
                let block = new BlockUIContainer(Child = img)
                let child = childDoc (block :> ContentElement) nochange
                child addIn |> appendUI |> ignore

            else
                host.Child <- winChart
                winChart.Update()
                let block = new BlockUIContainer(Child = host)
                let child = childDoc (block :> ContentElement) nochange
                child addIn |> appendUI |> ignore
            
            // write information into a table.
            let uniqueCounts = stringValueCount attr data
            let labels1 = List.map(fun (n, s) -> s) uniqueCounts
            let counts = List.map(fun (n, s) -> n) uniqueCounts
            let table = drawTable ["Label";"Count";] (List.zip labels1 counts)
            table addIn |> appendUI |> ignore
            
                
        | Continuous
        | NumericOrdinal -> 
            let histcnts = histogramCount attr data

            let (x', y') = cdfFromHistogramCount attr data

            // plot an x y curve of volume for each point in the histogram.
            let len = List.length histcnts
            // y - the count of values within each low and high pair of the range.
            let y = List.map(fun ((n:int), (low, high)) -> Convert.ToDouble(n)) histcnts |> List.toArray
            
            let total = Array.fold (+) 0.0 y

            let y2 = Array.map(fun n -> 100.0 * (n/total)) y
            
            // the high value of each pair in the range
            let x = List.map(fun (n, (low, high)) -> high) histcnts |> List.toArray
            
            // the gaussian distribution.
            let gauss = gaussiandist attr data

            let gaussX = List.mapi (fun i x -> (Convert.ToDouble(i)) - (Convert.ToDouble( List.length gauss ) / 2.0)) gauss

            // use the candle function to chart the population of this attribute.
            let n = List.map(fun (datum:Datum) -> datum.FloatVal) (columnAt attr.Column data).ProcessedData 
                    |> List.toArray

            
            let ax = labels ( arialR 8 >> format "F1" ) >> grid ( solid lightSteelBlue 1 )
                
            let area = 
                areaF |> axisX ( ax >> linear 1.0 ) 
                      |> axisY ( ax >> linear 1.0 ) 
                      |> backColor aliceBlue
                      |> legend ( top >> near >> transparent >> arialR 8 )
            
            let series1 = bar 1.0
                            |> border darkBlue 1 
                            |> text ( ((new System.Text.StringBuilder()).AppendFormat("Count of {0}", attr.AttributeLabel)).ToString() )
            
            let cdfseries = line 
                            |> color red 
                            |> text "CDF" 

            // values for reporting
            let u = mean attr data
            let std = stddev attr data
            let vari = variance attr data
            let datamin = dataMin attr data
            let datamax = dataMax attr data

            let fmt (num:float) = num.ToString("0.000")

            let seriesgauss = line 
                              |> text ("Gauss STDEV = " + (fmt std) + " MEAN = " + (fmt u))

            let chart =
                 lattice (0.0, 0.1, 0.03, 0.0) (0.0, 0.1, 0.0, 0.0)
                    [ 
                        [area, [ series1, SeriesData.XY (x, y2) ];
                         area, [ seriesgauss, SeriesData.XY(gaussX |> List.toArray, gauss |> List.toArray)];
                         area, [cdfseries, SeriesData.XY (x' |> List.toArray, y' |> List.toArray)] ]
                    ]
                 |> title  ( text attr.AttributeLabel >> arialR 14 >> color steelBlue )

            let winChart = new Charting.Chart()

            MSChart.displayIn winChart chart
            
            if (printFlag) then
                let img = makeImage winChart attr
                let block = new BlockUIContainer(Child = img)
                let child = childDoc (block :> ContentElement) nochange
                child addIn |> appendUI |> ignore

            else
                host.Child <- winChart
                winChart.Update()
                let block = new BlockUIContainer(Child = host)
                let child = childDoc (block :> ContentElement) nochange
                child addIn |> appendUI |> ignore

            let table = drawTable ["Attribute";"Value";] 
                                  [("Min", datamin);
                                   ("Mean", u);
                                   ("Max", datamax);
                                   ("Variance", vari);
                                   ("Standard Devation", std)]

            table addIn |> appendUI |> ignore


        | Bool -> 
                let block = new BlockUIContainer()
                let child = childDoc (block :> ContentElement) nochange
                child addIn |> appendUI |> ignore

    /// <summary>
    /// Display information for each column available in the application.
    /// Firstly display basic information about each kind of attribute.
    /// This will be displayed within a stack panel.
    /// Each grid component will host a summary chart.
    /// </summary>
    let chart (appState:UIState) printFlag : FlowDocument =
        
        let doc = new FlowDocument()
        doc.Background <- Media.Brushes.White
        
        descriptionFragment |> List.iter (fun block -> doc.Blocks.Add(block :?> Block) )

        
        let data = convertFromRawData appState.TrainPercent appState.ClassColumn.Column appState.Attributes appState.Data
        
        // process each attribute.
        List.iter(fun (attr:Attribute) -> 
                   if (appState.IsIgnored(attr.Column)) then ()
                   else 
                       let addTodoc = addDoc doc
                       let section = childDoc (new Section() :> ContentElement) 
                                              (fun parent (child:ContentElement) ->
                                                let s = (child :?> Section)
                                                s.BorderBrush <- Media.Brushes.Black
                                                s.BorderThickness <- new Thickness(1.0)

                                                let addIn = addDoc child
                                                let para = childDoc (new Paragraph(FontFamily = new Media.FontFamily("Arial"), FontSize = 14.0) :> ContentElement)
                                                                    (fun parent (child:ContentElement) ->
                                                                        let p = (child :?> Paragraph)
                                                                        let bold = new Bold()
                                                                        bold.Inlines.Add(String.Format("Column {0}, Name {1}", attr.Column,attr.AttributeLabel))
                                                                        p.Inlines.Add(bold)                                        
                                                                        parent)
                                                para addIn |> appendUI |> ignore
                                                
                                                // draw the chart.    
                                                makeChart attr data.TrainData printFlag s

                                                parent)
                                     
                       
                       let linebreak = childDoc (new LineBreak() :> ContentElement) nochange
                       
                       section addTodoc |> 
                       (appendUI 
                       >> linebreak
                       >> appendUI)
                       |> ignore

                       ) appState.Attributes
        
        doc

         

        
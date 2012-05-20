namespace au.id.cxd.Math

open System
open System.Data
open System.IO
open TextIO

module RawData =

    /// <summary>
    /// A raw data set comprises of columns of strings.
    /// </summary>
    type RawDataSet = { RawData: string list seq; Columns:int; Rows:int }

    /// <summary>
    /// An empty data set.
    /// </summary>
    let emptyDataSet = { RawData = Seq.empty; Columns = 0; Rows = 0; }

    /// <summary>
    /// Read a csv file and convert it to a RawDataSet
    /// </summary>
    let readFromCsv delimiter file = 
        let data = Seq.map(fun (line:string) -> delimitedTokens delimiter line) (readFile file)
        let cols = if (Seq.length data > 0) then List.length (Seq.head data)
                   else 0
        { RawData = data ; Columns = cols; Rows = Seq.length data; }

    /// <summary>
    /// Read a csv file and convert it to a RawDataSet
    /// </summary>
    let readAndFilterFromCsv (filterFn:string list seq -> string list seq) delimiter file = 
        let data = Seq.map(fun (line:string) -> delimitedTokens delimiter line) (readFile file) 
                   |> filterFn 
        let cols = if (Seq.length data > 0) then List.length (Seq.head data)
                   else 0
        { RawData = data; Columns = cols; Rows = Seq.length data; }

    /// <summary>
    /// Convert the raw data to a data set.
    /// </summary>
    let convertToDataSet (rawDS:RawDataSet) = 
        let dataSet = new DataSet()
        let dataTable = new DataTable()
        let n = [0..rawDS.Columns]
        List.iter (fun i -> 
                        let col = new DataColumn(i.ToString())
                        dataTable.Columns.Add(col)) n
        Seq.iter (fun row -> 
                    if (List.length row) = rawDS.Columns then
                        let dataRow = dataTable.NewRow()
                        List.iteri (fun i item -> 
                                    dataRow.[i] <- item
                                    ()) 
                                    row
                        dataTable.Rows.Add(dataRow)
                    else ()
                    ) rawDS.RawData
        dataSet.Tables.Add(dataTable)
        dataSet

    /// <summary>
    /// Filter the raw data set and remove the ignored columns.
    /// </summary>
    let filterIgnoreColumns (rawDS:RawDataSet) (ignore:int List) =
        let rec makeSet n (ss:string list seq) (src:string list seq) = 
            if (Seq.isEmpty src) then ss
            else if (not (List.exists (fun a -> a = n) ignore)) then
                makeSet (n+1) (Seq.append ss (Seq.take 1 src)) (Seq.skip 1 src)
            else makeSet (n+1) ss (Seq.skip 1 src)
        { RawData = (makeSet 0 Seq.empty rawDS.RawData); Columns = List.length ignore; Rows = rawDS.Rows }
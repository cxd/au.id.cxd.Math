namespace au.id.cxd.Math

open System
open System.Data
open System.IO
open TextIO

module RawData =

    /// <summary>
    /// A raw data set comprises of columns of strings.
    /// </summary>
    type RawDataSet = { RawData: string list list; Columns:int; Rows:int }

    /// <summary>
    /// An empty data set.
    /// </summary>
    let emptyDataSet = { RawData = List.empty; Columns = 0; Rows = 0; }

    /// <summary>
    /// Read a csv file and convert it to a RawDataSet
    /// </summary>
    let readFromCsv delimiter file = 
        let data = Seq.map(fun (line:string) -> delimitedTokens delimiter line) (readFile file) |> Seq.toList
        let cols = if (data.Length > 0) then List.length (List.head data)
                   else 0
        { RawData = data; Columns = cols; Rows = data.Length; }

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
        List.iter (fun row -> 
                    let dataRow = dataTable.NewRow()
                    List.iteri (fun i item -> 
                                dataRow.[i] <- item
                                ()) 
                                row
                    dataTable.Rows.Add(dataRow)
                    ) rawDS.RawData
        dataSet.Tables.Add(dataTable)
        dataSet

    /// <summary>
    /// Filter the raw data set and remove the ignored columns.
    /// </summary>
    let filterIgnoreColumns (rawDS:RawDataSet) (ignore:int List) =
        let rec makeSet n ss = 
            match ss with
            | (s::rest) -> if (not (List.exists (fun a -> a = n) ignore)) then
                            s :: (makeSet (n+1) rest)
                           else (makeSet (n+1) rest)
            | [] -> []
        { RawData = (makeSet 0 rawDS.RawData); Columns = List.length ignore; Rows = rawDS.Rows }
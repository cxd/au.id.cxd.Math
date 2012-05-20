namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open RawData

module TrainingData =

    /// <summary>
    /// An attribute definition defines the kinds of data that can be processed
    /// User attributes are defined prior to creating training data.
    /// </summary>
    type AttributeDefinition =      
                                   /// <summary>
                                   /// A numeric ordinal is a whole number value.
                                   /// </summary>
                                   NumericOrdinal
                                   /// <summary>
                                   /// A string is a free text field recorded in the data.
                                   /// </summmary>
                                   | String
                                   /// <summary>
                                   /// A continuous value is a real number value.
                                   /// </summary>
                                   | Continuous
                                   /// <summary>
                                   /// Boolean data type
                                   /// Either true or false. 
                                   /// </summary>
                                   | Bool

    /// <summary>
    /// Get a label for the definition
    /// </summary>
    let labelOfDefinition def =
            match def with
            | NumericOrdinal -> "NumericOrdinal"
            | String -> "String"
            | Continuous -> "Continuous"
            | Bool -> "Bool"
    

    
    /// <summary>
    /// Each attribute in the data set is identified by
    /// a name, a type and the column where it appears in the tabular data set.
    /// </summary>
    type Attribute = { 
                        /// <summary>
                        /// This is the name of the attribute.
                        /// </summary>
                        AttributeLabel : string; 
    
                        /// <summary>
                        /// This is the kind of the attribute.
                        /// </summary>
                        AttributeType: AttributeDefinition; 
    
                        /// <summary>
                        /// This is the column where the attribute occurs.
                        /// The column should be unique.
                        /// </summary>
                        Column: int; }

    /// <summary>
    /// A column definition that identifies the column containing the class label within the set.
    /// </summary>
    type ClassificationAttribute = { 
                                    /// <summary>
                                    /// The classification label associated with the column.
                                    /// </summary>
                                    Label: string;
                                    /// <summary>
                                    /// The index of the column containing the classification attribute
                                    /// </summary>
                                    Column: int; }

    /// <summary>
    /// A set of attributes used to describe the data.
    /// </summary>
    type AttributeList = Attribute List

    /// <summary>
    /// Identify the attribute at the supplied position
    /// </summary>
    let attributeAt n (attrs:AttributeList) = List.nth attrs n

    /// <summary>
    /// Set an attribute for a given column
    /// </summary>
    let setAttributeAt n (attrs:AttributeList) attr =
        List.mapi (fun i at -> if (i.Equals(n)) then attr
                               else at) attrs

    /// <summary>
    /// The data type that describes any of the accepted values within
    /// the table of data.
    /// </summary>
    type Datum = {  IntVal: int;
                    StringVal: string;
                    FloatVal: float;
                    BoolVal: bool;
                    Missing: bool; }
    
    /// <summary>
    /// All raw data when it is initially read is a string.
    /// </summary>
    type DataColumn = { Column: int; RawData: string List; ProcessedData: Datum List; }
    
    /// <summary>
    /// An index in a table is defined as the (row, column) tuple.
    /// </summary>
    type Index = int * int

    /// <summary>
    /// All data associated with the set of data to train on.
    /// </summary>
    type DataTable = AttributeList * DataColumn List

    /// <summary>
    /// Make an empty data table
    /// </summary>
    let makeEmptyTable attrList = 
        let l = [1..(List.length attrList)]
        (attrList, List.map(fun n -> {Column = n - 1; RawData = List.empty; ProcessedData = List.empty }) l )
    
    /// <summary>
    /// filter attribute list with ignore list.
    /// </summary>
    let filterAttributes (attrList:AttributeList) ignore =
        List.filter (fun (attr:Attribute) -> not (List.exists(fun n -> n = attr.Column) ignore)) attrList

    /// <summary>
    /// Access the data table attributes.
    /// </summary>
    let attributesInTable (dataTable:DataTable) = fst dataTable

    /// <summary>
    /// Access the data stored in the data table.
    /// </summary>
    let dataInTable (dataTable:DataTable) = snd dataTable

    /// <summary>
    /// Select the column at the supplied index n
    /// </summary>
    let columnAt n (dataTable:DataTable) = 
            List.nth (snd dataTable) n

    /// <summary>
    /// Determine whether the supplied table of data was empty.
    /// </summary>
    let isEmpty classCol (dataTable:DataTable) =
        let classes = columnAt classCol dataTable
        (List.length classes.RawData).Equals(0)
        

    /// <summary>
    /// All data used to perform learning with the algorithm.
    /// </summary>
    type LearningData = { ClassColumn: int; TrainData: DataTable; TestData: DataTable; }
    
    /// <summary>
    /// Convert a string to double 
    /// and automatically replace the value.
    /// if unsuccessful return 0.0
    /// </summary>
    let convert v defaultVal convertFn =
        if String.IsNullOrEmpty(v) then
            defaultVal
        else 
            try 
                convertFn(v)
            with
                | _ -> defaultVal


    /// <summary>
    /// Convert a string to double 
    /// and automatically replace the value.
    /// if unsuccessful return 0.0
    /// </summary>
    let convertToDouble v =
        convert v 0.0 (fun n -> Convert.ToDouble(n))

    let convertToInt v =
        convert v 0 (fun n -> Convert.ToInt32(n))

    let convertToBool v =
        convert v false (fun n -> Convert.ToBoolean(n))


    /// <summary>
    /// Define the value for the data in Index (row, column)
    /// </summary>
    let defineValue attr v = 
                    match attr with
                            | NumericOrdinal -> 
                                if (System.String.IsNullOrEmpty(v)) then   
                                    { IntVal = 0; StringVal = "0"; FloatVal = 0.0; BoolVal = false; Missing = true; }
                                else 
                                    { IntVal = convertToInt(v); StringVal = null; FloatVal = convertToDouble(v); BoolVal = false; Missing = false; }                            
                            | String -> 
                                if (System.String.IsNullOrEmpty(v)) then   
                                    { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = true;  }
                                else 
                                    { IntVal = 0; StringVal = v.ToLower(); FloatVal = 0.0; BoolVal = false; Missing = false; }                            
                            | Continuous -> 
                                if (System.String.IsNullOrEmpty(v)) then   
                                    { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = true; }
                                else 
                                    { IntVal = 0; StringVal = ""; FloatVal = convertToDouble(v); BoolVal = false; Missing = false; }                            

                            | Bool -> 
                                if (System.String.IsNullOrEmpty(v)) then   
                                    { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = false; Missing = true; }
                                else 
                                    { IntVal = 0; StringVal = ""; FloatVal = 0.0; BoolVal = convertToBool(v); Missing = false; }

    /// <summary>
    /// Define the data for a column n with the given attribute for the raw input data list.
    /// </summary>
    let defineColumn colN attr (data:string List) = 
            { Column = colN; RawData = data; ProcessedData = List.mapi (fun i item -> defineValue attr.AttributeType item) data }

    
    /// <summary>
    /// read csv data into the training data table
    /// partition it based on the training percent.
    /// attributes are defined ahead of time.
    /// </summary>
    let importTrainingData delimiter trainPartitionPercent classColumnIndex (attributes:AttributeList) csvFileName = 
        let lines = (readFile csvFileName)
        let sampleCount = Seq.length lines
        let percent = Convert.ToInt32(trainPartitionPercent * Convert.ToDouble(sampleCount))
        let trainData = Seq.take percent lines
        let testData = Seq.skip percent lines
        let colState dataSeq = 
            Seq.fold ( fun (colState:(string list)[]) (line:string) -> 
                                let tokens = Array.map (fun (token:string) -> token.Replace("'", "").Replace("\"", "") ) (line.Split(delimiter))
                                fst (List.fold (fun (colState, n) token -> 
                                                    let col = colState.[n]
                                                    colState.[n] <- List.append [token] col
                                                    (colState, n + 1)) (colState, 0) (Array.toList tokens) )
                      ) (Array.map (fun item -> List.empty) (List.toArray attributes)) dataSeq
        { ClassColumn = classColumnIndex; 
          TrainData = (attributes, List.mapi2 defineColumn attributes (colState trainData |> Array.toList)); 
          TestData = (attributes, List.mapi2 defineColumn attributes (colState testData |> Array.toList)); }
    
    /// <summary>
    /// Build multiple training data sets out of the multiple groups in data set each group contain at least minRecords.
    /// </summary>
    let importTrainingDataGroups minRecords delimiter trainPartitionPercent classColumnIndex (attributes:AttributeList) csvFileName = 
        // partition the sequences.
        let partitions = 
            let sets = Seq.fold (fun (n, set, curseq) (item:string) -> 
                                        if (n >= minRecords) then
                                            (0, (Seq.append set (seq [ curseq])), (Seq.append Seq.empty (seq [ item ])) )
                                        else 
                                            (n + 1, set, (Seq.append curseq (seq [ item ])) ) ) (0, Seq.empty, Seq.empty) (readFile csvFileName)
            let extract (n, set, curseq) = if (n < minRecords) then Seq.append set (seq [ curseq])
                                           else set
            extract sets
        Seq.map (fun lines -> 
                        let sampleCount = Seq.length lines
                        let percent = Convert.ToInt32(trainPartitionPercent * Convert.ToDouble(sampleCount))
                        let trainData = Seq.take percent lines
                        let testData = Seq.skip percent lines
                        let colState dataSeq = 
                            Seq.fold ( fun (colState:(string list)[]) (line:string) -> 
                                                let tokens = Array.map (fun (token:string) -> token.Replace("'", "").Replace("\"", "") ) (line.Split(delimiter))
                                                fst (List.fold (fun (colState, n) token -> 
                                                                    let col = colState.[n]
                                                                    colState.[n] <- List.append [token] col
                                                                    (colState, n + 1)) (colState, 0) (Array.toList tokens) )
                                      ) (Array.map (fun item -> List.empty) (List.toArray attributes)) dataSeq
                        { ClassColumn = classColumnIndex; 
                          TrainData = (attributes, List.mapi2 defineColumn attributes (colState trainData |> Array.toList)); 
                          TestData = (attributes, List.mapi2 defineColumn attributes (colState testData |> Array.toList)); }) partitions
        
    // this little method taken from
    // http://fsharp-code.blogspot.com/2010/11/random-permutation-on-sequences_06.html
    // this is very similar to the fisher yates shuffle
    // problem with this implementation is that the shuffle is not in place
    // it requires generating subsequences which adds extra time
    let scramble (sqn : seq<'a>) = 
        let rnd = new Random()
    
        let lst = Seq.toList sqn
        let len = List.length lst
        // this process of using a new guid is sourced from
        // http://www.codinghorror.com/blog/2007/12/shuffling.html
        Seq.sortBy (fun item -> Guid.NewGuid() ) sqn
        (*
        // this remove function is probably the bottle kneck O(n) to visit each node in sequence in order to filter    
        let remove n sqn = sqn |> Seq.filter (fun x -> x <> n)
        let rec innerScramble (sqn: seq<'a>) n =
            seq {
                let x = sqn |> Seq.nth (rnd.Next(0, (sqn |> Seq.length)  - 1))
                yield x
                let rem = remove x sqn
                if not (rem |> Seq.isEmpty) then
                    yield! innerScramble rem (n-1)
            }
        innerScramble sqn (Seq.length sqn) *)
        
    /// <summary>
    /// Permute the lines in a supplied file.
    /// this is useful for rotating input data.
    /// </summary>
    let permuteLinesInFile (fileIn:string) (fileOut:string) =
        let fileOut = new StreamWriter(fileOut)
        Seq.iter (fun (line:string) -> fileOut.WriteLine(line)) (readFile fileIn |> scramble)
        fileOut.Flush()
        fileOut.Close()



    /// <summary>
    /// Convert the raw data to training data.
    /// </summary>
    let convertFromRawData trainPartitionPercent classColumnIndex (attributes:AttributeList) (rawData:RawDataSet) skipFirst =
        let seqdata = if skipFirst then Seq.skip 1 rawData.RawData
                      else rawData.RawData
        let sampleCount = rawData.Rows
        let percent = Convert.ToInt32(trainPartitionPercent * Convert.ToDouble(sampleCount))
        let trainData = Seq.take percent seqdata
        let testData = Seq.skip percent seqdata
        
        let colState dataSeq = 
            Seq.fold ( fun (colState:(string list)[]) tokens -> 
                                fst (List.fold (fun (colState, n) token -> 
                                                    let col = colState.[n]
                                                    colState.[n] <- List.append [token] col
                                                    (colState, n + 1)) (colState, 0) tokens )
                      ) (Array.map (fun item -> List.empty) (List.toArray attributes)) dataSeq
        
        let train = (colState trainData |> Array.toList)
        let test = (colState testData |> Array.toList)

        { ClassColumn = classColumnIndex;
          TrainData = (attributes, List.mapi2 defineColumn attributes train); 
          TestData = (attributes, List.mapi2 defineColumn attributes test); }

    /// <summary>
    /// The text for the corresponding attribute type.
    /// </summary>
    let attributeTypeText t =
        match t with
        | Continuous -> "Continuous"
        | NumericOrdinal -> "NumericOrdinal"
        | String -> "String"
        | Bool -> "Bool"

    /// <summary>
    /// Select the unique classes from the sample data.
    /// </summary>
    let uniqueClasses classCol (data:DataTable) =
        let classList = (List.nth (snd data) classCol).RawData
        List.fold (fun set (item:string) -> Set.add (item.ToLower()) set) Set.empty classList

    /// <summary>
    /// Select the set of unique values for the supplied from the sample data.
    /// </summary>
    let uniqueAttributeValues (attr:Attribute) (data:DataTable) = 
            let dataList = (List.nth (snd data) attr.Column).ProcessedData
            List.fold (fun set item -> Set.add item set) Set.empty dataList
            
    
    /// <summary>
    /// Count the occurance of supplied class label in the data table.
    /// </summary>
    let countClass (classLabel:string) classColumn (data:DataTable) = 
        List.fold (fun n (item:string) -> match (item.ToLower().Equals(classLabel)) with
                                            | true -> n + 1
                                            | false -> n + 0) 0 ((columnAt classColumn data).RawData)

    /// <summary>
    /// Take the string attribute and convert it to a series of numbers.
    /// This will return a set of pairs (int * string) where the int is the count of values for the string.
    /// and the snd group is the string value converted to the corresponding int for each column in the data set.
    /// </summary>
    let stringAttributeToNumericKey (attr:Attribute) (data:DataTable) : (float * string) list * float list = 
        let col = columnAt attr.Column data
        let unique = uniqueAttributeValues attr data 
                     |> Set.toList 
                     |> List.map (fun datum ->
                                        datum.StringVal)
        let nums = List.zip (List.map (fun (n:Int32) -> System.Convert.ToDouble(n)) [1..(List.length unique)]) unique
        let countForString stringValue = List.find (fun (n, s) -> s.Equals(stringValue)) nums |> fst
        (nums, List.map (fun col -> countForString col.StringVal) col.ProcessedData)
        

    /// <summary>
    /// Generate the N-1 histogram of sorted X where histogram is [(X.1, X.2), (X.2, X.3), (X.3, X.4) ... (X.n-1, X.n) ]
    /// This can then be fed into a function to approximate probability of a continuous value
    /// by measuring the count (X.n-1 >= x <= X.n) / |X|
    /// </summary>
    let histogram (attr:Attribute) (data:DataTable) =
           let vals = 
                match attr.AttributeType with
                | Continuous ->  List.map (fun (datum:Datum) -> datum.FloatVal) ((columnAt attr.Column data).ProcessedData)
                | NumericOrdinal -> List.map (fun (datum:Datum) -> Convert.ToDouble(datum.IntVal)) ((columnAt attr.Column data).ProcessedData)
                | _ -> List.empty
           let sortedA = (List.sort vals) |> Set.ofList |> Set.toSeq
           let sortedB = (List.sort vals) |> Set.ofList |> Set.toSeq
           let skip = Seq.append (Seq.skip 1 sortedB) [0.0]
           let test = Seq.length skip
           let hist = Seq.map2 (fun v1 v2 -> (v1, v2) ) sortedA skip 
                      |> Seq.take ((Seq.length sortedA) - 1) 
                      |> Seq.toList
           if (List.length hist).Equals(0) then [(0.0, Seq.head sortedA)]
           else hist

    /// <summary>
    /// Count the frequency of each histgram range in the data set.
    /// </summary>
    let histogramCount (attr:Attribute) (data:DataTable) =
        let histcnts = histogram attr data |> List.map (fun h -> (0, h))
        let col = columnAt attr.Column data
        let vals = 
                match attr.AttributeType with
                | Continuous ->  List.map (fun (datum:Datum) -> datum.FloatVal) ((columnAt attr.Column data).ProcessedData)
                | NumericOrdinal -> List.map (fun (datum:Datum) -> Convert.ToDouble(datum.IntVal)) ((columnAt attr.Column data).ProcessedData)
                | _ -> List.empty
        List.map(fun (n, (low, high)) -> 
                        let cnt = List.fold(fun n f -> 
                                                if (f >= low && f <= high) then n + 1
                                                else n) 0 vals
                        (cnt, (low, high))) histcnts

    /// <summary>
    /// Calculate the continuous distribution function
    /// from the population.
    /// This gives the same size of x in the population of n..N where
    /// x = n
    /// Returns the (x series, y series) of the binned data
    /// </summary>
    let cdfFromHistogramCount (attr:Attribute) (data:DataTable) =
        let histcnts = histogramCount attr data
        let len = List.length histcnts
        // y - the count of values within each low and high pair of the range.
        let y = List.map(fun ((n:int), (low, high)) -> Convert.ToDouble(n)) histcnts
        let total = List.fold (+) 0.0 y
        //
        (
        // compute the x series 
         List.map (fun (n, (low, high)) -> high) histcnts,
         // compute the CDF curve percent of the population.
         List.mapi (fun i n -> List.fold (fun (index, total) k -> if (index <= i) then
                                                                    (index + 1, total + k)
                                                                  else (index, total)) (0, 0.0) y 
                               |> snd 
                               |> (fun r -> 100.0 * (r / total))) y)
    /// <summary>
    /// Calculate the continuous distribution function
    /// from the population.
    /// This gives the same size of x in the population of n..N where
    /// x = n
    /// Series is a population of values.
    /// Returns the y series of the binned data
    /// for an unknown X series
    /// </summary>
    let cdfFromSeries series =
        let total = List.fold (+) 0.0 series
        List.mapi (fun i n -> List.fold( fun (index, total) k -> 
                                                    if (index <= i) then
                                                      (index + 1, total + k)
                                                    else (index, total)) (0, 0.0) series
                                       |> snd
                                       |> (fun result -> 100.0 * (result / total)) ) series
        
    /// <summary>
    /// Count the occurance of an instance in the data set based on the attribute type
    /// </summary>
    let countAttributeValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            let data = (columnAt attr.Column examples).ProcessedData
            match attr.AttributeType with
                | Continuous 
                | NumericOrdinal -> 
                    // continuous and numeric ordinal are processed in the same way.
                    // 
                    // bin the values in the data set X into [(X.1, X.2) .. (X.n-1, X.n)]
                    // then find the pair that bounds the instance value
                    // then approximate the probability by counting the number of samples that fall inside the same
                    // range and divide by the total number of samples
                    // (a, b) = [(i,j) | i <= x <= j, (i,j) <- histogram]
                    // K = [k | a <= k <= b, k <- X]
                    // P(x.i) ~= |K| / |X|
                    let hist = histogram attr examples
                    // find the pair that contains the instance
                    let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.FloatVal >= lt && datum.FloatVal <= gt then n + 1.0
                                                       else n) 0.0 data)
                | String -> 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.StringVal.Equals(instance.StringVal) then n + 1.0
                                                       else n) 0.0 data)
                | Bool -> 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.BoolVal.Equals(instance.BoolVal) then n + 1.0
                                                       else n) 0.0 data)

    /// <summary>
    /// Calculate the probability of an instance in the data set based on the attribute type
    /// </summary>
    let probabilityAttributeValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            let data = (columnAt attr.Column examples).ProcessedData
            let cnt = Convert.ToDouble(List.length data)
            (countAttributeValue instance attr examples) / cnt

    /// <summary>
    /// Calculate the probabilities of each sample in the set
    /// </summary>
    let probabilities (attr:Attribute) (examples:DataTable) = 
        let hist = histogram attr examples
        let data = (columnAt attr.Column examples).ProcessedData
        let countAttrValue (instance:Datum) (attr:Attribute) (examples:DataTable) =
            match attr.AttributeType with
                | Continuous 
                | NumericOrdinal -> 
                    // continuous and numeric ordinal are processed in the same way.
                    // 
                    // bin the values in the data set X into [(X.1, X.2) .. (X.n-1, X.n)]
                    // then find the pair that bounds the instance value
                    // then approximate the probability by counting the number of samples that fall inside the same
                    // range and divide by the total number of samples
                    // (a, b) = [(i,j) | i <= x <= j, (i,j) <- histogram]
                    // K = [k | a <= k <= b, k <- X]
                    // P(x.i) ~= |K| / |X|
                    // find the pair that contains the instance
                    let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                    (List.fold (fun n (datum:Datum) -> if datum.Missing then n
                                                       else if datum.FloatVal >= lt && datum.FloatVal <= gt then n + 1.0
                                                       else n) 0.0 data)
                | _ -> 0.0
        let len = Convert.ToDouble(List.length data)
        data |>
        List.map (fun item -> (countAttrValue item attr examples) / len)
        
    /// <summary>
    /// Calculate the mean of the set of data.
    /// </summary>
    let mean (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let (total, cnt) = 
            match attr.AttributeType with
            | Continuous -> List.fold (fun (t, c) data -> (t + data.FloatVal, c + 1.0)) (0.0, 0.0) col.ProcessedData
            | NumericOrdinal -> List.fold (fun (t, c) data -> (t + Convert.ToDouble(data.IntVal), c + 1.0)) (0.0, 0.0) col.ProcessedData
            | _ -> (1.0, 1.0)
        total / cnt

    /// <summary>
    /// Calculate the variance of the data set.
    /// Var(X) = sum of P(x) * (x - u)
    /// where u = mean(X)
    /// P(x) is approximated using a histogram.
    /// </summary>
    let probablevariance (attr:Attribute) (data:DataTable) = 
        let u = mean attr data
        let col = columnAt attr.Column data
        let hist = histogram attr data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let len = Convert.ToDouble(List.length vals)
        probabilities attr data
        |> List.zip vals
        |> List.map (fun (v, p) -> p * ((v - u)**2.0) )
        |> List.fold (fun n v -> n + v) 0.0

    /// <summary>
    /// Compute the standard deviation.
    /// std = square root of variance
    /// </summary>
    let probablestddev (attr:Attribute) (data:DataTable) =
        probablevariance attr data |> sqrt

    /// <summary>
    /// Calculate the variance of the data set.
    /// Var(X) = ( sum of (x - u)^2 ) / (N - 1)
    /// where u = mean(X)
    /// </summary>
    let variance (attr:Attribute) (data:DataTable) = 
        let u = mean attr data
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let len = Convert.ToDouble(List.length vals)
        let n =
            vals
            |> List.map (fun v -> ((v - u)**2.0) )
            |> List.fold (fun n v -> n + v) 0.0
        n / (len - 1.0)

    /// <summary>
    /// Get the minimum value of the data set.
    /// </summary>
    let dataMin (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        List.min vals

    /// <summary>
    /// Get the maximum value of the data set.
    /// </summary>
    let dataMax (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        List.max vals


    /// <summary>
    /// Estimate the covariance for the data column 
    /// Cov(X,Y) = (sum of (x - xu)(y - yu)) / N
    /// where xu is mean X
    /// yu is mean Y
    /// N is length X
    /// </summary>
    let covariance (attrA:Attribute) (dataA:DataTable) (attrB:Attribute) (dataB:DataTable) = 
        let uA = mean attrA dataA
        let colA = columnAt attrA.Column dataA
        let uB = mean attrB dataB
        let colB = columnAt attrB.Column dataB
        let vals attr col = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let valsA = vals attrA colA
        let valsB = vals attrB colB
        let XY = List.zip valsA valsB
        let cnt = List.length valsA
        let total = List.fold (fun n (x,y) -> n + (x - uA)*(y - uB)) 0.0 XY
        total / Convert.ToDouble(cnt)

    /// <summary>
    /// Compute the standard deviation of the data set
    /// by the 
    /// sqrt of variance
    /// </summary>
    let stddev (attr:Attribute) (data:DataTable) =
        variance attr data |> sqrt


    /// <summary>
    /// Get the column float values for the attribute.
    /// Will not supply values for String or Bool types.
    /// </summary>
    let columnFloatValues (attr:Attribute) (data:DataTable) =
        match attr.AttributeType with
        | String -> stringAttributeToNumericKey attr data |> snd
        | Bool -> List.empty
        | Continuous -> (columnAt attr.Column data).ProcessedData |> List.map (fun datum -> datum.FloatVal)
        | NumericOrdinal -> (columnAt attr.Column data).ProcessedData |> List.map (fun datum -> Convert.ToDouble(datum.IntVal))

    /// <summary>
    /// Calculate the euclidean norm which is the sqrt of sum of squares of the vector
    /// </summary>
    let euclidnorm (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let total = List.fold (fun n x -> n + x*x) 0.0 vals
        sqrt total

    /// <summary>
    /// Use euclidean norm to normalise the vector matching the attribute.
    /// Applies only to continuous or numeric types.
    /// </summary>
    let euclidnormalisation (attr:Attribute) (data:DataTable) =
        let col = columnAt attr.Column data
        let vals = 
            match attr.AttributeType with
            | Continuous -> List.map (fun data -> data.FloatVal) col.ProcessedData
            | NumericOrdinal -> List.map (fun data -> Convert.ToDouble(data.IntVal)) col.ProcessedData
            | _ -> List.empty
        let (total, lst) = List.fold (fun (n, ss) x -> ((n + x*x), x :: ss)) (0.0, []) vals
        let enorm = sqrt total
        List.map (fun n -> n / total) lst

    /// <summary>
    /// Compute the guassian distribution of the vector matching the attribute.
    /// The guassian distribution is calculated as
    /// 1/(sqrt 2pi std) exp (- (x - u)^2/var(X) )
    /// where u is mean X
    /// Since the input data is normalised then 1/(sqrt 2pi) should equal 1
    /// </summary>
    let gaussiandist (attr:Attribute) (data:DataTable) =
        
        let vals = euclidnormalisation attr data
        let len = Convert.ToDouble(List.length vals)
        let total = List.fold (fun t x -> t + x) 0.0 vals
        let u = total / len
        let n =
            vals
            |> List.map (fun v -> ((v - u)**2.0) )
            |> List.fold (fun n v -> n + v) 0.0
        let varx = n / (len - 1.0)
        let std = sqrt varx
        //let prob = 1.0 / (sqrt (2.0* Math.PI * varx) )
        List.sort vals |> List.map ( fun x -> (exp (-1.0*((x - u)**2.0) / varx ) ) )
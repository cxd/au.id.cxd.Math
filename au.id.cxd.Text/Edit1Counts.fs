namespace au.id.cxd.Text

open System
open System.Collections.Generic
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open System.Runtime.Serialization.Formatters
open System.Runtime.Serialization
open PorterStemmerAlgorithm
open au.id.cxd.Text.WordTokeniser


/// <summary>
/// This module reads the text file for edit1 distances
/// and binary serialises the parsed data in a simple dictionary
/// in order to create a simple lookup table.
/// </summary>
module Edit1Counts =
    
    let edit1Splitter (line:string) = line.Split([|' '; '\t'|])
    
    let edit1Filter (word:string) = word.ToLower().Trim()

    /// <summary>
    /// Read the edit file
    /// the format is each edit is separated on a line
    /// a|b cnt
    /// a|b are mistyped cnt times
    /// </summary>
    let parseTextEditData (editFile:string) =
        let sentences = readLines edit1Filter lineFilter edit1Splitter editFile
        let data = new Dictionary<string, float>()
        Seq.iter(fun (pair:TokenList) ->
                     if (pair.Length = 2) then
                         System.Console.WriteLine("{0} = {1}", pair.[0], pair.[1])
                         let key = pair.[0]
                         let cnt = Double.Parse(pair.[1])
                         if not (data.ContainsKey(key)) then
                            data.Add(key, cnt)
                     else ()) sentences
        data

    /// <summary>
    /// Store the parsed text edit data as a binary serialized resource.
    /// </summary>
    let storeEditData (fileName:string) (data:Dictionary<string, float>) =
        let fmt = new BinaryFormatter()
        let stream = new FileStream(fileName, FileMode.Create)        
        fmt.Serialize(stream, data)
        stream.Flush()
        stream.Close()
        fileName
    
    /// <summary>
    /// Read the parsed text edit data from a binary serialized resource.
    /// </summary>
    let readEditData (fileName:string) =
        let fmt = new BinaryFormatter()
        let stream = new FileStream(fileName, FileMode.Open)
        let data = fmt.Deserialize(stream) :?> Dictionary<string, float>
        stream.Close()
        data

    /// <summary>
    /// Load the edit 1 counts from the default path
    /// This is expected to be the app domain base directory
    /// </summary>
    let load () =
        // load and cache the edit counts
        let mutable data = null
        if (data <> null) then 
            data
        else 
            let file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "count1edit.bin")
            if (File.Exists(file)) then
                data <- readEditData file
                data
            else 
                raise (new Exception(String.Format("The count1edit.bin file could not be found at {0}", file)))


    /// <summary>
    /// Return the count of the number of times substring A has been mistyped as substring B
    /// </summary>
    let editCount (textA:string) (textB:string) (data:Dictionary<string, float>) =
        let key = String.Format("{0}|{1}", textA, textB).ToLower()
        if (data.ContainsKey(key)) then data.[key]
        else 0.0

    /// <summary>
    /// for x in edits compute P(x|w)
    /// This takes into account whether the edit was
    /// an insertion
    /// a deletion
    /// a substitution
    /// or a transposition
    /// The supplied input vocab list should be sorted for binary searching.
    /// </summary>
    let computeProbabilitiesWith (word:string) (data:Dictionary<string, float>) (vocab:string list)  =
        if String.IsNullOrEmpty(word) then List.empty
        else
        let alpha = ['a'..'z']
        let chars = word.ToCharArray() |> Array.toList
        let len = List.length chars
        // create pairs for each word
        let pairs = 
            List.fold (fun collect cur -> 
                           let prev = List.head collect
                           (snd prev, cur) :: collect) [('0', List.head chars)] (List.tail chars) 
            |> List.rev
        
        let isknown set = 
                List.filter 
                    (fun word -> 
                        let test = fst word
                        if (String.IsNullOrEmpty(test) = true) then false
                        else
                            let result =
                                List.exists 
                                    (fun (known:string) -> 
                                        if ((known.ToLower()).Equals(test.ToLower()) = true) then
                                            true
                                        else false
                                        ) vocab
                            result) set

        let splitparts i =
            if (i = 0) then
                ("", String.Format("{0}", word.Substring(1, len-1)))
            else if (i = len-1) then
                (word.Substring(0, len-1), "")
            else 
                (word.Substring(0,i), word.Substring(i+1))

        // the subset of words in the vocabulary
        // that could be obtained by deleting a character
        let deletions =
            List.mapi 
                    (fun i a -> 
                        if (len = 1) then ("", ("",""))
                        else if (i = 0) then
                            let del = word.Substring(0,2)
                            let replace = word.Substring(1,1)
                            (String.Format("{0}", word.Substring(1, len-1)), (del, replace))
                        else if (i = len-1) then
                            let del = word.Substring(i-1,2)
                            let replace = new string(chars.[i],1)
                            (String.Format("{0}", word.Substring(0, len-1)), (del, replace))
                        else 
                            let del = word.Substring(i,2)
                            (String.Format("{0}{1}", word.Substring(0,i), word.Substring(i+1)), (del, ""))
                            ) chars 
            |> isknown

        /// obtain the list of edits that can be obtained by swapping two characters
        let transpositions = 
            List.mapi
                (fun i a -> 
                    if (i = len - 1) then
                        ("", ("",""))
                    else 
                        let swap i j (arr:char array) = 
                            let tmp = arr.[i]
                            arr.[i] <- arr.[j]
                            arr.[j] <- tmp
                            (new string(arr), (new string([|chars.[i]; chars.[j]|]), new string([|chars.[j]; chars.[i]|])) )
                        swap i (i+1) (word.ToCharArray()) ) chars 
            |> isknown

        /// the list of edits that could be obtained by substituting a character
        let substitutions =
            List.mapi
                (fun i a ->
                    let (partA, partB) = splitparts i
                    List.map (fun c -> (String.Format("{0}{1}{2}", partA, c, partB), ( new string([|word.[i]|]), new string([|c|]) ) ) ) alpha
                    ) chars
            |> (fun lst -> seq { for i in lst do
                                 yield! i } |> Seq.toList) 
            |> isknown

        /// the list of edits obtained by inserting a character
        let insertions =
            List.mapi
                (fun i a ->
                    let (partA, partB) = splitparts i
                    List.map (fun c -> (String.Format("{0}{1}{2}{3}", partA, word.[i], c, partB), ( new string([|word.[i]|]), new string([|word.[i]; c|]) ) ) ) alpha
                    ) chars
            |> (fun lst -> seq { for i in lst do
                                 yield! i } |> Seq.toList) 
            |> isknown

        
        let edits = seq { for edit in [ deletions; transpositions; substitutions; insertions ] do
                            yield! edit } |> Seq.toList
    
        // calculate the counts
        let counts = List.map (fun (newword, (before, after)) -> (newword, (before, after), (editCount before after data) + 1.0) ) edits
        let sum = List.fold (fun total (newword, (before, after), cnt) -> total + cnt) 0.0 counts
        let probabilities = 
            List.map (fun (newword, (before, after), cnt) -> (newword, cnt/sum) ) counts
            |> List.sortBy (fun (newword, prob) -> 1.0 - prob)
        // check if the substring if a word.
        probabilities

    /// <summary>
    /// for x in edits compute P(x|w)
    /// This takes into account whether the edit was
    /// an insertion
    /// a deletion
    /// a substitution
    /// or a transposition
    /// The supplied input vocab list should be sorted for binary searching.
    /// </summary>
    let computeProbabilities (word:string) (vocab:string list)  =
        let data = load()
        computeProbabilitiesWith word data vocab

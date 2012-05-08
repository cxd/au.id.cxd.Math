namespace au.id.cxd.Text

open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open System.Runtime.Serialization.Formatters
open System.Runtime.Serialization
open System.Text.RegularExpressions
open PorterStemmerAlgorithm

/// <summary>
/// A module that can be used to read data and produce a sequence of strings for each line
/// </summary> 
module WordTokeniser =
    /// a list of tokens
    type TokenList = string array

    /// Sentences is a collection of token lists
    type Sentences = TokenList seq

    /// a collection of tokens with a class label assigned
    type ClassifiedTokenList = string * TokenList

    /// a collection of labeled tokens used for classification tasks
    type ClassifiedSentences = ClassifiedTokenList seq

    /// <summary>
    /// a default function that is used to post process each word as it is read from file
    /// </summary>
    let emptyFilter (word:string) = word.ToLower().Trim()

    let stemmer = new PorterStemmer()

    /// <summary>
    /// apply a stemming filter
    /// </summary>
    let stemmerFilter (word:string) = 
        stemmer.stemTerm(word.ToLower())

    let lineFilter (line : string array) =
        Array.filter (fun (item:string) -> not (String.IsNullOrEmpty(item)) ) line

   
    /// <summary>
    /// Convert a line into an array of tokens
    /// </summary>
    let defaultSplitter (line:string) = 
        // remove numbers and non word characters
        let regex = new Regex("\d", RegexOptions.Multiline )
        let line' = regex.Replace(line, "")
        line'.Split([|' '; '-'; '.'; '('; ')'; '\"'; ','; '?'; ';';'\t'; ':'|]) 

    /// <summary>
    /// Read all words from the supplied file into a sequence of lists
    /// use a splitter to convert the line to words (fun splitter (line:string) -> Array of string)
    /// use a filter to post process each of the words (change to lower case or stem and so on)
    /// (fun filter (word:string) -> string)
    /// </summary>
    let readLines wordfilter linefilter splitter (fileName:string) = 
            seq { 
                    use reader = new StreamReader(fileName)
                    while (not reader.EndOfStream) do  
                        yield (reader.ReadLine()
                                // perform tokenisation
                                |> splitter
                                // remove all empty terms
                                |> linefilter
                                // modify the words
                                |> Array.map wordfilter
                                |> Array.filter (fun (word:string) -> not (String.IsNullOrEmpty(word)) )
                                )
                    reader.Close()
                }
        
     /// <summary>
    /// supply a line delimited file name
    /// the stop word filter will read in a list of stop words
    /// and will return a function that can be used to filter stopwords
    let stopwordFilter (file:string) =
        let stopWords = readLines emptyFilter lineFilter defaultSplitter file 
                        |> (fun line -> seq { for word in line do yield! word })
                        |> Seq.toList 
        (fun (word:string) ->
             if List.exists (fun (item:string) -> item.Equals(word, StringComparison.OrdinalIgnoreCase)) stopWords then
                ""
             else word)
        

    /// <summary>
    /// Read all words using a simple splitter and without modifying the words
    /// </summary>
    let defaultReadLine (fileName:string) =
        readLines emptyFilter lineFilter defaultSplitter fileName
    
    /// <summary>
    /// Read all words and stem them during the process
    /// </summary>
    let stemReadLine (fileName:string) =
        readLines stemmerFilter lineFilter defaultSplitter fileName
    
    /// <summary>
    /// read a set of classified documents
    /// from within the base directory
    /// these classified documents are organised beneath
    /// folders where each of the names of the folders
    /// represent the class name.
    /// </summary>
    let readClassifiedDocuments (baseDir:string) wordfilter linefilter splitter =
        let dinfo = new DirectoryInfo(baseDir)
        let directories = dinfo.GetDirectories() |> Array.toSeq
        let classes = Seq.map (fun (directory:DirectoryInfo) -> directory.Name) directories
        // each child directory name is a class label
        // read each file and convert the file into a token list.
        let data =
            directories
            |>
            Seq.map (fun dir ->
                            dir.GetFiles() |>
                            Array.map (fun file ->
                                        let sentences = readLines wordfilter linefilter splitter file.FullName
                                        (dir.Name, seq { for sentence in sentences do yield! sentence } |> Seq.toArray ))
                     )
            // convert it to a sequence.
            |> (fun pairs -> seq { for pair in pairs do yield! pair })
        (classes, data)

    /// <summary>
    /// Store the data as a binary serialized resource.
    /// </summary>
    let storeData (fileName:string) data =
        let fmt = new BinaryFormatter()
        let stream = new FileStream(fileName, FileMode.Create)        
        fmt.Serialize(stream, data)
        stream.Flush()
        stream.Close()
        fileName
    
    /// <summary>
    /// Read the data from a binary serialized resource.
    /// </summary>
    let readData (fileName:string) =
        let fmt = new BinaryFormatter()
        let stream = new FileStream(fileName, FileMode.Open)
        let data = fmt.Deserialize(stream)
        stream.Close()
        data


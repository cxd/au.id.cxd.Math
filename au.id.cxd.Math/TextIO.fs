namespace au.id.cxd.Math

open System
open System.IO 

module TextIO = 

    /// <summary>
    /// Read a file line by line within a sequence
    /// </summary>
    let readFile (fileName:string) = 
            seq { use reader = new StreamReader(fileName)
                  while (not reader.EndOfStream) do  
                    yield reader.ReadLine()
                  reader.Close()
                }

    /// <summary>
    /// Split a line on the delimited tokens.
    /// </summary>
    let delimitedTokens delimiter (line:string) = 
        List.map (fun (token:string) -> token.Replace("'", "").Replace("\"", "") ) (line.Split(delimiter) |> Array.toList)
                                                
    

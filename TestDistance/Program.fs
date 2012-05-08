// Learn more about F# at http://fsharp.net


open System
open au.id.cxd.Text

let pairs = [ ("verification", "verify");
              ("verify", "verify");
              ("obfuscate", "ofbsucate");
              ("Rich Heir Estate Services", "Rich Hier State Services")]

List.iter (fun (wordA, wordB) ->
                let dist1 = LevensteinDistance.computeDistanceMatrix wordA wordB
                let distA = fst dist1
                Console.WriteLine("Levenstein Distance")
                Console.WriteLine("{0} to {1} = {2}", wordA, wordB, distA)
                
                let dist1' = DamerauLevensteinDistance.computeDistanceMatrix wordA wordB
                let distA' = fst dist1'
                Console.WriteLine("Damerau-Levenstein Distance")
                Console.WriteLine("{0} to {1} = {2}", wordA, wordB, distA')) pairs
                
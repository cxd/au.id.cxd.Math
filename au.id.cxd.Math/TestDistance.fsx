#light

#r @"FSharp.PowerPack.dll"
#r @".\bin\Debug\au.id.cxd.Math.dll"

open System
open au.id.cxd.Math.LevensteinDistance

let test =
    let wordA = "verification"
    let wordB = "verify"
    let wordC = "verify"
    let dist1 = computeDistanceMatrix wordA wordB
    let dist2 = computeDistanceMatrix wordB wordC
    let distA = fst dist1
    let distB = fst dist2
    Console.WriteLine("{0} to {1} = {2}", wordA, wordB, distA)
    Console.WriteLine("{0} to {1} = {2}", wordB, wordC, distB)

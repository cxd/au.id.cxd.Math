#light

/// This file is a script that can be executed with the F# Interactive.  
/// It can be used to explore and test the library project.
/// Note that script files will not be part of the project build.

#r @"C:\Program Files\FSharpPowerPack-2.0.0.0\\bin\FSharp.PowerPack.dll"

#load "Matrix.fs"
#load "Neuron.fs"
open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators

open au.id.cxd.Math
/// temperature
let M = Matrix.Ones 10 10

let L = Matrix.LogSig M 1.0 0.5

L

let (Z:Matrix<float>) = (Matrix.Generic.zero 10 11)
Z
let A = Matrix.InsertMatrix M Z
A
Z
let b = A = Z
b

let Bias = ( Vector.map (fun a -> a + 2.0) (Vector.zero 10) )
Bias

let A2 = (Matrix.InsertColumn 10 Bias A)
A2

let T = (Matrix.InsertColumn 10 
            ( Vector.map (fun a -> a + 2.0) (Vector.zero 10) ) 
            (Matrix.InsertMatrix M 
                (Matrix.Generic.zero 10 11)))
T

let U = Matrix.InsertMatrix M
                (Matrix.InsertRow 10 ((Vector.map (fun a -> a + 2.0) (Vector.zero 10)).Transpose) (Matrix.zero 11 10) )
U


let R = Matrix.Rand 10 10
R


let Sum = Matrix.SumColumns M
Sum

let RowSum = Matrix.SumRows M
RowSum

/// test repmat
let TRM = Matrix.RepMat (Matrix.Ones 2 2) 2 2
TRM

let AT = Matrix.Ones 10 1
let BT = Matrix.Ones 10 1
let CT = AT.Transpose * BT
let c = cos CT.[0,0]

let N = Matrix.NormaliseColumns U

//
//let max = Matrix.Max (U.[0..10, 2..2])
//let min = Matrix.Min (U.[0..10, 2..2])
//let fn = (fun a -> (a - min) / (max - min))
//let cols = [2]
//let Norm = Matrix.ApplyToColumn U 2 fn
//let tmp = [for a in cols -> let m = Matrix.ApplyToColumn U a fn
//                            a]

let MF = Matrix.CsvWrite N @"C:\temp\normals.csv"
let TF = Matrix.CsvRead @"C:\temp\normals.csv"

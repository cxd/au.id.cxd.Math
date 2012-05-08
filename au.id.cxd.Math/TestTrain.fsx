#light


/// This file is a script that can be executed with the F# Interactive.  
/// It can be used to explore and test the library project.
/// Note that script files will not be part of the project build.

#r @"C:\Program Files\FSharpPowerPack-2.0.0.0\\bin\FSharp.PowerPack.dll"

#load "Matrix.fs"
#load "Neuron.fs"
#load "MLPTrainer.fs"
open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System

open au.id.cxd.Math

// test a simple cos function generator with 2 input variables and one output variable.
let D = Matrix.Ones 500 3
let fn = (fun i j a -> 
            match j with 
                | 2 ->
                   Math.Sin ((float)(i+j)) 
                |_ -> (float)(i+j))
let X = Matrix.mapi fn D

let Trainer = ((new MLPTrainer()).PrepareData X).PrepareNetwork 1;


// Learn more about F# at http://fsharp.net
open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System;
open System.Text;
open System.IO;

open au.id.cxd.Math
open au.id.cxd.Math.MatrixGaussElimination
// take parameters A
let A = matrix [[1.0; 4.0; 7.0];
                [2.0; 5.0; 8.0];
                [3.0; 6.0; 10.0]] 
Console.WriteLine("{0}", A)
Console.WriteLine("")
// M contains both matrices L and U 
let M = gaussFwdElim A
Console.WriteLine("{0}", M)
Console.WriteLine("")
// Ux = Y
let Y = [1.0;-1.0;0.0;]
// find X through the upperTriangleMatrix U where Ux = y.
let S = upperTriangleSubBack M Y

List.iter (fun s -> Console.WriteLine("{0}", s.ToString())) S

Console.WriteLine("")
// Finding Ly = B
// determine B by multiplying original A by C.
let C = Matrix.init (List.length S) 1 (fun r c -> S.[r])
let B = A*C
Console.WriteLine("{0}", B)
Console.WriteLine("")

let I = M*A
Console.WriteLine("{0}", I)


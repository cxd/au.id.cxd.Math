#light

#r "FSharp.PowerPack.dll"
#r @"bin\Release\au.id.cxd.Math.dll"

open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System;
open System.Text;
open System.IO;

open au.id.cxd.Math
open au.id.cxd.Math.MatrixGaussElimination
//open au.id.cxd.Math.MatrixDeterminant

/// <summary>
    /// let Cij =(−1)i+jMij
    /// where Mij = the matrix formed by deleting the ith row and jth column of matrix A
    /// The determinant |A| = sum aij * Cij
    /// </summary>
let determinant (A:Matrix<float>) =
    let rec C (A:Matrix<float>) i j t =
        if A.NumRows = 1 && A.NumCols = 1 then A.[0,0] + t
        else if (i < 0 || j < 0) then t
        else
            let i' = Convert.ToDouble(i)
            let j' = Convert.ToDouble(j)
            let A' = Matrix.SubMat A 0 (i-1) 0 (j-1)
            -1.0**(i'+j')*(t + (C A' (i-1) (j-1) t))
    let det (A:Matrix<float>) =
        if A.NumCols = 1 && A.NumRows = 1 then A.[0,0]
        else C A (A.NumRows-1) (A.NumCols-1) 0.0
    if A.NumRows <> A.NumCols then raise(new Exception("Matrix rows and column are not equal"))
    else det A


let A = matrix [[1.0; 4.0; 7.0];
                [2.0; 5.0; 8.0];
                [3.0; 6.0; 10.0]] 

let M = gaussFwdElim A

M

let D = determinant M
D





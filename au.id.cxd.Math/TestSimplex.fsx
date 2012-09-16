
#r "FSharp.PowerPack.dll"
#r "FSharp.PowerPack.Compatibility.dll"


#load "Matrix.fs"
#load "Simplex.fs"

open au.id.cxd.Math.Simplex

(*
max z = 0.6x1 + 0.8x2
s.t.
0.2x1 + 0.4x2 <= 60
0.2x1 + 0.2x2 <= 40
x1, x2 >= 0
*)
let xvars1 = ["x1";"x2";]
let c1 = vector [0.6; 0.8]
let a1 = matrix [ [0.2; 0.4] ; 
                  [0.2; 0.2] ]
let b1 = vector [ 60.0; 40.0 ]

let (flag1, A1) = maximise xvars1 c1 a1 b1

printf "\n\n==========================================\n\n"

(*

max z = 3x + 5y
s.t
3x+y <= 6
x + 2y <= 8

*)

let xvars2 = ["x"; "y"]
let c2 = vector [3.0; 5.0]
let a2 = matrix [ [3.0;1.0];
                  [1.0;2.0] ]
let b2 = vector [6.0; 8.0]
let (flag2, A2) = maximise xvars2 c2 a2 b2

printf "\n\n==========================================\n\n"

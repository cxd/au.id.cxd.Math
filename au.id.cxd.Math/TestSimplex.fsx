
#r "FSharp.PowerPack/osx/FSharp.PowerPack.dll"
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
let pass1 = flag1 = Optimal

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
let pass2 = flag2 = Optimal

printf "\n\n==========================================\n\n"

(* Test Infeasibility

max z = 2x1 + x2
st
x1 + x2 >= 6
x1 <= 4
x2 <= 1
x1,x2 >= 0

*)
let xvars3 = ["x1";"x2"]
let c3 = vector [1.0; 1.0]
let a3 = matrix [ [ -1.0; -1.0 ];
                  [1.0; 0.0];
                  [0.0; 1.0] ]
let b3 = vector [-6.0; 4.0; 1.0 ]
let (flag3, A3) = maximise xvars3 c3 a3 b3
// we expect flag3 to equal Infeasible
let pass3 = flag3 = Infeasible

printf "\n\n==========================================\n\n"

(*
Test unboundedness

max z = 2x1 + x2
st
x1 <= 3
-x1 -x2 <= -2 

*)
let xvars4 = ["x1";"x2"]
let c4 = vector [2.0; 1.0]
let a4 = matrix [ [ 1.0; 0.0 ];
                  [-1.0; -1.0] ]
let b4 = vector [ 3.0; -2.0 ]
let (flag4, A4) = maximise xvars4 c4 a4 b4
// we expect flag3 to equal Infeasible
let pass4 = flag4 = Unbounded

printf "\n\n==========================================\n\n"

(*
Multiple optimal solutions
max z = 3x1 + 6x2
st
x1 + 2x2 <= 4
x1 <= 3
x2 <= 1
x1,x2 >= 0

*)

let xvars5 = ["x1";"x2"]
let c5 = vector [3.0; 6.0]
let a5 = matrix [ [ 1.0; 2.0 ];
                  [1.0; 0.0];
                  [0.0; 1.0] ]
let b5 = vector [ 4.0; 3.0; 1.0 ]
let (flag5, A5) = maximise xvars5 c5 a5 b5
// we expect flag5 to equal Multiple
let pass5 = flag5 = Multiple

printf "\n\n==========================================\n\n"

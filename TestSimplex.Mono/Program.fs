module Program

open au.id.cxd.Math.Simplex
open au.id.cxd.Math.IPBranchBound

let reportBasis (varname, idx, sol) =
    printf "%s = %A\n" varname sol
    
let reportNonBasis (varname, idx, sol, status) =
    printf "%s = %A, %A\n" varname sol status

let report xvars flag (A:Matrix<float>) =
    printf "Solution: \n%A\n%A\n" flag A
    List.iter reportBasis (extractBasis xvars A)
    List.iter reportNonBasis (extractNonBasis xvars A)
    let (rows, cols) = A.Dimensions
    printf "Max z = %f" (A.[rows-1,cols-1])

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

report xvars1 flag1 A1

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


report xvars2 flag2 A2

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

report xvars3 flag3 A3

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

report xvars4 flag4 A4

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

report xvars5 flag5 A5

let I5 = inverse xvars5 A5
printf "\ninverse\n%A\n" I5

printf "\n\n==========================================\n\n"

// example from winston
(*
max z = 500x1 + 6x2 + 10x3 + 8x4
st
400x1 + 3x2 + 2x3 + 2x4 <= 50.0
200x1 + 2x2 + 2x3 + 4x4 <= 20
150x1 + 4x3 + x4 <= 30
500x1 + 4x3 + 5x4 <= 80
x1,x2,x3,x4 >= 0

we need to convert this problem to the dual problem
as it is an unbounded problem (ie the z value could maximise arbitrarily)


let xvars6 = ["x1"; "x2"; "x3"; "x4"]
let c6 = vector [500.0; 6.0; 10.0; 8.0]
let a6 =  matrix [ [400.0; 3.0; 2.0; 2.0 ];
                   [200.0; 2.0; 2.0; 4.0 ];
                   [150.0; 0.0; 4.0; 1.0 ];
                   [500.0; 0.0; 4.0; 5.0]]
let b6 = vector [50.0; 20.0; 30.0; 80.0 ]
let (flag6, A6) = maximise xvars6 c6 a6 b6
// we expect flag5 to equal Multiple
let pass6 = flag6 = Optimal

report xvars6 flag6 A6

let (yvars7, c7, a7, b7) = makeDualAsMax xvars6 c6 a6 b6
let lp = buildLP yvars7 c7 a7 b7
printf "\ndual\n%A\n" lp
let (flag7, A7) = maximise yvars7 c7 a7 b7
report yvars7 flag7 A7
// another example this one is taken from the IP problem.
// however in this test it is for the relaxed IP only.

*)

(*
max z = 3x1 + 2x2
st
x1 + x2 <= 6.0
x1,x2 >= 0 and integer
*)
let ipvars8 = ["x1"; "x2"]
let xvars8 = ["x1"; "x2"]
let C8 = vector [3.0; 2.0]
let A8 = matrix [[1.0; 1.0]]
let b8 = vector [6.0]
let sol8 = maximiseIP ipvars8 xvars8 C8 A8 b8

printf "sol8 %A\n" sol8
printf "\n\n==========================================\n\n"



(*
max z = 28x1 + 11x2
st
1.4x1 + 0.6x2 <= 2.5
x1,x2 >= 0, x1 integer
*)
let ipvars9 = ["x1"]
let xvars9 = ["x1";"x2"]
let C9 = vector [28.0; 11.0]
let A9 = matrix [[1.4; 0.6]]
let b9 = vector [2.5]
let sol9 = maximiseIP ipvars9 xvars9 C9 A9 b9
printf "sol9 %A\n" sol9
printf "\n\n==========================================\n\n"




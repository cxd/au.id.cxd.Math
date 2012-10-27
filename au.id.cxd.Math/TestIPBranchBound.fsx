
#r "bin/mono/Debug/FSharp.PowerPack.dll"
#r "bin/mono/Debug/FSharp.PowerPack.Compatibility.dll"


#load "Matrix.fs"
#load "Simplex.fs"
#load "IPBranchBound.fs"


open au.id.cxd.Math.Simplex
open au.id.cxd.Math.IPBranchBound

(*
max z = 3x1 + 2x2
st
x1 + x2 <= 6.0
x1,x2 >= 0 and integer
*)
let ipvars = ["x1"; "x2"]
let xvars = ["x1"; "x2"]
let C = vector [3.0; 2.0]
let A = matrix [[1.0; 1.0]]
let b = vector [6.0]
let sol = maximiseIP ipvars xvars C A b

printf "\n\n==========================================\n\n"

(*

IN the problem below y1, y2, y3 must equal 1 or 0.
Based on winston section 9.2 example 3.
The 0 coefficient is used so that all ij values of A are defined.

Also xi <= Miyi  has been converted to xi - Miyi <= 0

max z = 6x1 + 4x2 + 7x3 - 200y1 - 150y2 - 100y3
st
3x1 + 2x2 + 6x3 + 0y1 + 0y2 + 0y3 <= 150
4x1 + 3x2 + 4x3 + 0y1 + 0y2 + 0y3 <= 160
x1 + 0x2 + 0x3 - 40y1 + 0y2 + 0y3 <= 0
0x1 + x2 + 0x3 + 0y1 - 53y2 + 0y3 <= 0
0x1 + 0x2 + x3 + 0y1 + 0y2 - 25y3 <= 0

x1,x2,x3,y1,y2,y3 >= 0, integer and y1,y2,y3 = 1 or 0

let ipvars2 = ["x1"; "x2"; "x3"; "y1"; "y2"; "y3"]
let xvars2 = ["x1"; "x2"; "x3"; "y1"; "y2"; "y3"]
let C2 = vector [6.0; 4.0; 7.0; -200.0; -150.0; -100.0]
let A2 = matrix [[3.0; 2.0; 6.0; 0.0; 0.0; 0.0];
                 [4.0; 3.0; 4.0; 0.0; 0.0; 0.0];
                 [1.0; 0.0; 0.0; -40.0; 0.0; 0.0];
                 [0.0; 1.0; 0.0; 0.0; -53.0; 0.0];
                 [0.0; 0.0; 1.0; 0.0; 0.0; -25.0]]
let b2 = vector [150.0; 160.0; 0.0; 0.0; 0.0]
let sol2 = maximiseIP ipvars2 xvars2 C2 A2 b2
printf "sol2 %A\n" sol2
*)


(*
max z = 28x1 + 11x2
st
1.4x1 + 0.6x2 <= 2.5
x1,x2 >= 0, x1 integer
*)
let ipvars3 = ["x1"]
let xvars3 = ["x1";"x2"]
let C3 = vector [28.0; 11.0]
let A3 = matrix [[1.4; 0.6]]
let b3 = vector [2.5]
let sol3 = maximiseIP ipvars3 xvars3 C3 A3 b3
printf "sol3 %A\n" sol3

namespace au.id.cxd.Math

open System

/// <summary>
/// The simplex problem consists of both a 
/// Primal Problem a maximisation problem.
/// and a Dual Problem a minimization problem.
///
/// The primal problem will contain 
/// X vector the variables including the decision variables and the slack variables.
/// C vector - coefficients
/// A matrix - the constraint matrix
/// b vector - the solution vector.
/// Z vector - the objective function row.
///
///
/// So the problem can be defined as:
/// max z = transpose C * x
/// s.t. Ax <= b
///      x >= 0
///
/// We can also identify the basic and non-basic variables.
/// As well as shadown price for each variables.
///
/// The dual problem is represented by:
/// y vector the variables of the minimisation problem including decision variables and slack variables.
/// G vector - the objective function row.
///
/// It is the minimization of G
///
/// min G = transpose b * y
/// s.t. transpose A * y >= C
///      y >= 0
///
/// The method of finding a solution is through convergence.
/// The simplex algorithm will search for the optimal feasible solution.
/// In order to reach the feasible solution.
/// </summary>
module Simplex =

    /// <summary>
    /// Maximize the objective function Cx
    /// With x decision variables
    /// C objective function coefficients
    /// A constraint matrix of coefficients
    /// b solution vector.
    ///
    /// </summary>
    let maximise xvars (C:Vector<float>) (a:Matrix<float>) (b:Vector<float>) = 
        // concat A and S
        let rows = fst a.Dimensions
                
        // identity matrix of slack variables
        let S = Matrix.identity rows
        
        let acols = snd a.Dimensions

        let cols = (snd a.Dimensions) + (snd S.Dimensions)
        
        // the constraint matrix including the slack variables.
        let A' = Matrix.init rows cols (fun i j ->
                                            if (j < acols) then
                                                a.[i,j]
                                            else
                                                S.[i,j-acols])

        // add the b solution vector 
        // to create the augmented matrix
        let A'' = Matrix.init rows (cols+1) (fun i j ->
                                                if (j < snd A'.Dimensions) then
                                                    A'.[i, j]
                                                else 
                                                    b.[i])
        
        // calculate the indicator row and add it to the matrix
        let ind = Matrix.init 1 (snd A''.Dimensions) (fun i j -> 
                                                        if (j < C.Length) then
                                                            -1.0*C.[j]
                                                        else 0.0)

        // now the initial tableau is constructed.
        // A  |b
        // z-c|z
        let A = Matrix.init (rows+1) (cols+1) (fun i j ->
                                                    if (i < fst A''.Dimensions) then
                                                        A''.[i,j]
                                                    else
                                                        ind.[0,j])
        // check that no values in the solution column are negative.
        let isFeasible (A:Matrix<float>) =
            let (rows, cols) = A.Dimensions
            let b = (A.Column (cols - 1)).[0..rows-1]
            not (Vector.exists (fun value -> value < 0.0) b)
        
        // determine if all values in the indicator row are >= 0
        let isOptimal (A:Matrix<float>) =
            let (rows, cols) = A.Dimensions
            let zc = A.Row (rows - 1)
            let zc' = zc.[0..cols - 1] |> RowVector.transpose
            not (Vector.exists (fun value -> value < 0.0) zc')

        // find the index of the max absolute value
        let maxAbsIndex (v:Vector<float>) =
            let negative = Vector.toArray v |> Array.mapi (fun i value -> (i, value))
            let neg = Array.maxBy (fun (i,value) -> 
                                        if (value < 0.0) then Math.Abs value
                                        else Double.MinValue) negative
            fst neg

        /// find the index of the departing variable.
        let findDepartingVariable (c:Vector<float>) (b:Vector<float>) filterFn =
            let ratios = Vector.toArray b 
                        |> Array.mapi (fun i b' -> (i, Math.Abs(filterFn b' / c.[i])) )
            
            printf "Ratios %A\n" ratios

            let min = Array.minBy ( fun (i, value) ->
                                        value) ratios
            fst min
        
        // pivot the matrix for the pivot defined by i,j
        let pivot i j (A:Matrix<float>) =
            let (rows, cols) = A.Dimensions
            // firstly for row i col j divide the entire row by A[i,j]
            let p = A.[i,j]

            printf "Pivot %O\n" p |> ignore

            // first transform the pivot row
            // all other calculations are in relation to this row
            let A' = Matrix.mapi ( fun m n v ->
                                        if (m = i) then
                                            v / p
                                        else v) A
                    
            printf "A' \n %A\n\n" A'
            
            // now pivot the other rows work out a multiplier for each of the rows and work out a value to add or subtract.
            let quantities = A'.[0..(rows-1), j..j]

            printf "quantities \n %A\n\n" quantities
            

            Matrix.mapi( fun m n v ->
                             let quant = quantities.[m,0]
                             let b = A'.[i,n]
                             if (m = i) then
                                v
                             else 
                                //printf "q = %O b = %O\n" quant b
                                v - (quant * b)
                             ) A'

        /// solve the LP problem
        let rec solve (A:Matrix<float>) =
            printf "Solve\n" |> ignore
            printf "%A\n\n" A |> ignore

            let (rows, cols) = A.Dimensions
            if (isFeasible A && not (isOptimal A)) then
                // find the entering variable in the indicator row
                let j = maxAbsIndex (A.Row (rows-1)).Transpose
                // find the exiting variable via the b/candidate ratio
                let c' = (A.Column j).[0..(rows-2)]
                let b' = (A.Column (cols-1)).[0..(rows-2)]
                let i = findDepartingVariable c' b' (fun n -> n)

                printf "Depart %O Enter %O\n" i j
                // perform row operations and attempt to solve the resulting matrix.
                solve (pivot i j A)
            else if (isFeasible A && isOptimal A) then 
                // the solution has been found at this stage.
                (1, A)
            else 
                // need to process the tableau to search for convergence
                // choose the exiting variable from the b column
                let i = maxAbsIndex (A.Column (cols - 1))
                // then choose the entering variable from the 
                // smallest negative ratio in the exiting row
                let c' = (A.Row (rows-1)).Transpose
                let b' = (A.Row i).Transpose
                
                // firstly does it have a solution? Can it be solved?
                // if not then return the current solution with error 
                // (0,A)
                if (not (Vector.exists (fun value -> value < 0.0) b') ) then
                    (0, A) // not solvable.
                else 
                    // find the minimum ratio of the negative values.
                    let j = findDepartingVariable c' b' (fun n -> 
                                                            if (n >= 0.0) then
                                                                Double.MaxValue
                                                            else 
                                                                n)
                    
                    printf "Depart2 %O Enter2 %O\n" i j
                    // then perform row operations and attempt to solve
                    solve (pivot i j A)
        // attempt to solve the LP
        solve A

    ()


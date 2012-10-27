namespace au.id.cxd.Math

open System
open au.id.cxd.Math
open au.id.cxd.Math.Simplex

/// <summary>
/// Branch and bound is a search algorithm for integer programming problems.
/// It iteratively solves the problem by using a relaxed IP problem.
/// Then adds additional constraints for the variables that are found to be part of the solution
/// at each iteration in order to constrain those variables as being either less than the closest integer
/// or greater than the next greatest integer.
/// Each problem describes a new branch in the search space.
/// there are two branches in the search space at any given time.
/// </summary>
module IPBranchBound =

    /// a division is either greater than 
    /// or less than a given integer value
    /// if it is None
    /// then this is the starting point of the solution 
    type Operation =
        | LessThan of float
        | GreaterThan of float
        | Solution
        | None

    type IPSolution =
        { 
          Status: SolutionState;
          Solution: Matrix<float>;
          MaxZ: float;
          XVars: string list;
          IPVars: string list;
          SplitVar: string;
          Op: Operation;
          Problem: Vector<float> * Matrix<float> * Vector<float>;
         }
         
    /// the ip tree used to store the solution.
    /// it is either a branch with a solution and a left and right division
    /// or it is a terminal node.
    type IPTree =
        | Branch of IPTree * IPSolution * IPTree
        | Terminal of IPSolution
        
    /// <summary>
    /// Identify the integer constraint variables from the A matrix
    /// choose the ip constrained variable that has a fractional component.
    /// if none of the ip constrained variables contain a fractional component
    /// then the solution is solved for this branch.
    /// </summary>
    let identifyIntegerConstrainedVariable (ipvars:string list) (xvars:string list) (A:Matrix<float>) = 
        // select the ip constrained variables
        let ipBasisVars = 
                extractBasis xvars A
                |> List.filter (fun (varName, col, varVal) ->
                                    List.contains varName ipvars)
        // process the ip constrained values and select those items
        // that have fractional components
        let components =
                ipBasisVars 
                |> List.map (fun (varName, col, varVal) ->
                                 let floor = Math.Floor(varVal)
                                 let fractional = varVal - floor
                                 let ciel = Math.Ceiling(varVal)
                                 ((varName, col, varVal), (floor, fractional, ciel)))
                |> List.filter (fun ((varName, col, varVal), (floor, fractional, ciel)) ->
                                    fractional > 0.0)
        // return the selection.
        components
    
    /// <summary>
    /// insert the constraint and return the new a coefficient matrix and the new solution vector.
    /// </summary>
    let insertConstraint (a:Matrix<float>) (b:Vector<float>) col coefficient constraintVal =
        let b' =
                Vector.init ((Vector.length b) + 1) 
                            (fun i -> if (i < (Vector.length b)) then
                                        b.[i]
                                      else
                                        constraintVal)
        let (rows, cols) = a.Dimensions
        let newConstraint = 
            Vector.init cols (fun i -> if (i = col) then coefficient
                                       else 0.0)
            |> Vector.transpose
        let a' =
                Matrix.AppendRow newConstraint a
        (a', b')
    
    /// <summary> 
    /// search the problem space 
    /// recursively solve and add constraints where required.
    /// <param name="ipvars">
    /// string list of variable names that are constrained as integers
    /// </param>
    /// <param name="xvars">
    /// string list of decision variable names
    /// </param>
    /// <param name="C">
    /// the coefficient matrix for the objective function
    /// </param>
    /// <param name="a">
    /// The coefficient constraint matrix
    /// </param>
    /// <param name="b">
    /// the initial solution vector (RHS of the constraint matrix)
    /// </param>
    /// </summary>
    let maximiseIP ipvars xvars (C:Vector<float>) (a:Matrix<float>) (b:Vector<float>) =
        let ipsolution = 
         { Status = Start;
           Solution = matrix [];
           MaxZ = 0.0;
           XVars = xvars;
           IPVars = ipvars;
           SplitVar = String.Empty;
           Op = None;
           Problem = (C,a,b)
          }
        /// recursive branch and bound search.
        let rec solveIP (sol:IPSolution) =
             let (C,a,b) = sol.Problem
             let (status, A) = maximise sol.XVars C a b
             let (rows,cols) = A.Dimensions
             let z = A.[rows-1,cols-1]
             let splitChoices = 
                     identifyIntegerConstrainedVariable ipvars xvars A
             // if there are no choices this is solved at this level.
             let mustStop =
                 match status with
                 | Infeasible
                 | Unbounded -> true
                 | _ -> false
             if mustStop then
                 Terminal {
                 Status = status;
                 Solution = A;
                 XVars = sol.XVars;
                 IPVars = ipvars;
                 MaxZ = z;
                 SplitVar = sol.SplitVar;
                 Op = None;
                 Problem = (C,a,b)
                 }
             else if (List.length splitChoices = 0) then
                 Terminal {
                 Status = status;
                 Solution = A;
                 XVars = sol.XVars;
                 IPVars = ipvars;
                 MaxZ = z;
                 SplitVar = sol.SplitVar;
                 Op = Solution;
                 Problem = (C,a,b)
                 }
             else
                 // current solution
                 let current = {
                         Status = status;
                         Solution = A;
                         XVars = sol.XVars;
                         IPVars = ipvars;
                         MaxZ = z;
                         SplitVar = sol.SplitVar;
                         Op = Solution;
                         Problem = (C,a,b)
                         }
                 // choose an arbitrary splitting value
                 let ((varName, i, varVal), (floor, fraction, ciel)) = splitChoices.Head
                 // left solution
                 let (lA, lB) =
                     insertConstraint a b i 1.0 floor
                 let solLeft = 
                      { Status = status;
                        Solution = A;
                        MaxZ = z;
                        XVars = sol.XVars;
                        IPVars = ipvars;
                        // pick a variable on which to split.
                        SplitVar = varName;
                        // choose the solution operation
                        Op = LessThan floor;
                        // update the problem
                        Problem = (C,lA,lB)
                      }
                 // right solution
                 let (rA, rB) =
                     insertConstraint a b i -1.0 ciel
                 let solRight = 
                      { Status = status;
                        Solution = A;
                        MaxZ = z;
                        XVars = sol.XVars;
                        IPVars = ipvars;
                        // pick a variable on which to split.
                        SplitVar = varName;
                        // choose the solution operation
                        Op = GreaterThan ciel;
                        // update the problem
                        Problem = (C,rA,rB)
                      }
                 // this is where we need to perform the continuations
                 // TODO: convert to continuation passing.
                 Branch ((solveIP solLeft), current, (solveIP solRight))
               
        // then traverse the solution to find the maxZ value 
        // from the leaf nodes for feasible solutions
        
        ()

    ()

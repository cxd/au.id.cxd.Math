namespace au.id.cxd.Math

open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System
open System.Text;
open System.IO;

module MatrixGaussElimination =
       
       /// <summary>
       /// Return the identity matrix.
       /// </summary>
       let identity n =
            Matrix.Generic.identity n

       /// <summary>
       /// Perform factorisation of a lower triangle system L with has outputs column vector b
       /// The factorisation determines the vector x.
       /// xi = (bi - (sum(lij*xj)/lii
       /// This algorithm is taken from "Matrix Computations 3rd edition" Ch 3 
       /// Ch3 Algorithm 3.1.1 Forward Substitution Row Version p 89.
       /// it outputs a column vector X 
       /// Number of rows in b and L must be equal.
       /// </summary>
       let lowerTriangleSubFwd (L:Matrix<float>) (b:float List) =
            List.fold (fun (i, (b':float List)) n -> 
                        if (i = 0) then (i+1, (b.[0]/L.[0,0]) :: b')
                        else
                            let lcol = L.Row i 
                            // this is the selection if the i-1 sequence
                            // L(i,1:i-1)
                            let lvals = RowVector.toArray lcol  |> Array.toSeq |> Seq.take (i-1) |> Seq.toList
                            // b(1:i-1) - this is equivalent to the values accumulated so far in b' 
                            // whose current length should equal i-1
                            // so List.rev b' |> List.toSeq |> Seq.take (i-1) |> Seq.toList
                            // really equals:
                            let bvals = List.rev b' |> List.toSeq |> Seq.toList
                            let mult = List.map2 (fun a b -> a*b) lvals bvals |> List.sum
                            (i+1, ((b.[i] - mult) / L.[i,i])::b')) (0, []) b
            |> snd 
            |> List.rev
        
       
       /// <summary>
       /// Perform factorisation over an upper triangle system U which has outputs of column vector b.
       /// The factorisation determines the vector x 
       /// xi = (bi - sum(uij*xj))/uii
       /// This algorithm is taken from "Matrix Computations 3rd edition" Ch 3 
       /// Ch3 Algorithm 3.1.2 Back Substitution Row Version p 89.
       /// </summary>
       let upperTriangleSubBack (U:Matrix<float>) (b:float List) =
           let n = (List.length b) - 1
           List.fold (fun (i, b') value ->
                        if (i = n) then (i-1, b.[i]/U.[i,i]::b')
                        else 
                            let ucol = U.Row i
                            // this is the sequence subset i+1:n
                            let uvals = RowVector.toArray ucol |> Array.toSeq |> Seq.skip (i+1) |> Seq.toList
                            // this is b(i + 1:n) in the algorithm.
                            // it is really the values that are accumulated thus far in b'
                            // it wil have the same length as i+1..n
                            // additionally we do not need to reverse the result as we are consing values into the list
                            // in reverse order (backward substitution).
                            let bvals = b'   
                            let mult = List.map2 (fun a b -> a*b) uvals bvals |> List.sum
                            (i-1, ((b.[i] - mult) / U.[i,i])::b')) (n, []) b
           |> snd


       /// <summary>
       /// This method is the forward elimination technique described in 
       /// "Matrix Computations 3rd edition" Ch3
       /// Section 3.2.8 "Solving a Linear System" p99.
       /// </summary>
       let gaussFwdElim (A:Matrix<float>) =
           (*
           for k=1:n-1
            A(k+1:n,k) = A(k+1:n,k)/A(k,k)
            for i=k+1:n
                for j=k+1:n
                    A(i,j) = A(i,j) - A(i,k)A(k,j)
                end
            end
           end
           *)
           let n = (A.Dimensions |> snd) - 1
           let M = Matrix.copy A
           // for k=1:n-1
           for k = 0 to (n-1) do
                //A(k+1:n,k) = A(k+1:n,k)/A(k,k)
                List.iter (fun v -> 
                               M.[v,k] <- (M.[v,k]/M.[k,k]))
                           [k+1..n]
                (* for i=k+1:n
                    for j=k+1:n
                    *)
                for i=k+1 to n do
                    // A(i,k) = A(i,k)/A(k,k)
                    for j=k+1 to n do
                        // A(i,j) = A(i,j) - A(i,k)A(k,j)
                        M.[i,j] <- M.[i,j] - M.[i,k]*M.[k,j]
           // M is now the upper triangular matrix.
           M
       ()



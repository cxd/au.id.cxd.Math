#light

namespace au.id.cxd.Math
    
    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Core.Operators
    open System;
    open System.Text;
    open System.IO;
    
    type Matrix() =
        
        /// <summary>
        /// Initialise a matrix with a two dimensional array.
        /// </summary>
        static member InitWith (vals:float [] []) = 
            let r = Array.length vals
            let c = Array.length vals.[0]
            Matrix.init r c (fun i j -> vals.[i].[j])

        /// <summary>
        /// Create a submatrix from start row to num rows and start col to numcols
        /// </summary>
        static member SubMat (M:Matrix<float>) stRow numRows stCol numCols =
               Matrix.Generic.init numRows numCols (fun i j -> M.[stRow + i, stCol + j]) 

        /// create a matrix containing value 1.0 at all M[i,j] with i >0 < rows and j > 0 < cols
        static member Ones rows cols =
            Matrix.Generic.init rows cols (fun i j -> 1.0)
            
        /// create a matrix containing a random value at all positions.
        static member Rand rows cols = 
            let rnd = new Random()
            Matrix.map (fun a -> rnd.NextDouble()) (Matrix.zero rows cols)

        /// apply the exp function to all values in the matrix
        static member Exp (M : Matrix<float>) =
            Matrix.Generic.map (fun a -> exp a) M
        
        /// multiple the matrix by a scalar.
        static member Mult (M : Matrix<float>) (n:float) = 
            Matrix.Generic.map (fun a -> a * n) M

        /// add a scalar to all values in the matrix
        static member AddScalar (M : Matrix<float>) n =
            Matrix.Generic.map (fun a -> a + n) M

        /// divide all values by the scalar
        static member DivideByScalar (M: Matrix<float>) n =
            Matrix.Generic.map (fun a -> a / n) M

        /// perform piecewise division A ./ B
        static member PieceWiseDivide (A : Matrix<float>) (B : Matrix<float>) =
            Matrix.Generic.mapi (fun i j a -> a / B.[ i, j] ) A

        /// LogSig sigmoid function.
        /// H - input matrix to apply logistic function t0
        /// temperature - temperature of logistic function
        /// amplitude - amplitude of function
        ///logSig -> Matrix<'a> -> float -> float -> float -> Matrix<'a>
        static member LogSig (H : Matrix<float>) (temperature : float) (amplitude : float) =
                let I = Matrix.Ones H.NumRows H.NumCols
                let E = Matrix.Exp ( -1.0 * (temperature * H) )
                Matrix.PieceWiseDivide (I * amplitude) (Matrix.AddScalar E  1.0 )
              

        /// O is the output of logSig
        /// T is the temperature.
        static member DerivativeLogSig (O:Matrix<float>) (temperature : float) = 
            let I = (Matrix.Ones O.NumRows O.NumCols) in
                (temperature * I ) .* ( I - O)
         
        /// RangeLogSig sigmoid function.
        /// H - input matrix to apply logistic function t0
        /// temperature - temperature of logistic function
        /// amplitude - amplitude of function
        /// range = range of function ie from range to amplitude
        /// range = 0 and amplitude = 1 function is logistic
        /// range = -1 and amplitude = 2 function is tansig
        ///logSig -> Matrix<'a> -> float -> float -> float -> Matrix<'a>        
        static member RangeLogSig (H : Matrix<float>) (temperature : float) (amplitude : float) (range : float) =
                Matrix.AddScalar (Matrix.LogSig H temperature amplitude) range
        
        /// O is the output of logSig
        /// T is the temperature.
        static member RangeDerivativeLogSig (O:Matrix<float>) (temperature : float) (amplitude: float) (range : float) = 
            let I = (Matrix.Ones O.NumRows O.NumCols)
            let N = Matrix.DivideByScalar I temperature
            let D = Matrix.DivideByScalar (Matrix.AddScalar O -1.0*range) amplitude
            N * D * (Matrix.AddScalar (-1.0 * D) 1.0)
         
         
        /// Insert all values in supplied matrix A to supplied matrix B
        /// return matrix B
        static member InsertMatrix (A:Matrix<float>) (B:Matrix<float>) =
            let M = Matrix.mapi (fun i j a -> 
                                (Matrix.set B i j a) 
                                B.[ i, j ]) A
            B                           
        
        /// Insert a matrix into matrix B starting at position row x col
        static member InsertMatrixAt (A:Matrix<float>) (B:Matrix<float>) row col =
            let M = Matrix.mapi (fun i j a -> 
                                (Matrix.set B (row + i) (col + j) a) 
                                B.[ row + i, col + j ]) A
            B 
            
        /// Set all values of matrix B at column int
        /// to all values defined in Vector
        /// return matrix B
        static member InsertColumn col (A:Vector<float>) (B:Matrix<float>) =
            let V = Vector.mapi (fun i a -> (Matrix.set B i col a) 
                                            A.[i]) A
            B
            
        /// Set all values of matrix B at row int
        /// to all values defined in RowVector
        /// return matrix B
        static member InsertRow row (A:RowVector<float>) (B:Matrix<float>) =
            let V = Vector.mapi (fun i a -> (Matrix.set B row i a) 
                                            A.[i]) A.Transpose
            B
        
        /// Return a matrix of 1 row x N columns where each nth column 
        /// is the sum of the nth column from the matrix A
        static member SumColumns (A:Matrix<float>) =
            let M = Matrix.zero 1 A.NumCols
            let T = Matrix.mapi (fun i j a -> 
                                            (Matrix.set M 0 j (M.[0,j] + a)) 
                                            a) A
            M
        
        /// Return a matrix of N rows x 1 columns where each nth row
        /// is the sum of the nth row from the matrix A
        static member SumRows (A:Matrix<float>) =
            let M = Matrix.zero A.NumRows 1
            let T = Matrix.mapi (fun i j a -> 
                                            (Matrix.set M i 0 (M.[i,0] + a)) 
                                            a) A
            M
        
        /// <summary>
        /// Tile matrix C into matrix A for n x m times starting at row a and column b
        /// </summary>
        static member private tileB (A : Matrix<float>) (C : Matrix<float>) n m (a : int) (b : int) =
            match b with 
                | _ when b <= ((A.NumCols * m) - A.NumCols) ->
                    Matrix.tileB A (Matrix.InsertMatrixAt A C a b) n m a (b + A.NumCols)
                | _ -> C
            
        
        /// <summary>
        /// Tile matrix C inside matrix A at n x m starting at row a and column b
        /// </summary>
        static member private tile (A : Matrix<float>) (C : Matrix<float>) n m (a : int) (b : int) =
                match a with
                    | _ when a <= ((A.NumRows * n) - A.NumRows) ->
                        Matrix.tile A (Matrix.tileB A C n m a b) n m (a + A.NumRows) b
                    | _ -> C    
        
        /// Create a matrix consisting of n-by-m tiling of copies of A
        static member RepMat (A:Matrix<float>) n m =
            Matrix.tile A (Matrix.zero (A.NumRows * n) (A.NumCols * m)) n m 0 0
        
        
        static member Max (A:Matrix<float>) =
            Seq.max (Matrix.to_vector A)
        
        static member Min (A:Matrix<float>) =
            Seq.min (Matrix.to_vector A)
        
        static member ApplyToColumn (A:Matrix<float>) col fn =
            let colVector = Vector.map fn (A.Column col)
            Matrix.InsertColumn col colVector A
        
        static member ApplyToRow (A:Matrix<float>) row fn =
            let rowVector = Vector.map fn (A.Row row).Transpose
            Matrix.InsertRow row rowVector.Transpose A
            
            
        
        /// Normalise along columns using
        /// A ij = A ij - min(A ij) / max (A ij) - min (A ij)
        /// This will modify the matrix in place.
        static member NormaliseColumns (A:Matrix<float>) =
            let max = Matrix.Max A
            let min = Matrix.Min A
            let fn = (fun a -> (a - min) / (max - min))
            let cols = [0..(A.NumCols - 1)]
            let tmp = [for a in cols -> let tmpM = Matrix.ApplyToColumn A a fn
                                        a]
            A
        
      
        /// utility method to read a file
        static member private readFile (csvFile : string) =
            seq { use reader = new StreamReader(csvFile)
                  while (not reader.EndOfStream) do  
                    yield reader.ReadLine()
            }
        
                
        static member private readLine i (A:Matrix<float>) (line : string) =
            let tmp = Array.mapi (fun j k -> let f = System.Double.Parse(k)
                                             Matrix.set A i j f ) (line.Split([|','|]))
            line
                                    
        /// read a matrix from a CSV
        static member CsvRead (csvFile : string) =
            let lines = Seq.toArray (Matrix.readFile csvFile)
            let rows = lines.Length
            let cols = (lines.[0].ToString()).Split([|','|]).Length
            let A = Matrix.zero rows cols
            let tmp = Array.mapi (fun i a -> 
                                    let tmp = Matrix.readLine i A a
                                    a) lines
            A
            
        
        /// write a matrix to a csv
        static member CsvWrite (A:Matrix<float>) (csvFile : string) = 
            let writer = new StreamWriter(csvFile)
            let sb = new StringBuilder();
            let M = Matrix.mapi (fun i j a -> 
                                match j with
                                    | _ when j = (A.NumCols - 1) -> 
                                        let tmp = sb.AppendFormat("{0}", a)
                                        let tmp = sb.AppendLine()
                                        a
                                    | _ -> let tmp = sb.AppendFormat("{0},",a)
                                           a) A
            let tmp = writer.Write(sb.ToString())
            let tmp = writer.Close();
            A


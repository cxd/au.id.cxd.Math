namespace au.id.cxd.ImageProcessor

module Processor =

    open System
    open System.Drawing
    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Compatibility
    open MathProvider.LinearAlgebra
    open au.id.cxd.Math

    module L = MathProvider.LinearAlgebra



    L.startProvider()

    /// <summary>
    /// A record type to store the 4 matrices extracted from a bitmap.
    /// </summary>
    type MatrixImage = { red:matrix; green:matrix; blue:matrix; grey:matrix; alpha:matrix }

    /// <summary>
    /// Convert a bitmap to a MatrixImage representation
    /// that defines matrices for red green blue and grey components
    /// </summary>
    let bitmapToMatrices (map:Bitmap) = 
        let redM = Matrix.create map.Width map.Height 0.0
        let blueM = Matrix.create map.Width map.Height 0.0
        let greenM = Matrix.create map.Width map.Height 0.0
        let greyM = Matrix.create map.Width map.Height 0.0
        let alphaM = Matrix.create map.Width map.Height 0.0
        for i in [0..((map.Width) - 1)] do
            for j in [0..((map.Height) - 1)] do
                let color = map.GetPixel(i, j)
                Matrix.set redM i j (Convert.ToDouble color.R)
                Matrix.set greenM i j (Convert.ToDouble color.G)
                Matrix.set blueM i j (Convert.ToDouble color.B)
                Matrix.set alphaM i j (Convert.ToDouble color.A)
                // the weights are from wikipedia section on converting color to grayscale.
                // http://blog.paranoidferret.com/index.php/2007/08/31/csharp-tutorial-convert-a-color-image-to-greyscale/
                // http://en.wikipedia.org/wiki/Grayscale
                // greyM[i, j] = red[i, j] * 0.3 + green[i, j] * 0.59 + blue[i, j] * 0.11;
                Matrix.set greyM i j ( (Matrix.get redM i j) * 0.3 + (Matrix.get greenM i j) * 0.59 + (Matrix.get blueM i j) * 0.11 )
        { red = redM;  blue = blueM; green = greenM; grey = greyM; alpha = alphaM }


    /// Functional methods for matrix.
    let normalise (m:matrix) = 
        let min = Matrix.create 1 (snd m.Dimensions) 0.0
        let max = Matrix.create 1 (snd m.Dimensions) 0.0
        for i in [0..((snd m.Dimensions) - 1)] do
            let cmin = Double.MaxValue
            let cmax = Double.MinValue
            Matrix.set min 0 i (Vector.toArray (m.Column i) |> Array.min)
            Matrix.set max 0 i (Vector.toArray (m.Column i) |> Array.max)
 
    let numCols (m:matrix) = snd m.Dimensions

    let numRows (m:matrix) = fst m.Dimensions

    /// Create a matrix consisting of n-by-m tiling of copies of A
    let repmat (A:matrix) n m =
            Matrix.RepMat A n m
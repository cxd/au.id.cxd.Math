namespace au.id.cxd.Math

open System
open System.IO
open TextIO
open TrainingData
open DataSummary

module Regression =

    /// <summary>
    /// Straight line regression.
    /// yi = a + Bx
    /// B = (sum(xy) - 1/N (sum x)(sum y)) / (sum(x^2) - 1/N(sum x)^2)
    /// a = 1/N (sum(y) - B(sum(x)))
    /// </summary>
    let straightline (attrX:Attribute) (attrY:Attribute) (data:DataTable) =
        let X = columnFloatValues attrX data
        let Y = columnFloatValues attrY data
        let S = List.zip X Y
        let N = Convert.ToDouble(List.length X)
        let sumX = List.fold (+) 0.0 X
        let sumY = List.fold (+) 0.0 Y
        let sumXY = List.fold (+) 0.0 (List.map (fun (x,y) -> x*y) S)
        let sumXBysumY = sumX * sumY
        let sumXsq = List.fold (+) 0.0 (List.map (fun x -> x**2.0) X)      
        let B = (sumXY - (1.0/N)*sumXBysumY)/(sumXsq - (1.0/N)*sumX**2.0)
        let a = 1.0/N * (sumY - B*sumX)
        let estimate x = a + B*x
        (X, List.map estimate X)

    /// <summary>
    /// Perform loess logistic regression on the data attribute
    /// yi =a+b1(xi −x0)+b2(xi −x0)2 +···+bp(xi −x0)p +ei
    /// W(z) = (1 − |z|3)3 for |z| < 1 0 for |z| ≥ 1
    /// zi = (xi −x0)/h, where h is the half-width of a window
    /// This can only be applied to numeric
    /// or continuous series.
    ///
    /// alpha is the smoothing factor a
    /// windowSize is the neighbourhood size to use for smoothing
    /// </summary>
    let loess alpha windowSize (attrX:Attribute) (attrY:Attribute) (data:DataTable) =
        let vals = columnFloatValues attrX data
        let yvals = columnFloatValues attrY data
        let pairs = List.zip vals yvals
        let h = windowSize/2.0
        let z xN xO = (xN - xO) / h
        let W zvec = 
            let len = vectorLength zvec
            if (len < 1.0) then
                (1.0 - len**3.0)**3.0
            else 0.0
        // partition the data into fields of the window size
        let (n, set, last) =
            List.fold (fun (n, set, last) v ->
                            if n < windowSize then
                             (n+1.0, set, v :: last)
                            else (0.0, (List.rev last)::set, []) )
                       (0.0, [], []) pairs 
        let set' =
            if (List.length last > 0) then
                List.rev (last :: set)
            else List.rev set
        // calculate Z for each window
        let windows =
            List.mapi (fun i (win:(float*float) list) ->
                            let (x, y) = win.[0]
                            let err = 
                             List.fold (fun n (x1, y1) -> n + (y1 - x1)) 0.0 win
                             / Convert.ToDouble(List.length win)
                            (x, (List.map 
                                (fun (x', y') -> 
                                    let z' = z y' y
                                    let w = W [z']
                                    let i' = Convert.ToDouble(i)
                                    w*((x' - x)**i'))
                                win), err) ) set'
        (List.map (fun (x, window, err) ->
                            x) windows,
         List.map (fun (x, window, err) ->
                            List.sum window
                            |> (+) alpha
                            |> (+) err) windows)


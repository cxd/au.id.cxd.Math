namespace au.id.cxd.Math

open Microsoft.FSharp.Math
open Microsoft.FSharp.Core.Operators
open System
open System.Text
open System.IO

/// metrics for classifier 
/// based on the confusion matrix
module ClassifierMetrics =

    /// metric summary for the classifier.
    type Metrics =
        { Accuracy:float;
          Kappa:float;
          FScore:float; 
          }


    /// calculate the metrics derived from the confusion matrix
    /// the confusion matrix must be of dimension len x len
    let calculateMetrics (confusionMatrix:Matrix<float>) (len:int) = 
        // calculate accuracy
        // accuracy = sum D(k,k) / |D|
        // find the sum of the matrix 
        let total = Matrix.fold (fun cnt datum -> cnt + datum) 0.0 confusionMatrix
        let accuracy = Matrix.foldi (fun i j sum datum -> 
                                         if i = j && total > 0.0 then
                                            sum + (datum/total)
                                         else sum) 0.0 confusionMatrix
        // compute the probability matrix
        let probMatrix = Matrix.map (fun datum -> 
                                           if total > 0.0 then
                                            datum / total 
                                           else 0.0) confusionMatrix
        // P^(i,j) = P(i)*P(j)
        // P(i) = sum of columns
        let prior_i_row = RowVector.create len 0.0
        let prior_i = Matrix.foldByCol (fun n datum -> n + datum) prior_i_row probMatrix
        // P(j) = sum of rows
        let prior_j_col = Vector.create len 0.0
        let prior_j = Matrix.foldByRow (fun n datum -> n + datum) prior_j_col probMatrix
        // P^(i,j)
        let p_hat i j = prior_i.[i] * prior_j.[j]
        // calculate the kappa statistic
        // K = ( sum of P(k,k) - sum of P^(k,k) ) / ( 1 - sum of P^(k,k) )  
        let ksum = Matrix.foldi (fun i j sum datum -> 
                                      if i = j then
                                        sum + (p_hat i j)
                                      else sum) 0.0 probMatrix
        let colsum = Matrix.foldi (fun i j sum datum ->
                                       if i = j then
                                        sum + datum
                                       else sum) 0.0 probMatrix
        let kappa = (colsum - ksum) / (1.0 - ksum)         
        // calculate f-score
        let p_t i = prior_i.[i]
        let p_s j = prior_j.[j]
        let p i j = Matrix.get probMatrix i j
        // sum of P_t(k) * ( 2*P(k,k) / (P_t(k) + P_s(k)) )
        let score k = 
            let pt = p_t k
            let ps = p_s k
            if (pt + ps) > 0.0 then
                pt * (2.0 *  (p k k)) / (pt + ps)
            else 0.0
        let fscore = Matrix.foldi (fun i j sum datum ->
                                       if i = j then
                                        sum + score i
                                       else sum) 0.0 probMatrix
        // return the total , kappa, fscore and confusion matrix
        { Accuracy = accuracy; Kappa = kappa; FScore = fscore; }

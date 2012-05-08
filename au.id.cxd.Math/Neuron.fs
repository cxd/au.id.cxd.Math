#light

namespace au.id.cxd.Math

    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Core.Operators
    open au.id.cxd.Math

    /// A neuron performs the Activation and BackPropogate operations 
    /// and stores the state resulting from these operations.
    type Neuron() =
        /// Neuron bias applied to weight matrix.
        let mutable (bias : float) = 0.0
        /// temperature applied to activation
        /// see LogSig
        let mutable (temperature : float) = 0.5
        /// Amplitude applied to activation
        /// see LogSig
        let mutable (amplitude : float) = 1.0
        /// Range applied to activation
        /// see LogSig
        let mutable (range : float) = 0.0
        /// Learning rate applied to back propogation
        let mutable (learnRate : float) = 0.01
        /// Momentum of batch propogation - used to prevent being trapped in gullies
        /// a small value between 0 and 1
        let mutable (momentum : float) = 0.1
        /// activation output
        let mutable (output : Matrix<float>) = matrix [[]]
        ///  derivative output
        let mutable (derivative : Matrix<float>) = matrix [[]]
        /// weights learnt from training data
        let mutable (weights : Matrix<float>) = matrix [[]]
        
        /// delta weights produced from back propogation
        let mutable (deltaWeights : Matrix<float>) = matrix [[]]
        
        /// gradient of the error at the current neuron instance.
        let mutable (gradient : Matrix<float>) = matrix[[]]
        
        /// data fed into the network
        let mutable (data : Matrix<float>) = matrix [[]]
        
        /// error matrix resulting from backpropogation
        let mutable (error : Matrix<float>) = matrix [[]]
        
        /// Neuron bias applied to weight matrix.
        member n.Bias with get() = bias and set(b) = bias <- b
        /// temperature applied to activation
        /// see LogSig
        member n.Temperature with get() = temperature and set(t) = temperature <- t
        /// Amplitude applied to activation
        /// see LogSig
        member n.Amplitude with get() = amplitude and set(a) = amplitude <- a
        
        /// Range applied to activation
        /// see LogSig
        member n.Range with get() = range and set(r) = range <- r
        
        /// activation output
        member n.Output with get() = output and set(O) = output <- O
        ///  derivative output
        member n.Derivative with get() = derivative and set(D) = derivative <- D
        
        /// weights learnt from training data
        member n.Weights with get() = weights and set(W) = weights <- W
        
        /// delta weights produced from back propogation
        member n.DeltaWeights with get() = deltaWeights and set(W) = deltaWeights <- W
        
        /// gradient of the neuron with respect to the error.
        member n.Gradient with get() = gradient and set(G) = gradient <- G
        
        /// data fed into the network
        member n.Data with get() = data and set(D) = data <- D
         
        /// learning rate of back propogation
        member n.LearnRate with get() = learnRate and set(r) = learnRate <- r
        
        /// Momentum of batch propogation - used to prevent being trapped in gullies
        /// a small value between 0 and 1
        member n.Momentum with get() = momentum and set(m) = momentum <- m
        
        /// error matrix resulting from backpropogation
        member n.Errors with get() = error and set(e) = error <- e
        
        /// Create a neuron with weight matrix W 
        static member Create (W: Matrix<float>) =
            let n = new Neuron()
            n.Weights <- W
            n.DeltaWeights <- Matrix.zero (n.Weights.NumRows) (n.Weights.NumCols)
            n
        
        /// Activate the neuron
        /// Weight matrix
        /// Data matrix
        /// float bias
        /// Temperature
        /// Amplitude
        /// range
        member n.Activate (X : Matrix<float>) =
            n.Data <- X
            let M = Matrix.InsertMatrix  n.Weights 
                                        (Matrix.zero (n.Weights.NumRows + 1) n.Weights.NumCols)
            let R = Matrix.InsertRow (M.NumRows - 1) 
                                        (Vector.map (fun a -> a + n.Bias) (Vector.zero M.NumCols)).Transpose 
                                        M   
            let X2 = Matrix.InsertRow n.Data.NumRows 
                                      (Vector.map (fun a -> -1.0) (Vector.zero n.Data.NumCols)).Transpose 
                                      (Matrix.InsertMatrix n.Data (Matrix.zero (n.Data.NumRows + 1) n.Data.NumCols))
                                                                  
            n.Output <- Matrix.LogSig (R.Transpose * X2) n.Temperature n.Amplitude
            n.Derivative <- Matrix.DerivativeLogSig n.Output n.Temperature
            n.Output
       
       
       /// Bakpropogation may occur only after activation.
       /// This algorithm performs backpropogation for output nodes.
       /// C - Matrix of target classes (conformable with output of matrix)
       /// X - outputs of previous layer l - 1
       /// Outputs:
       /// Gradient G of the error
       /// Sideaffects:
       /// Updates the internal weight matrix.
       /// Updates the deltaWeight matrix
       /// Updates the Error Matrix
        member n.OutputLayerBackPropogate (C : Matrix<float>) (X : Matrix<float>) =
            n.Errors <- (C - n.Output)
            n.Gradient <- n.Derivative .* n.Errors
            let DW = (n.LearnRate * n.Gradient) * X.Transpose
            // repmat DW
            //let DW2 = (Matrix.RepMat DW n.DeltaWeights.NumRows n.DeltaWeights.NumCols);
            n.DeltaWeights <- (n.Momentum * n.DeltaWeights) + DW.Transpose
            n.Weights <- n.Weights + n.DeltaWeights;
            n.Gradient
        
        /// Perform back propogation for the hidden layer.
        /// W is the weights of next layer L+1
        /// X is input layer
        /// G is the gradient from the layer L+1
        member n.HiddenLayerBackPropogate (X : Matrix<float>) (N : Neuron)  =
            /// e = W*g;
            n.Errors <- N.Weights * N.Gradient
            /// g = do .* e;
            n.Gradient <- n.Derivative .* n.Errors
            /// dw = r*sum(x*g', 2);
            let Sum : Matrix<float> = Matrix.Mult (Matrix.SumColumns(X.Transpose*n.Gradient.Transpose).Transpose) n.LearnRate
            /// dw = repmat(dw, size(dw,2), size(W,1));
            /// dw = a*dW + dw';
            /// dW = dw;
            let cols = Sum.NumCols 
            let rows = n.Weights.NumRows
            let rep : Matrix<float> = (Matrix.RepMat Sum cols rows)
            n.DeltaWeights <- n.Amplitude*n.DeltaWeights + rep.Transpose
            // W = W + dw;
            n.Weights <- n.Weights + n.DeltaWeights
            n.Gradient
            
            
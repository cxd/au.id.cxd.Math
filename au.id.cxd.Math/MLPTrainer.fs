#light


namespace au.id.cxd.Math

    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Core.Operators
    
        
    type PublishErrorEventHandler = delegate of float -> unit
    
    /// A Multi Layer Perceptron Trainer.
    /// this is limited to a simple 3 layer network.
    /// Input Layer -> Hidden Layer -> Output Layer
    /// It uses the back propogation method of training.
    type MLPTrainer() =
        
        /// source data set.
        let mutable (source : Matrix<float>) = matrix[[]]
        /// data partition from source data set.
        let mutable (data : Matrix<float>) = matrix[[]]
        /// test partition from source data set.
        let mutable (test : Matrix<float>) = matrix[[]]
        /// The training outputs from the source data set.
        let mutable (trainOutputs : Matrix<float>) = matrix[[]]
        /// the test outputs from the source data set.
        let mutable (testOutputs : Matrix<float>) = matrix[[]]
        /// training errors.
        let mutable (trainErrors : Matrix<float>) = matrix[[]]
        /// testing errors.
        let mutable (testErrors : Matrix<float>) = matrix[[]]
        
        /// the percentage of data to use for training
        let mutable trainPercent = 70
        
        /// the number of epochs to train for
        let mutable epochs = 500;
        /// the error threshold at which to stop training
        let mutable errorThreshold = 0.0001;
        
        
        let mutable (hiddenLayer : Neuron) = new Neuron()
        
        let mutable (outputLayer : Neuron) = new Neuron()
        
        let mutable (errorPercent : float) = 0.0
        
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
        
        /// Accessor for source data
        member n.SourceData with get() = source and set(s) = source <- s
        /// Accessor for training data
        member n.TrainData with get() = data and set(d) = data <- d
        /// Accessor for test data.
        member n.TestData with get() = test and set(t) = test <- t
        /// the training outputs from the source data.
        member n.TrainOutputs with get() = trainOutputs and set(o) = trainOutputs <- o
        /// the test outputs from the source data.
        member n.TestOutputs with get() = testOutputs and set(o) = testOutputs <- o
        /// the training error vector
        member n.TrainErrors with get() = trainErrors and set(e) = trainErrors <- e
        /// the test error vector.
        member n.TestErrors with get() = testErrors and set(e) = testErrors <- e
        
        /// The percentage of data to use for training.
        member n.TrainPercent with get() = trainPercent and set(t) = trainPercent <- t


        member n.TestPercentError with get() = errorPercent and set(e) = errorPercent <- e
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

        /// learning rate of back propogation
        member n.LearnRate with get() = learnRate and set(r) = learnRate <- r
        
        /// Momentum of batch propogation - used to prevent being trapped in gullies
        /// a small value between 0 and 1
        member n.Momentum with get() = momentum and set(m) = momentum <- m
        
        /// the error threshold at which to stop training
        member n.ErrorThreshold with get() = errorThreshold and set(e) = errorThreshold <- e
        
        member n.HiddenLayer with get() = hiddenLayer and set(h) = hiddenLayer <- h
        
        member n.OutputLayer with get() = outputLayer and set(o) = outputLayer <- o
        
        /// partition source data into training and test data
        /// using 70% for training and 30% for testing
        /// This method must be called before attempting to train the network
        /// Additionally the last column (numcols - 1) MUST be the output column
        /// used to train the network.
        member n.PrepareData (D : Matrix<float>) =
            n.SourceData <- D
            let T = n.SourceData.PermuteRows
            
            // select the training data.
            let z = (int)((float)n.SourceData.NumRows * ((float)n.TrainPercent / 100.0))
            let u = (n.SourceData.NumRows - z)
            let c = n.SourceData.NumCols - 2
            let t = n.SourceData.NumCols - 1
            // training and test data are normalised.
            n.TrainData <- n.SourceData.[0..z, 0..c]
            n.TestData <- n.SourceData.[(z+1)..(n.SourceData.NumRows - 1), 0..c]
            n.TrainOutputs <- n.SourceData.[0..z, t..t] 
            n.TestOutputs <- n.SourceData.[(z+1)..(n.SourceData.NumRows - 1), t..t]
            n.TrainData <- Matrix.NormaliseColumns n.TrainData
            n.TrainOutputs <- Matrix.NormaliseColumns n.TrainOutputs
            n.TestData <- Matrix.NormaliseColumns n.TestData
            n.TestOutputs <- Matrix.NormaliseColumns n.TestOutputs
            n
        
            
        /// construct the hidden and output layers of the network
        /// define the number of outputs to allow for
        /// PreCondition:
        /// Data must have been defined, user must call PrepareData first.
        /// Post Condition:
        /// creates conformant hidden and output layers
        member n.PrepareNetwork numOutputs =
              
              let rnd = new System.Random()
              
              /// weights for input -> hidden layer
              n.HiddenLayer.Weights <- Matrix.map (fun a -> rnd.NextDouble() / (float)n.TrainData.NumRows) (Matrix.RepMat (Matrix.zero numOutputs n.TrainData.NumCols).Transpose numOutputs n.TrainData.NumCols)
              n.HiddenLayer.DeltaWeights <- Matrix.zero n.HiddenLayer.Weights.NumRows n.HiddenLayer.Weights.NumCols
              
              /// weights for hidden -> output layer
              n.OutputLayer.Weights <- Matrix.map (fun a -> rnd.NextDouble() / (float)n.TrainData.NumRows) (Matrix.zero n.HiddenLayer.Weights.NumCols numOutputs)
              n.OutputLayer.DeltaWeights <- Matrix.zero n.OutputLayer.Weights.NumRows n.OutputLayer.Weights.NumCols
               
              
              n.HiddenLayer.Amplitude <- n.Amplitude
              n.OutputLayer.Amplitude <- n.Amplitude
              n.HiddenLayer.Bias <- n.Bias
              n.OutputLayer.Bias <- n.Bias
              n.HiddenLayer.LearnRate <- n.LearnRate
              n.OutputLayer.LearnRate <- n.LearnRate
              n.HiddenLayer.Momentum <- n.Momentum
              n.OutputLayer.Momentum <- n.Momentum
              n.HiddenLayer.Temperature <- n.Temperature
              n.OutputLayer.Temperature <- n.Temperature
              n      
              
        /// Perform training by batch propogation.
        member n.Train(epochs, (fn : PublishErrorEventHandler) ) =
            /// initialise the error matrices
            n.TrainErrors <- Matrix.zero epochs 1
            let mutable i = 0  
            let mutable error = System.Double.MaxValue
            while (i < epochs) && (error > n.ErrorThreshold) do
                    let OO = n.Activate n.TrainData.Transpose
                    // backpropogate output first
                    let G = n.OutputLayer.OutputLayerBackPropogate n.TrainOutputs.Transpose n.HiddenLayer.Output
                    // backpropogate hidden layer
                    let W = n.OutputLayer.Weights
                    let HG = n.HiddenLayer.HiddenLayerBackPropogate n.TrainData n.OutputLayer
                    // calculate errors.
                    let E = (n.TrainOutputs.Transpose - n.OutputLayer.Output)
                    error <- 0.5 * Matrix.sum (E.*E)
                    if (fn <> null) then
                        fn.Invoke(error)
                    Matrix.set n.TrainErrors i 0 error
                    i <- i + 1
            n
            
            
        member n.Activate (D:Matrix<float>) =
            let HO = n.HiddenLayer.Activate D;
            let OO = n.OutputLayer.Activate HO;
            OO
            
            
       /// Perform testing by batch propogation.
        member n.Test =
            let mutable error = System.Double.MaxValue
            let OO = n.Activate n.TestData.Transpose
            n.TestErrors <- (n.TestOutputs.Transpose - n.OutputLayer.Output)
            let testMatrix = Matrix.zero n.TestErrors.NumRows n.TestErrors.NumCols
            let M = Matrix.mapi (fun i j a -> 
                                    match a with 
                                        | _ when (a > 0.0) -> (Matrix.set testMatrix i j 1.0)
                                                              a
                                        |_ -> Matrix.set testMatrix i j 0.0
                                              a) n.TestErrors
            n.TestPercentError <- (Matrix.sum testMatrix)/(float)n.TestOutputs.NumRows
            error <- 0.5 * Matrix.sum (n.TestErrors.*n.TestErrors)
            error
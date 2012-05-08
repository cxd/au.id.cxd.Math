namespace au.id.cxd.Text

open System
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser
open au.id.cxd.Text.Tokens
open au.id.cxd.Text
open au.id.cxd.Math.ClassifierMetrics

/// a simple naive bayes textual classifier to handle multi-class labelling
module NaiveBayesTextClassifier = 
    
    type TokenModel = (string*float)

    type ClassModel =
        { 
        Label:string;
        Data:Map<string,TokenModel>;
        ClassInstanceCount:float;
        ClassDocumentCount:float;
        }

    type NaiveBayesTextModel =
        {
            TotalDocuments:float;
            TotalTokenInstances:float;
            ClassModels:ClassModel list
        }

    /// find the class in the collection of counters
    let findClass maps cls =
        List.find ( fun { Label = c; Data = map; ClassInstanceCount = cnt; ClassDocumentCount = docs; } -> c = cls ) maps

    /// replace the class in the collection of counters
    let replaceClass maps {Label = cls; Data = map; ClassInstanceCount = cnt; ClassDocumentCount = docs; } =
        List.fold ( fun accum { Label = cl; Data = m; ClassInstanceCount = c; ClassDocumentCount = d; } ->
                        match cl = cls with
                        | true -> { Label = cls; Data = map; ClassInstanceCount = cnt; ClassDocumentCount = docs; } :: accum
                        | false -> { Label = cl;  Data = m; ClassInstanceCount = c; ClassDocumentCount = d } :: accum ) List.empty maps 

    /// train the existing model on the supplied data set and classes
    /// this method allows an existing model to be updated after earlier training.
    let trainModel (model:NaiveBayesTextModel) (data:ClassifiedSentences) classes =
        let results =
            data 
            |>
            Seq.fold (fun { ClassModels = maps; TotalTokenInstances = total; TotalDocuments = totalDocs; } (cls, tokens) -> 
                            let { Label = c; Data = map; ClassInstanceCount = cnt; ClassDocumentCount = docCnt; } = findClass maps cls
                            let (map', total') = 
                                List.fold ( fun (m, tcnt) token ->
                                                match (Map.containsKey token m) with
                                                | true ->
                                                    let (t, tcnt) = Map.find token m
                                                    (m |> Map.remove token |> Map.add token (t, tcnt+1.0), tcnt+1.0)
                                                | false ->
                                                    (Map.add token (token, 1.0) m, tcnt+1.0) 
                                                    ) (map, 0.0) (tokens |> Array.toList) 
                            let maps' = replaceClass maps { Label = c; Data = map'; ClassInstanceCount = cnt + total'; ClassDocumentCount = docCnt+1.0; }
                            { ClassModels = maps'; TotalTokenInstances = total+total'; TotalDocuments = totalDocs+docCnt+1.0; } 
                        ) model
        results

    /// create an empty model
    let makeEmptyModel classes = 
        let maps = List.map (fun cls -> {Label = cls; Data = Map.empty; ClassInstanceCount = 0.0; ClassDocumentCount = 0.0; } ) classes
        { ClassModels = maps; TotalTokenInstances = 0.0; TotalDocuments = 0.0; }

    /// train the classifier on the supplied data
    /// this results in 
    /// a NaiveBayesTextModel record 
    let train (data:ClassifiedSentences) classes =
        let maps = List.map (fun cls -> {Label = cls; Data = Map.empty; ClassInstanceCount = 0.0; ClassDocumentCount = 0.0; } ) classes
        trainModel { ClassModels = maps; TotalTokenInstances = 0.0; TotalDocuments = 0.0; } data classes
        
    /// using the supplied model and token list
    /// calculate the most likely class.
    let classify (model:NaiveBayesTextModel) (sentence:TokenList) =
        let map = Map.empty
        let priors =
            model.ClassModels 
            |> List.fold (fun map' cls -> Map.add cls.Label (Math.Log(cls.ClassDocumentCount / model.TotalDocuments)) map') map
        
        // the probability a single token belongs to a given class
        let prob { Label = c; Data = map; ClassInstanceCount = cnt; ClassDocumentCount = docCnt } (totalInstances:float) (word:string) =
            if (Map.containsKey word map) then
                let instCnt = (Map.find word map) |> snd
                Math.Log((instCnt+1.0)/(cnt + totalInstances))
            else 
                Math.Log(1.0 / (cnt + totalInstances))
        
        // calculate the likelihood for a token list for each class    
        let likelihoods = 
            List.map (fun cls -> 
                            let clsprob =
                                Array.fold (fun p word ->
                                                p + prob cls model.TotalTokenInstances word) 0.0 sentence
                            (cls, clsprob)) model.ClassModels 
            |> List.sortBy (fun (cls, clsprob) -> clsprob)
        // return the class label with the maximum likelihood
        likelihoods |> List.maxBy (fun (cls, clsprob) -> clsprob) |> fst |> (fun cls -> cls.Label)

    /// test the model and calculate the key statistics from the confusion matrix
    let testModel (model:NaiveBayesTextModel) (data:ClassifiedSentences) (classes:string list) =
        let confusionMatrix = 
            data
            |> Seq.toList
            |>
            List.fold ( fun (confusion:Matrix<float>) (token:ClassifiedTokenList) ->
                           let (cls, tokens) = token
                           let cls' = classify model tokens
                           let i = List.findIndex (fun (item:string) -> item.Equals(cls, StringComparison.OrdinalIgnoreCase)) classes
                           let j = List.findIndex (fun (item:string) -> item.Equals(cls', StringComparison.OrdinalIgnoreCase)) classes
                           confusion.[i,j] <- confusion.[i,j] + 1.0 
                           confusion) (Matrix.zero (List.length classes) (List.length classes))
        let metrics = calculateMetrics confusionMatrix (List.length classes)
        (metrics, confusionMatrix)
    ()


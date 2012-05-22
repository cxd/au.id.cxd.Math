// Learn more about F# at http://fsharp.net

(*

This module implements the cart training algorithm.

The data set that is uses is a collection of seq for each row of the table.

The data has a label column.
Each column of the data is also identified by a attribute name and an attribute type.
These must be assigned prior to training the model.

*)

namespace au.id.cxd.Math.Cart

open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.DataDescription
open au.id.cxd.Math.DataHistogram
open au.id.cxd.Math.DataProbabilities




module CartAlgorithm = 

    /// <summary>
    /// operations permitted at each rule level in the tree
    /// </summary>
    type Operator = 
        | EqualTo
        | NotEqualTo
        | LessThan
        | GreaterThanOrEqualTo

    /// <summary>
    /// rule body contains data and operator.
    /// (Operator, Attribute used for splitting, Datum for operator test, float is entropy of the value)
    /// </summary>
    type RuleBody = Operator * Attribute * Datum * float

    let ruleOp (op, attr, datum, entropy) = op
    let ruleAttr (op, attr, datum, entropy) = attr
    let ruleDatum (op, attr, datum, entropy) = datum
    let ruleEntropy (op, attr, datum, entropy) = entropy

    /// Information gain label
    type InfoGain = float

    /// <summary>
    /// Decision Tree structure.
    /// Each level of the decision tree can contain multiple children
    /// </summary>
    type DecisionTree =  
        | Rule of RuleBody * DecisionTree
        | Head of Attribute * InfoGain * DecisionTree List
        | Terminal of ClassificationAttribute 
        | Empty


    /// <summary>
    /// A path in a decision tree
    /// </summary>
    type Path = 
        | Top 
        | Node of DecisionTree * Path * DecisionTree
    
    /// <summary>
    /// A collection of decision trees is called a forest.
    /// </summary>
    type Forest = DecisionTree List

    let operationText op =
        match op with
        | NotEqualTo -> "NotEqualTo"
        | EqualTo -> "EqualTo"
        | LessThan -> "LessThan"
        | GreaterThanOrEqualTo -> "GreaterThanOrEqualTo"

    /// <summary>
    /// Compute the values that can potentially be used for histogram calculation of probability
    /// for a given attribute.
    /// this divides the sample space into N - 1 bins.
    /// The probability is approximated by the count of samples in each bin divided by the total samples
    /// This is also used as a measure for "split info" or splitting criteria.
    /// For continuous values the highest probability is used for splitting criteria.
    /// 
    /// Another approach would be to compute the guassian distribution instead.
    /// 
    /// Where V continuous values for a given attribute exists
    /// compute |V| - 1 values V' where
    /// V' = ( v.i + v.(i+1) ) / 2.0
    /// these are then used as input in computing the split info for a continuous float or int attribute.
    /// </summary>
    let splitCandidates (attr:Attribute) (data:DataTable) =
           let vals = 
                match attr.AttributeType with
                | Continuous ->  List.map (fun (datum:Datum) -> datum.FloatVal) ((columnAt attr.Column data).ProcessedData)
                | NumericOrdinal -> List.map (fun (datum:Datum) -> Convert.ToDouble(datum.IntVal)) ((columnAt attr.Column data).ProcessedData)
                | _ -> List.empty
           let sortedA = (List.sort vals) |> List.toSeq
           let sortedB = (List.sort vals) |> List.toSeq
           let skip = Seq.take 1 sortedB |> Seq.append [0.0]
           Seq.map2 (fun v1 v2 -> (v1 + v2) / 2.0) sortedA skip 
           |> Seq.take ((Seq.length sortedA) - 1) 
           |> Seq.toList

    
           
           
    /// <summary>
    /// Using the supplied data subset find the class label that is the maximum label represented in the population.
    /// 
    /// </summary>
    let majorityValue classColumnIdx (data:DataTable) (uniqueClasses:string Set) = 
           let classList = (List.nth (snd data) classColumnIdx).RawData
           let countItem item = List.fold (fun n (label:string) -> 
                                                match label.ToLower().Equals(item) with
                                                    | true -> n + 1
                                                    | false -> n) 0 classList 
           Set.map (fun item -> (item, countItem item)) uniqueClasses 
           |> Set.toList 
           |> List.sortBy (fun (itemA, a) -> a) 
           |> List.rev
           |> List.head 
           |> fst

    
    /// <summary>
    /// Compute the probability of each class in the data set.
    /// </summary>
    let classProbabilities (uniqueClasses:string Set) classCol (examples:DataTable) = 
            let counts = Set.map ( fun classLabel -> Convert.ToDouble(countClass classLabel classCol examples) ) uniqueClasses |> Set.toList
            let total = Convert.ToDouble(List.sum counts)
            List.map (fun cnt -> cnt / total) counts 
    

    /// <summary>
    /// The conditional probability of class Y.j occuring given example X.i
    /// 
    /// H(Y | X) = for H 1..j X 1..i P(X.i) * H(Y.j | X.i)  
    ///
    /// H(Y.j|X.i) is the conditional probability that class Y.j occurs given instance is X.i
    ///
    /// This returns a list of the conditional probabilities for the attribute instance given each unique class
    ///
    /// </summary>
    let conditionalProbabilities (instance:Datum) (attr:Attribute) (uniqueClasses:string Set) classColumnIdx (examples:DataTable) =
        let data = (columnAt attr.Column examples).ProcessedData
        let classList = (columnAt classColumnIdx examples).RawData
        // count the attribute X.i matching class Y.j 
        let countAttributeForClass classMatch = 
                match attr.AttributeType with
                    | Continuous
                    | NumericOrdinal -> 
                        // find the matching histogram for the instance
                        let hist = histogram attr examples
                        let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                        List.zip data classList |>
                        List.fold (fun n (datum, classLabel) -> 
                                        if datum.Missing then n
                                        else if (lt <= datum.FloatVal && datum.FloatVal <= gt && classLabel.Equals(classMatch)) then n + 1.0
                                        else n) 0.0
                    | String -> 
                        List.zip data classList |>
                        List.fold (fun n (datum, classLabel) -> 
                                        if datum.Missing then n
                                        else if datum.StringVal.Equals(instance.StringVal) && classLabel.Equals(classMatch) then n + 1.0
                                        else n) 0.0
                    | Bool -> 
                        List.zip data classList |>
                        List.fold (fun n (datum, classLabel) -> 
                                        if datum.Missing then n
                                        else if datum.BoolVal.Equals(instance.BoolVal) && classLabel.Equals(classMatch) then n + 1.0
                                        else n) 0.0
        // count the occurance of the attribute
        let attrCount = countAttributeValue instance attr examples
        Set.toList uniqueClasses |> List.map (fun classLabel -> (countAttributeForClass classLabel) / attrCount) 
        

    /// <summary>
    /// The measure of information entropy in a data set
    /// this is based only on the classes present within the dataset.
    /// I(P(v.1) .. P(v.i)) = SUM - P(v.i) log2 P(v.i)
    /// this is the probability of class v.i 
    /// the probability is calculated as
    /// count(v.i) / count(v.1 .. v.n) where n is the number of classes in v.
    /// </summary>
    let entropy (uniqueClasses:string Set) classCol (examples:DataTable)  = 
            let probabilities = classProbabilities uniqueClasses classCol examples
            /// add a small value of +0.0001 to the log function to prevent it returning NaN
            let logVals = List.map ( fun p -> -1.0 * (p * Math.Log((p+0.0001), 2.0) ) ) probabilities
            List.sum logVals

    /// <summary>
    /// Given an attribute, usually a continuous attribute.
    /// Find the value with the lowest conditional entropy in the column within the data set.
    /// Return it as a datum.
    /// If the attribute is a continuous attribute then the value returned is the midpoint to use for splitting criteria.
    /// </summary>
    let bestSplittingValue (attr:Attribute) (uniqueClasses:string Set) classCol (examples:DataTable) =
           // define unique values for X
            let uniqueData = uniqueAttributeValues attr examples
            // now there is unique data need to work out P(X)
            // for continuous values this is most likely the same length as the original data set.
            let probabilities = Seq.map (fun datum -> probabilityAttributeValue datum attr examples) uniqueData
            // now count the number of classes matching each instance in the data set.
            // for continuous values use the histogram approximation.
            let conditionalProbs = 
                    Seq.map (fun datum -> 
                                conditionalProbabilities datum attr uniqueClasses classCol examples) uniqueData
            let pairs = List.zip (Seq.toList probabilities) (Seq.toList conditionalProbs)
            // calculate the conditional entropy 
            let entropy = List.map (fun (p, ps) -> p * (List.sum (List.map (fun pn -> -1.0 * pn * Math.Log(pn+0.0001, 2.0)) ps) ) ) pairs
            let entropyPairs = List.zip (Set.toList uniqueData) entropy
            let bestPair = List.reduce (fun (d1, e1) (d2, e2) -> if e2 <= e1 then (d2, e2)
                                                                 else (d1, e1)) entropyPairs
            match attr.AttributeType with
            | NumericOrdinal 
            | Continuous -> 
                let hist = histogram attr examples
                let instance = fst bestPair
                let (lt, gt) = List.find (fun (a, b) -> instance.FloatVal >= a && instance.FloatVal <= b) hist 
                // to partition either side of the midpoint of the range offset it with the minimum value lt.
                ({ IntVal = 0; StringVal = ""; FloatVal = (lt + ((gt - lt) / 2.0) ); BoolVal = false; Missing = false; }, snd bestPair)        
            | _ -> bestPair

    /// <summary>
    /// The measure of entropy conditioned on a specified attribute.
    /// This is based on the classes present in the data set and the occurance of the attribute value for each class.
    /// For attribute X 
    /// For class Y
    /// 
    /// P(X.i) the probability of X.i
    ///
    /// The entropy is calculated as:
    ///
    /// H(Y | X) = for H 1..j X 1..i sum P(X.i) * H(Y.j | X.i)  
    ///
    /// H(Y.j|X.i) is the conditional probability that class Y.j occurs given instance is X.i
    ///
    /// </summary>
    let conditionalEntropy (uniqueClasses:string Set) classCol (attr:Attribute) (examples:DataTable) =
            // define unique values for X
            let uniqueData = uniqueAttributeValues attr examples
            // now there is unique data need to work out P(X)
            // for continuous values this is most likely the same length as the original data set.
            let probabilities = Seq.map (fun datum -> probabilityAttributeValue datum attr examples) uniqueData
            // now count the number of classes matching each instance in the data set.
            // for continuous values use the histogram approximation.
            let conditionalProbs = 
                    Seq.map (fun datum -> 
                                conditionalProbabilities datum attr uniqueClasses classCol examples) uniqueData
            let pairs = List.zip (Seq.toList probabilities) (Seq.toList conditionalProbs)
            // calculate the conditional entropy 
            // add 0.0001 to the value to allow for 0 sent to the log function
            List.map (fun (p, ps) -> p * (List.sum (List.map (fun pn -> -1.0 * pn * Math.Log((pn+0.0001), 2.0)) ps) ) ) pairs
            |> List.sum

    /// <summary>
    /// Calculate the information gain for the supplied attribute within the current set of examples
    /// </summary>
    let gain (uniqueClasses:string Set) classCol (attr:Attribute) (examples:DataTable) =
           if (attr.Column.Equals(classCol)) then Double.MinValue
           else 
            let e = (entropy uniqueClasses classCol examples)
            let ce = (conditionalEntropy uniqueClasses classCol attr examples)
            e - ce
    
    /// <summary>
    /// Choose an attribute by selecting the attribute with the maximum information gain
    /// </summary>
    let chooseAttribute (uniqueClasses:string Set) classCol (examples:DataTable) attribs =
            let attList = List.map (fun attr -> (attr, gain uniqueClasses classCol attr examples)) attribs
            List.reduce (fun (attr1, g1) (attr2, g2) -> if g2 >= g1 then (attr2, g2)
                                                        else (attr1, g1)) attList 

    /// <summary>
    /// Partition the data based on the rule body.
    /// Return two partitions the Left DataTable matches the rule body and the Right DataTable does not match.
    /// Use the class column to extract indices.
    /// </summary>
    let partitionData (op, (attr:Attribute), (datum:Datum), entropy) classCol (examples:DataTable) =
            let matches = Array.empty
            let others = Array.empty
            // the list for each column 
            let indices = List.toArray [0..(List.length (columnAt classCol examples).RawData)]
            // instead of partitioning the actual data process the operation and identify the indices
            // that correspond to the operation.
            let data = (columnAt attr.Column examples).ProcessedData
            // accumulate the indexes
            let accumulateIndices = match (op, attr.AttributeType) with
                                        | (EqualTo, Continuous) 
                                        | (EqualTo, NumericOrdinal) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if datum.FloatVal.Equals(item.FloatVal) then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (LessThan, Continuous) 
                                        | (LessThan, NumericOrdinal) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.FloatVal.CompareTo(datum.FloatVal) < 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (GreaterThanOrEqualTo, Continuous)
                                        | (GreaterThanOrEqualTo, NumericOrdinal) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.FloatVal.CompareTo(datum.FloatVal) > 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (EqualTo, String) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.StringVal.Equals(datum.StringVal) then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data
                                        | (NotEqualTo, String) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.StringVal.CompareTo(datum.StringVal) <> 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (LessThan, String) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.StringVal.CompareTo(datum.StringVal) < 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data
                                        | (GreaterThanOrEqualTo, String) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.StringVal.CompareTo(datum.StringVal) > 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (EqualTo, Bool) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if datum.BoolVal.Equals(item.BoolVal) then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (NotEqualTo, Bool) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if datum.BoolVal.Equals(item.BoolVal).Equals(false) then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (LessThan, Bool) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.BoolVal.CompareTo(datum.BoolVal) < 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data

                                        | (GreaterThanOrEqualTo, Bool) -> 
                                            List.fold (fun (n, matches, others) item -> 
                                                            if item.BoolVal.CompareTo(datum.BoolVal) > 0 then
                                                                (n + 1, Array.append matches [|n|], others)
                                                            else (n + 1, matches, Array.append others [|n|])) 
                                                      (0, matches, others) data
            let matchIndices (n, matches, others) = matches
            let matches = matchIndices accumulateIndices
            // apply the indices to select records from the data set. 
            let matchData = List.empty
            let otherData = List.empty
            
            let partitions data = 
                List.map (fun dataColumn ->
                           // associate indices with each row of the data
                           let pdata = List.mapi (fun i datum -> (i, datum)) dataColumn.ProcessedData
                           let rdata = List.mapi (fun i raw -> (i, raw) ) dataColumn.RawData
                           // partition on the indices
                           let (pMatch, pOther) = List.partition (fun (i, datum) -> Array.contains i matches) pdata
                           let (rMatch, rOther) = List.partition (fun (i, raw) -> Array.contains i matches) rdata
                           // convert partitions into pair of data columns (matches, other)
                           ({ Column = dataColumn.Column;
                             RawData = List.fold (fun rawList (i, raw) -> List.append rawList [raw]) List.empty rMatch;
                             ProcessedData = List.fold (fun pList (i, pdata) -> List.append pList [pdata]) List.empty pMatch;
                             },
                            { Column = dataColumn.Column;
                             RawData = List.fold (fun rawList (i, raw) -> List.append rawList [raw]) List.empty rOther;
                             ProcessedData = List.fold (fun pList (i, pdata) -> List.append pList [pdata]) List.empty pOther;
                             })
                             ) data
            // convert the partitions into two data tables (matchtable, othertable)
            List.fold (fun ((mList, mCols), (oList, oCols)) (mCol, oCol) -> 
                                ((mList, List.append mCols [mCol]), (oList, List.append oCols [oCol]))
                                ) ( ((fst examples), List.empty), ((fst examples), List.empty) ) (partitions (snd examples))

   

    
    /// <summary>
    /// The agorithm is based on AMIA Chapter 18 p658
    /// and on Data Mining, Han and Kambler Chapter 6.3 p 293
    /// This algorithm is a combination of the two algorithms from those books.
    ///
    /// function DECISION-TREE-LEARNING (examples, attribs, default) returns a decision tree
    ///     Inputs: examples, a set of data examples or rows
    ///             attributes, a set of attributes
    ///             defaultClass, value for the predicate (or classification) when examples is empty
    ///             multiBranchAllowed, a boolean flag to determine whether multi branching is allowed
    ///     if examples is empty return default
    ///     else if all examples have the same classification then return the classification
    ///     else if attribs is empty then return MAJORITY-VALUE(examples)
    ///     else
    ///         best <- CHOOSE-ATTRIBUTE(attribs, examples)
    ///         tree <- a new decision tree with root test as best
    ///         m <- MAJORITY-VALUE(examples)
    ///     foreach value v.i of best do
    ///         examples <- { elements of examples with best = v.i }
    ///         subsetOfAttributes = attribs
    ///         if best is discrete valued and multiBranched then
    ///             subsetOfAttributes = attribs - best
    ///         subtree <- DECISION-TREE-LEARNING(examples.i, subsetOfAttributes, m, multiBranched)
    ///         add a branch to tree with label v.i and subtree  
    ///     return tree
    /// </summary>
    let decisionTreeLearning classCol (examples:DataTable) defaultClass multiBranchAllowed =
        let emptyData = makeEmptyTable (fst examples)
        let rec learn rootNode nodeSet parentAttr classCol (examples:DataTable) (attribs:AttributeList) defaultClass buildFn =
            let len = List.length (columnAt classCol examples).RawData
            // if examples is empty return defaultTree
            if (isEmpty classCol examples) then 
                    match rootNode with
                    | Rule ( rule, dTree) -> buildFn ( Rule (rule, Terminal { Label = defaultClass; Column = classCol } ) )
                    | Head (attr, g, dlist) ->
                            match nodeSet with
                                | [] -> buildFn rootNode
                                | (n::ns) ->
                                    learn (fst n) [] parentAttr classCol (snd n) attribs defaultClass (fun child -> learn (Head (attr, g, child :: dlist)) ns parentAttr classCol emptyData attribs defaultClass buildFn)
                    | _ -> buildFn (Terminal { Label = defaultClass; Column = classCol } )
            else if attribs.IsEmpty then
                    match rootNode with
                    | Rule ( rule, dTree) -> buildFn ( Rule (rule, Terminal { Label = defaultClass; Column = classCol } ) )
                    | Head (attr, g, dlist) ->
                            match nodeSet with
                                | [] -> buildFn rootNode
                                | (n::ns) ->
                                    learn (fst n) [] parentAttr classCol (snd n) attribs defaultClass (fun child -> learn (Head (attr, g, child :: dlist)) ns parentAttr classCol emptyData attribs defaultClass buildFn)
                    | _ -> buildFn (Terminal { Label = defaultClass; Column = classCol } )
            else 
                let uniqueClasses = uniqueClasses classCol examples
                // if all classes are the same then this is a terminal node
                if (Set.count uniqueClasses).Equals(1) then 
                    match rootNode with
                    | Rule ( rule, dTree) -> buildFn ( Rule (rule, Terminal {Label = uniqueClasses |> Set.toList |> (fun lst -> List.nth lst 0); Column = classCol} ) )
                    | Head (attr, g, dlist) ->
                            match nodeSet with
                                | [] -> buildFn rootNode
                                | (n::ns) ->
                                    learn (fst n) [] parentAttr classCol (snd n) attribs defaultClass (fun child -> learn (Head (attr, g, child :: dlist)) ns parentAttr classCol emptyData attribs defaultClass buildFn) 
                    | _ -> buildFn (Terminal {Label = uniqueClasses |> Set.toList |> (fun lst -> List.nth lst 0); Column = classCol}) 
                else 
            
                    // m <- MAJORITY-VALUE(examples)
                    let majorityVal dataSet = majorityValue classCol dataSet uniqueClasses
                    
                    // best <- CHOOSE-ATTRIBUTE(attribs, examples)
                    let best dataSet = chooseAttribute uniqueClasses classCol dataSet attribs
            
                    // this is a head rule for the tree.
                    let selectAttribute next = 
                            if (len.Equals(2)) then
                                List.filter (fun attr2 -> next.AttributeLabel <> attr2.AttributeLabel) attribs
                            else
                            match (multiBranchAllowed, next.AttributeType) with
                            | (_, Continuous)
                            | (_, NumericOrdinal) -> attribs
                            | (true, _) -> List.filter (fun attr2 -> next.AttributeLabel <> attr2.AttributeLabel) attribs
                            | (false, _) -> attribs
                            | (_, _) -> attribs

                    // there are only 2 values left to choose from
                    match rootNode with
                        | Head (attr, g, dlist) ->
                            match nodeSet with
                                | [] -> buildFn rootNode
                                | (n::ns) ->
                                    learn (fst n) [] parentAttr classCol (snd n) attribs defaultClass (fun child -> learn (Head (attr, g, child :: dlist)) ns parentAttr classCol emptyData attribs defaultClass buildFn) 
                        | Rule (rule, dtree) -> 
                            let majority = majorityVal examples
                            // best <- CHOOSE-ATTRIBUTE(attribs, examples)
                            let (attr, g) = best examples
                            let subsetAttr = selectAttribute attr
    
                            match attr.AttributeType with
                                        | Continuous
                                        | NumericOrdinal -> 
                                            // find the best splitting value for a continuous variable
                                            let (datum, entropy) = bestSplittingValue attr uniqueClasses classCol examples
                                            let (lessThan, greaterThan) = partitionData (LessThan, attr, datum, entropy) classCol examples
                                            let childRules = [(Rule ((LessThan, attr, datum, entropy), Empty), lessThan);
                                                                (Rule ((GreaterThanOrEqualTo, attr, datum, entropy), Empty), greaterThan)]
                                            // discard the data set by supplying an empty list.
                                            learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority (fun child -> buildFn (Rule (rule, child)) )
                                        | String
                                        | Bool -> 
                                            if multiBranchAllowed then
                                                // Note if multi branching we do not record the entropy of the attribute value
                                                // only the gain of the parent attribute
                                                let childRules = List.map (fun datum -> 
                                                                            let (equal, notequal) = partitionData (EqualTo, attr, datum, g) classCol examples
                                                                            (Rule ((EqualTo, attr, datum, g), Empty), equal)) (uniqueAttributeValues attr examples |> Set.toList )
                                                learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority (fun child -> buildFn (Rule (rule, child)) )
                                            else 
                                                let (datum, entropy) = bestSplittingValue attr uniqueClasses classCol examples
                                                let (equal, notequal) = partitionData (EqualTo, attr, datum, g) classCol examples
                                                let childRules = [(Rule ((EqualTo, attr, datum, entropy), Empty), equal );
                                                                    (Rule ((NotEqualTo, attr, datum, entropy), Empty), notequal )]
                                                learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority (fun child -> buildFn (Rule (rule, child)) )
              
                        | Empty ->  
                            let majority = majorityVal examples
                            // best <- CHOOSE-ATTRIBUTE(attribs, examples)
                            let (attr, g) = best examples
                            let subsetAttr = selectAttribute attr
    
                            match attr.AttributeType with
                                        | Continuous
                                        | NumericOrdinal -> 
                                            // find the best splitting value for a continuous variable
                                            let (datum, entropy) = bestSplittingValue attr uniqueClasses classCol examples
                                            let (lessThan, greaterThan) = partitionData (LessThan, attr, datum, entropy) classCol examples
                                            let childRules = [(Rule ((LessThan, attr, datum, entropy), Empty), lessThan);
                                                                (Rule ((GreaterThanOrEqualTo, attr, datum, entropy), Empty), greaterThan)]
                                            learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority (fun id -> buildFn id)
                                        | String
                                        | Bool -> 
                                            if multiBranchAllowed then
                                                // Note if multi branching we do not record the entropy of the attribute value
                                                // only the gain of the parent attribute
                                                let childRules = List.map (fun datum -> 
                                                                            let (equal, notequal) = partitionData (EqualTo, attr, datum, g) classCol examples
                                                                            (Rule ((EqualTo, attr, datum, g), Empty), equal)) (uniqueAttributeValues attr examples |> Set.toList )
                                                learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority (fun id -> buildFn id)
                                            else 
                                                let (datum, entropy) = bestSplittingValue attr uniqueClasses classCol examples
                                                let (equal, notequal) = partitionData (EqualTo, attr, datum, g) classCol examples
                                                let childRules = [(Rule ((EqualTo, attr, datum, entropy), Empty), equal );
                                                                    (Rule ((NotEqualTo, attr, datum, entropy), Empty), notequal )]
                                                learn (Head (attr, g, [])) childRules attr classCol emptyData subsetAttr majority  (fun id -> buildFn id)
              
        let attribs = fst examples
        learn Empty [] { AttributeLabel = "NONE"; AttributeType = String; Column = 0; } classCol examples attribs defaultClass (fun id -> id)


    /// <summary>
    /// Iterate over each row and classify the result.
    /// </summary>
    let classifySample attribs examples dtree = 
            let rec classify row node =
                    match node with
                    | Head (attr, g, children) ->
                        // find the matching child rule for the current row
                        let ruleMatch = List.tryFind (fun child -> 
                                                       match child with
                                                       | Rule ((op, at, datum, entropy), dtree) -> 
                                                            let column = (columnAt at.Column examples).ProcessedData
                                                            let rowData = List.nth column row
                                                            match op with
                                                            | LessThan ->
                                                                if (at.AttributeType.Equals(Continuous) || at.AttributeType.Equals(NumericOrdinal)) then rowData.FloatVal.CompareTo(datum.FloatVal) < 0
                                                                else if at.AttributeType.Equals(String) then rowData.StringVal.ToLower().CompareTo(datum.StringVal.ToLower()) < 0
                                                                else rowData.BoolVal.CompareTo(datum.BoolVal) < 0
                                                            | GreaterThanOrEqualTo ->
                                                                if (at.AttributeType.Equals(Continuous) || at.AttributeType.Equals(NumericOrdinal)) then rowData.FloatVal.CompareTo(datum.FloatVal) >= 0
                                                                else if at.AttributeType.Equals(String) then rowData.StringVal.ToLower().CompareTo(datum.StringVal.ToLower()) >= 0
                                                                else rowData.BoolVal.CompareTo(datum.BoolVal) >= 0
                                                            | EqualTo ->
                                                                if (at.AttributeType.Equals(Continuous) || at.AttributeType.Equals(NumericOrdinal)) then rowData.FloatVal.CompareTo(datum.FloatVal).Equals(0)
                                                                else if at.AttributeType.Equals(String) then rowData.StringVal.ToLower().CompareTo(datum.StringVal.ToLower()).Equals(0)
                                                                else rowData.BoolVal.CompareTo(datum.BoolVal).Equals(0)
                                                            | NotEqualTo ->
                                                                if (at.AttributeType.Equals(Continuous) || at.AttributeType.Equals(NumericOrdinal)) then rowData.FloatVal.CompareTo(datum.FloatVal).Equals(0).Equals(false)
                                                                else if at.AttributeType.Equals(String) then rowData.StringVal.ToLower().CompareTo(datum.StringVal.ToLower()).Equals(0).Equals(false)
                                                                else rowData.BoolVal.CompareTo(datum.BoolVal).Equals(0).Equals(false)
                                                           
                                                           ) children
                        match ruleMatch with 
                        | Some rule -> classify row rule
                        | None -> "nomatch"
                    | Rule (body, dtree) -> classify row dtree
                    | Terminal classification -> classification.Label
                    | Empty -> failwith "Decision tree is invalid, terminates with Empty Node - No classification "
            let indices = [0..((List.length (columnAt 0 examples).RawData) - 1)]
            List.map (fun i -> classify i dtree) indices




    /// <summary>
    /// Classify the examples for the collection of decision trees.
    /// Use the majority class for each result.
    /// This will return a list of classes each item corresponds to a row in the input examples.
    /// </summary>
    let forestClassifySample attribs examples forest =
           let classes = List.map (fun dtree -> classifySample attribs examples dtree) forest
           List.map (fun classList ->
                            let unique = Set.ofList classList |> (Set.remove "nomatch") |> Set.toList
                            let counts = List.map (fun classLabel -> List.fold (fun n (result:string) -> 
                                                                                    match result.Equals(classLabel) with
                                                                                    | true -> n + 1
                                                                                    | _ -> n) 0 classList) unique
                            
                            let pairs = List.zip unique counts
                            // find the max value
                            let max = List.fold (fun (class1, cnt1) ((class2:string), cnt2) ->
                                                            if (cnt2 >= cnt1) then (class2, cnt2)
                                                            else (class2, cnt2)) ("nomatch", Int32.MinValue) pairs
                            fst max) classes



    /// <summary>
    /// Save a decision tree to a binary file
    /// </summary>
    let binarySerialize file dtree =
          let writer = File.OpenWrite(file)
          let serializer = new BinaryFormatter()
          serializer.Serialize(writer, dtree)
          writer.Flush()
          writer.Close()

    /// <summary>
    /// Read a decision tree from a binary file
    /// </summary>
    let readSerializedTree file =
          let reader = File.OpenRead(file)
          let serializer = new BinaryFormatter()
          let data = serializer.Deserialize(reader)
          reader.Close()
          data :?> DecisionTree

    /// <summary>
    /// Read a Forest from a binary file
    /// </summary>
    let readSerializedForest file =
          let reader = File.OpenRead(file)
          let serializer = new BinaryFormatter()
          let data = serializer.Deserialize(reader)
          reader.Close()
          data :?> Forest

    /// <summary>
    /// Test the decision tree with the test data in the learning data set
    /// Return the percent accuracy
    /// </summary>
    let testDTree learningData dtree = 
           let data = learningData.TestData
           let attribs = fst data
           let results = classifySample (fst data) data dtree
           let classes = (columnAt learningData.ClassColumn data).RawData
           let totals = List.map2 (fun (a:string) (b:string) -> if a.ToLower().Equals(b.ToLower()) then 1.0
                                                                else 0.0) classes results
           (List.sum totals) / Convert.ToDouble(List.length classes)

    
    /// <summary>
    /// Train a decision tree
    /// Inputs: learningData -> LearningData
    /// defaultClass -> the default class to use
    /// enableMultiway -> determine if the tree can have M-way splits or whether it is a binary tree
    /// Provide a file name to store the resulting tree in.
    /// </summary>
    let trainDTree learningData defaultClass enableMultiway storeInFile =
            let dtree = decisionTreeLearning learningData.ClassColumn learningData.TrainData defaultClass enableMultiway
            // save the forest in the supplied file.
            binarySerialize storeInFile dtree
            let accuracy = testDTree learningData dtree
            (dtree, accuracy)
        
    /// <summary>
    /// Test the forest of decision trees against the learning data
    /// Return the percent accuracy
    /// </summary>
    let testForest learningData forest = 
           let data = learningData.TestData
           let attribs = fst data
           let results = forestClassifySample (fst data) data forest
           let classes = (columnAt learningData.ClassColumn data).RawData
           let matches = List.map (fun dtree -> 
                                        let matchResult = classifySample attribs learningData.TestData dtree
                                        let totals = List.map2 (fun (a:string) (b:string) -> 
                                                                    if a.ToLower().Equals(b.ToLower()) then 1.0
                                                                    else 0.0) classes matchResult
                                        (List.sum totals) / Convert.ToDouble(List.length classes)) forest
           (List.sum matches) / Convert.ToDouble(List.length matches)


    /// <summary>
    /// Train a forest of decision tree given the input data sequence.
    /// Return a 3 tuple (forest, accuracy list, average accuracy)
    /// This will train and test the trees asynchronously, so for N partitions in the data set
    /// this will execute N threads for training and testing.
    /// supply a file name to store the forest after training.
    /// </summary>
    let trainForest seqData defaultClass enableMultiway storeFile = 
            let learn learningData = async { return decisionTreeLearning learningData.ClassColumn learningData.TrainData defaultClass enableMultiway }
            let forest = Seq.map learn seqData |> Async.Parallel |> Async.RunSynchronously
            // save the forest in the supplied file.
            binarySerialize storeFile forest
            // calculate average accuracy
            let test learningData = async { return testForest learningData (forest |> Seq.toList) }
            let results = Seq.map test seqData |> Async.Parallel |> Async.RunSynchronously
            ((forest |> Seq.toList), results |> Seq.toList, (Seq.sum results) / Convert.ToDouble(Seq.length results))

    

namespace au.id.cxd.Math.Cart

open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.Cart.CartAlgorithm


module DTreeLearn3 =

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
let decisionTreeLearningTest classCol (examples:DataTable) defaultClass multiBranchAllowed =
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



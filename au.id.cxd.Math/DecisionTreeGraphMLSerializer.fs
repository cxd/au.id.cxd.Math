namespace au.id.cxd.Math.Cart

module DecisionTreeGraphMLSerializer =
    
    open System
    open System.IO
    open System.Xml
    open System.Xml.Serialization
    open au.id.cxd.Math.Cart.CartAlgorithm
    open au.id.cxd.Math.TrainingData
    open org.graphdrawing.graphml

    

    /// <summary>
    /// Write the decision tree to an graphml file for inspection
    /// </summary>
    let writeTreeToGraphML (file:string) name tree = 
      let additem item (g:graphtype) = 
            let items = item :: (Array.toList g.Items)
            g.Items <- List.toArray items
            g
      let rec write graph subtree parentId =
            match subtree with
                | Rule (body, dtree) -> 
                    let datumVal = if (ruleAttr body).AttributeType.Equals(Continuous) then (ruleDatum body).FloatVal.ToString()
                                   else if (ruleAttr body).AttributeType.Equals(NumericOrdinal) then (ruleDatum body).FloatVal.ToString()
                                   else if (ruleAttr body).AttributeType.Equals(Bool) then (ruleDatum body).BoolVal.ToString()
                                   else (ruleDatum body).StringVal
                    let node = new nodetype( id = Guid.NewGuid().ToString(),
                                             Items = [|new datatype(key="name", Text = [|operationText (ruleOp body) + " " + datumVal|]);
                                                       new datatype(key = "color", Text = [|"blue"|]) |] )
                    let g' = additem node graph
                    write (additem (new edgetype(source = parentId, target = node.id)) g') dtree node.id
                | Head (at, gain, dtreeList) -> 
                    let text = at.AttributeLabel + " Gain: " + gain.ToString("0.00")
                    let node = new nodetype( id = Guid.NewGuid().ToString(),
                                             Items = [|new datatype(key="name", Text = [|text|]);
                                                       new datatype(key = "color", Text = [|"yellow"|]) |] )
                    let g' = additem node graph
                    let g'' = List.fold(fun g dtree -> write g dtree node.id) g' dtreeList
                    if System.String.IsNullOrEmpty(parentId) then g''
                    else additem (new edgetype(source = parentId, target = node.id)) g''
                    
                | Terminal classAttr -> 
                    let text = classAttr.Label
                    let node = new nodetype( id = Guid.NewGuid().ToString(),
                                             Items = [|new datatype(key="name", Text = [|text|]);
                                                       new datatype(key = "color", Text = [|"green"|]) |] )
                    let g' = additem node graph
                    if System.String.IsNullOrEmpty(parentId) then g'
                    else additem (new edgetype(source = parentId, target = node.id)) g'
                | Empty -> graph
      let serializer = new XmlSerializer(typeof<graphmltype>)
      let graphroot = new graphmltype()
      let nodeName = new keytype(
                                 keyFor = keyfortype.node,
                                 id = "name",
                                 attributeName="name",
                                 attributeType="string"
                                 )
      let nodeColor = new keytype(
                                    keyFor = keyfortype.node,
                                    id = "color",
                                    attributeName = "color",
                                    attributeType = "string",
                                    defaultType = new defaulttype(Text = [| "yellow" |])
                                  )
      let graph = write (new graphtype(id = name, edgedefault = graphedgedefaulttype.directed, Items = Array.empty)) tree ""
      let root = new graphmltype(key = [|nodeName; nodeColor|], Items = [|graph|])
      serializer.Serialize(new StreamWriter(file), root)
      
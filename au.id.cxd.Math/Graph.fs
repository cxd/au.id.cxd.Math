namespace au.id.cxd.Collections

open System
open au.id.cxd.Collections.Stack

module Graph =

    // a node may contain data of type g and always has a guid
    type 'g Node = Node of Guid * 'g
    
    /// generate a new node for data g
    let makeNode g = Node (Guid.NewGuid(), g)
    
    /// an arc between two nodes
    type Edge = Edge of Guid * Guid * float

    /// make an edge between two nodes
    let makeEdge (from:'g Node) (tonode:'g Node) (weight:float) =
        let (Node (id1, g1)) = from
        let (Node (id2, g2)) = tonode
        (id1, id2, weight)
    
    /// a graph container
    type 'g Graph = 
        Graph of (Edge list * 'g Node * Edge list) list
    
    /// bootstrap a graph
    let makeGraph rest = Graph rest
    
    /// extract all nodes in the graph
    let extractNodes ((Graph graph):'g Graph) =
        List.fold (fun st (inedges, node, outedges) -> node :: st) [] graph
    
    let depthFirst (visitFn:('a -> 'b -> unit)) (graph:'g Graph) =
        ()
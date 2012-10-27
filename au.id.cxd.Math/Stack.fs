namespace au.id.cxd.Collections

module Stack =
    
    type 'a Stack =
        | Empty
        | Cell of 'a
        | Stack of ('a Stack) * ('a Stack)

    /// make an empty stack
    let makeStack = Empty

    /// push a value onto the stack
    let push (stack:'a Stack) a =
        Stack (Cell a, stack)
        
    /// determine if the stack is empty
    let isempty (stack:'a Stack) =
        match stack with
        | Empty -> true
        | _ -> false
    
    /// pop a value from the top of the stack
    let pop (stack:'a Stack) =
        match stack with
        | Cell a -> Some (a, Empty)
        | Stack (Cell a, rest) -> Some (a, rest)
        | _ -> None
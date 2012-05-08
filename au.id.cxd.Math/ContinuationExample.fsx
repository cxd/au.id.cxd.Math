
type Tree<'a> = Empty | Elem of 'a Tree * 'a * 'a Tree

let empty = Empty

(*
This is an example of a traversal that builds up a new tree.
*)
let insert' x t =
    let rec cont x t k =
        match t with
        | Empty -> k (Elem(empty,x,empty))
        | Elem (left,e,right) when x < e -> cont x left (fun t' -> k (Elem (t',e,right)))
        | Elem (left,e,right) when x > e -> cont x right (fun t' -> k (Elem (left,e,t')))
        | Elem _ -> k t
    cont x t id


let test = Elem ( Elem (Empty, 1, Empty), 2, Elem (Empty, 4, Empty) )
let test2 = insert' 3 test
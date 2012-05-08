namespace au.id.cxd.Math

module MaybeBuilder =
    
    type Maybe<'a> = option<'a>

    let succeed x = Some x
    let fail = None
    let bind x f =
        match x with
        | None -> None
        | Some x' -> f x'
    let delay f = f()

    type MaybeBuilder() =
        member m.Return(x) = succeed x
        member m.Delay(f) = delay f
        member m.Let(x, f) = f x
        member m.Bind(x, f) = bind x f

    let maybe = new MaybeBuilder()

namespace au.id.cxd.Math.UI 

open System.Windows
open System.Windows.Controls
open System

(*
State monad links.

http://cs.hubfs.net/forums/thread/7472.aspx

http://stevehorsfield.wordpress.com/2009/09/11/f-pipelined-monads/

http://stackoverflow.com/questions/2595673/state-monad-why-not-a-tuple

*)

module StateM =

    type State<'state, 'a> = State of ('state -> 'a * 'state)

    /// <summary>
    /// monad identity function.
    /// </summary>
    let returnM aval = State (fun state -> (aval,state))

    /// <summary>
    /// Execute the state.
    /// </summary>
    let runState (State stateFn) state = stateFn state 

    /// <summary>
    /// bind a state value to a state function.
    /// </summary>
    let bind sval stateFn = 
        State (fun state ->
                    let (retVal, newState) = runState sval state
                    runState (stateFn retVal) newState
                )

    /// <summary>
    /// Delay the operation to generate a new state.
    /// </summary>
    let delay stateFn = State (fun state -> runState (stateFn) state)

    type StateBuilder() = 
            
            member m.Return(aval) = returnM aval

            member m.Bind(sval, stateFn) = bind sval stateFn

            member m.Zero () = returnM ()

            member m.Combine(stateM1, stateM2) = bind stateM1 (fun () -> stateM2)


    /// <summary>
    /// The state builder monad
    /// </summary>
    let statebuilder = new StateBuilder()

    let getState = State(fun s -> (s, s))

    let setState aval = State(fun _ -> ((), aval))

    let mapState mapFn = State (fun s -> (s, mapFn s))

    /// <summary>
    /// Execute the state.
    /// </summary>
    let execState stateM stVal = 
        let v,_ = runState stateM stVal
        v
    
    /// <summary>
    /// Evaluate the state.
    /// </summary>
    let evalState stateM stVal = 
        let _,s = runState stateM stVal
        s

    // and example state function.
    let exampleForState test =
        statebuilder {
            let! a = getState
            Console.WriteLine("ExampleForState {0}", a)
            do! setState [|"test2"|]
            let! b = getState
            Console.WriteLine("Example2 {0}", b)
            return ()
        }

    let runExampleForState a = runState (exampleForState "") [|"testState"|]
   
    /// <summary>
    /// Make a state instance
    /// </summary>
    let makeState s = statebuilder {
        do! setState s
        return ()
    }
namespace au.id.cxd.Math.UIBuilder

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Documents

module MathUIBuilder =

    /// <summary>
    /// UI function
    /// </summary>
    type MathUI = (unit -> UIElement)
    
    /// the default wrapped ui
    let wrapUI ui = (fun () -> ui)

    let returnUI (uiM:MathUI) = uiM()

    // bind (ui:MathUI) (UIElement -> MathUI) -> MathUI
    /// invoke the MathUI monad and pipe the result into the build function.
    let bind (uiM:MathUI) (bindFn:(UIElement -> MathUI)) = uiM() |> bindFn

    // bind defined as an operator 
    let (>>=) (uiM:MathUI) (bindFn:(UIElement -> MathUI)) = uiM() |> bindFn

    /// take a (unit -> UIElement) and defer its execution
    let delay uiFn = returnUI (uiFn())
    
     /// <summary>
    /// Wrap a window within a monad
    /// </summary>
    let newWindow title = wrapUI (new Window(Title = title) :> UIElement)

    /// <summary>
    /// Convert to UIElement compatible with the builder monad.
    /// </summary>
    let uicomponent ui = (ui :> UIElement)

    /// <summary>
    /// A function used to help compose interfaces.
    /// Has no side effect on the parameters passed in.
    /// </summary>
    let nochange parent child = parent

    /// <summary>
    /// helper methods to construct composable elements
    /// fun parent -> postFn (ui ui -> ui) -> child -> parent
    /// </summary>
    let add (parent:UIElement) postFn (child:UIElement)  = 
                                    let cm = child
                                    match parent with 
                                        | :? ContentControl -> (parent :?> ContentControl).Content <- cm
                                        | :? Panel -> (parent :?> Panel).Children.Add(cm) |> ignore
                                        | :? ItemsControl -> (parent :?> ItemsControl).Items.Add(cm) |> ignore
                                        | :? Page -> (parent :?> Page).Content <- cm
                                    postFn parent child |> wrapUI 
    
    /// <summary>
    /// A builder method to accumulate Document elements within
    /// the UI.
    /// </summary>
    let addDoc (parent:ContentElement) postFn (child:ContentElement) = 
        let cm = child
        match parent with
        | :? FlowDocument ->
            match child with
            | :? Block -> (parent :?> FlowDocument).Blocks.Add(child :?> Block)
            | :? Inline -> 
                let para = new Paragraph()
                para.Inlines.Add(child :?> Inline)
                (parent :?> FlowDocument).Blocks.Add(para)
            | _ -> ()
        | :? Paragraph ->
            match child with
            | :? Inline -> (parent :?> Paragraph).Inlines.Add(child :?> Inline)
            | _ -> ()
        | :? Span ->
            match child with
            | :? Inline -> (parent :?> Span).Inlines.Add(child :?> Inline)
            | _ -> ()
        | :? Section ->
            match child with
            | :? Block -> (parent :?> Section).Blocks.Add(child :?> Block)
            | :? Inline -> 
                let para = new Paragraph()
                para.Inlines.Add(child :?> Inline)
                (parent :?> Section).Blocks.Add(para)
            | _ -> ()
        | :? Table ->
            match child with
            | :? TableRowGroup ->
                  (parent :?> Table).RowGroups.Add(child :?> TableRowGroup)
            | :? TableColumn ->
                  (parent :?> Table).Columns.Add(child :?> TableColumn)
            | _ -> ()
        | :? TableRowGroup ->
            match child with
            | :? TableRow ->
                (parent :?> TableRowGroup).Rows.Add(child :?> TableRow)
            | _ -> ()
        | :? TableRow ->
            match child with
            | :? TableCell ->
                (parent :?> TableRow).Cells.Add(child :?> TableCell)
            | _ -> ()
        | :? TableCell ->
            match child with
            | :? Block ->
                (parent :?> TableCell).Blocks.Add(child :?> Block)
            | _ -> ()
        | _ -> ()
        postFn parent child |> wrapUI

    let clearDoc (parent:ContentElement) = ()

    /// <summary>
    /// Clear the children of a ui element
    /// </summary>
    let clear (parent:UIElement) =
        match parent with
        | :? ContentControl -> (parent :?> ContentControl).Content <- null
        | :? Panel -> (parent :?> Panel).Children.Clear()
        | :? ItemsControl -> (parent :?> ItemsControl).Items.Clear()

    /// <summary>
    /// Update the parent ui element.
    /// </summary>
    let update (parent:UIElement) = parent.UpdateLayout();
    
    /// <summary>
    /// This childui allows a function that is compatible with "appendUI" composition or "insertUI" monad
    /// to be declared for a childui component that will be used at a later time.
    /// </summary>
    let childui elem changeFn = (fun addFn -> (addFn, changeFn, uicomponent elem))

    /// <summary>
    /// The childDoc allows for a function that is compatible with "appendUI" composition or "insertUI" monad
    /// to be declared for a childDoc component that will be used at a later time.
    /// </summary>
    let childDoc elem changeFn = (fun addFn -> (addFn, changeFn, elem))

    /// <summary>
    /// appendUI can be used to chain togethor series of append statements
    /// in order to use function composition (arrow operator in F#) to construct a nested user interface.
    /// The return value of an appendUI can be issued into a "childui" function.
    /// </summary>
    let appendUI (addFn, postFn, child) = 
                addFn postFn child |> ignore
                addFn
    
    /// <summary>
    /// an insert ui can be used with the uibuilder monad to extract 
    /// the UIElement out of a tuple generated by a childui function.
    /// This allows the UIElement to be used in mixed calls to the add function.
    /// </summary>                           
    let insertUI (addFn, postFn, child) =
        addFn postFn child


    /// <summary>
    /// UI Builder monad
    /// </summary>
    type UIBuilder() =

        /// wraps an ordinary UI value into a MathUI (unit -> UIElement) monad.
        member b.Return(ui) = wrapUI ui
        
        /// invoke bind on a ui monad.
        member b.Bind(uiM, fn) = bind uiM fn

        // delay the function which builds a MathUI until it is constructed
        member b.Delay(fn) = delay fn

        // take a raw UIElement and wrap it using the supplied function
        member b.Let(ui, wrapFn) = wrapFn ui

       
    /// this is the instance of the UI builder.   
    let uibuild = new UIBuilder()
          
 


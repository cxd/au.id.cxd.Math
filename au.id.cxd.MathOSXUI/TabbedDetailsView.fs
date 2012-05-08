namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.Math.MathProject

module TabbedDetailsView =

    /// <summary>
    /// Build the tabbed details view
    /// </summary>
    let buildTabbedView parent (appState:ApplicationState) =
        let view = new NSTabView()
        let makeTab name =
            let t = new NSTabViewItem()
            t.Label <- name
            t.Identifier <- new NSString(name)
            t

        let selectTabName (name:string) =
            view.Select(new NSString(name))

        let selectItem (tab:NSTabViewItem) =
            if (tab <> null) then
                view.Select(tab)
            else ()

        appState
           .PerformActivity
           .Add(fun activity ->
                    logfmt "Run Activity {0}" [| activity |]
                    match activity with
                    | LoadData ds ->
                        selectTabName "Source Data"
                    | ReviewData cols ->
                        selectTabName "Single Value Analysis"
                    | DualReviewData columns ->
                        selectTabName "Dual Value Analysis"
                    | MultiReviewData columns ->
                       selectTabName "Review Multiple Variables"
                    | _ -> ()
                    ())
                                   
            
        let tabView =
            childui (view)
                    (fun parent (child:NSView) ->
                     let sourceTab =
                         childtab (makeTab "Source Data")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   
                                   parent)
                     let attribTab =
                         childtab (makeTab "Define Attributes")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   parent)
                     let singleTab =
                         childtab (makeTab "Single Value Analysis")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   
                                   parent)

                     let dualTab =
                         childtab (makeTab "Dual Value Analysis" )
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   
                                   parent)

                     let multiTab =
                         childtab (makeTab "Multi Value Analysis")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   
                                   parent)

                     let trainTab =
                         childtab (makeTab "Training Data")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   parent)

                     let testTab =
                         childtab (makeTab "Test Data")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   parent)

                     let modelTab =
                         childtab (makeTab "Model Summary")
                                  (fun parent (tab:NSTabViewItem) ->
                                   tab.View <- new NSView()
                                   parent)


                     

                     (sourceTab (addTab (child :?> NSTabView))) |>
                     (appendUI
                      >> attribTab
                      >> appendUI
                      >> singleTab
                      >> appendUI
                      >> dualTab
                      >> appendUI
                      >> multiTab
                      >> appendUI
                      >> trainTab
                      >> appendUI
                      >> testTab
                      >> appendUI
                      >> modelTab
                      >> appendUI) |> ignore


                     
                     parent)
        (tabView (add parent)) |> appendUI |> ignore
        

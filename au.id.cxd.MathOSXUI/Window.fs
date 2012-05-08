namespace au.id.cxd.MathOSXUI

open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.MathMenu
open au.id.cxd.MathOSXUI.TabbedDetailsView
open au.id.cxd.MathOSXUI.ProjectTree

module Window =

    
    /// <summary>
    /// The math window state
    /// </summary>
    type MathWindowAdapter(parent:NSWindow) =
        let parentWindow = parent
        
        member m.testWindow() =
            parentWindow.BackgroundColor <- NSColor.Blue

        // todo work out how to embed views into the window
        // and follow up with window layout.

        /// <summary>
        /// Build the UI
        /// </summary>
        member m.build() =

            let app = new ApplicationState()
            
            buildMenu app

            let addView = add parentWindow.ContentView
            resizeWidthHeight parentWindow.ContentView

            let bounds = parentWindow.ContentRectFor(parentWindow.ContentView.Bounds)
            
            let split =
                childui (new NSSplitView(innerbounds 5.0f bounds))
                        (fun parent (child:NSView) ->
                         let splitView = (child :?> NSSplitView)
                         splitView.IsVertical <- true
                         
                         resizeWidthHeight splitView

                         let outline =
                             childui (projectTree app)
                                     (fun parent (child:NSView) ->
                                      let rect = new RectangleF(bounds.X, bounds.Y, bounds.Width*0.8f, bounds.Height)
                                      //child.Bounds <- innerbounds 10.0f rect
                                      
                                      parent)
                         
                         (outline (add child)) |>
                         appendUI
                         |> ignore

                         
                         buildTabbedView child app


                         parent)
            
            (split addView) |> appendUI |> ignore

            parentWindow.ViewsNeedDisplay <- true
            
            
        
            
        

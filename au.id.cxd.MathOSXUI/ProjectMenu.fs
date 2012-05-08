namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.Math.MathProject
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.FileBrowser

module ProjectMenu =
    /// <summary>
    /// Build the file menu
    /// </summary>
    let projectMenu (app:ApplicationState) =
        clearMenu "Project"
        let menuItem = new NSMenuItem("Project")
        let menu = new NSMenu()
        menu.Title <- "Project"
        let addTo = addMenu menu

        let newproject =
            childmenu (new NSMenuItem("New Project"))
                      (fun parent (child:NSMenuItem) ->

                           child
                              .Activated
                              .AddHandler(
                                 fun sender evt ->
                                     app.ClearActivities()
                                     
                                     ())
                            
                           parent)

        let activities =
            childmenu (new NSMenuItem("Activities"))
                      (fun parent (child:NSMenuItem) ->
                       let childMenu = new NSMenu()
                       child.Submenu <- childMenu
                       let addChild = addMenu childMenu

                       let load =
                           childmenu (new NSMenuItem("Load Data"))
                                     (fun parent (child:NSMenuItem) ->
                                      
                                          child
                                             .Activated
                                             .AddHandler(
                                                fun sender evt ->
                                                    let data = chooseData
                                                                 (fun item ->
                                                                      LoadData( { DataSource = item } ) )
                                                                 NoActivity
                                                    /// store the activity
                                                    app.AddActivity(data)
                                                    /// update the tree view menu.

                                                    /// populate the tree view

                                                    /// update the attribute panel
                                                    
                                                    ())
                           
                                          parent)
                       
                       let review =
                           childmenu (new NSMenuItem("Review Data"))
                                     (fun parent (child:NSMenuItem) ->
                                         
                                          child
                                             .Activated
                                             .AddHandler(
                                                fun sender evt ->
                                                    // review data
                                                    let data = ReviewData List.empty
                                                    app.AddActivity(data)
                                                    ())
                           
                                          parent)

                       let dualdata =
                           childmenu (new NSMenuItem("Review Dual Data"))
                                     (fun parent (child:NSMenuItem) ->
                                      
                                          child
                                             .Activated
                                             .AddHandler(
                                                fun sender evt ->
                                                    let data = DualReviewData List.empty
                                                    app.AddActivity(data)
                                                    ())
                           
                                       
                                          parent)
                       (load addChild) |>
                       (appendUI
                        >> review
                        >> appendUI
                        >> dualdata
                        >> appendUI) |> ignore

                       parent)
                      
        (newproject addTo) |>
        (appendUI
         >> activities
         >> appendUI) |> ignore

        menuItem.Submenu <- menu
        menuItem

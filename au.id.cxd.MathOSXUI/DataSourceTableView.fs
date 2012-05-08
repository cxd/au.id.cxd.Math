namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.Math.MathProject

module DataSourceTableView =

    type RawTableDataSource(app:ApplicationState) =
        inherit NSTableViewDataSource()

        let appState = app

        

    
    /// <summary>
    /// when the user selects the activity
    /// process the event and display the appropriate data
    /// </summary>
    let handleActivity (appState:ApplicationState) =
        appState
           .PerformActivity
           .Add(fun activity ->
                    logfmt "Run DataSource Activity {0}" [| activity |]
                    match activity with
                    | LoadData ds ->
                       // display the loaded data. 
                       ()
                    | _ -> ())


    /// <summary>
    /// Build the view that will display the data source
    /// </summary>
    let buildDataSourceView parent (appState:ApplicationState) =
        ()
        

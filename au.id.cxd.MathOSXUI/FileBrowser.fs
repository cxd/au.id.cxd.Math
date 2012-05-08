namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.Math.MathProject
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.MathOSXUI.OSXUIBuilder


module FileBrowser =

    /// <summary>
    /// choose the file to open and store it in the application state
    /// </summary>
    let chooseData assignFn failure =
        let openPanel = NSOpenPanel.OpenPanel
        openPanel.CanChooseFiles <- true
        openPanel.CanChooseDirectories <- false
        openPanel.AllowedFileTypes <- [|"*.csv"; "*.data"; "*.txt"|]
        let result = openPanel.RunModal()
        if (result = int NSPanelButtonType.Ok) then
            assignFn (openPanel.Url.Path)
        else failure

    
    
    ()

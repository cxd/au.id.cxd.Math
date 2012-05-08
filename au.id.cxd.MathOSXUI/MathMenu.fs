namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.MathOSXUI.FileMenu
open au.id.cxd.MathOSXUI.EditMenu
open au.id.cxd.MathOSXUI.ProjectMenu


module MathMenu =

    let buildMenu (app:ApplicationState) =
        fileMenu app |> addApplicationMenu
        editMenu app |> addApplicationMenu
        projectMenu app |> addApplicationMenu
        
        ()

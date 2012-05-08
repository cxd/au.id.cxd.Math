namespace au.id.cxd.MathOSXUI


open System
open System.Drawing
open MonoMac.Foundation
open MonoMac.AppKit
open au.id.cxd.Math.MathProject
open au.id.cxd.MathOSXUI.UIState
open au.id.cxd.MathOSXUI.OSXUIBuilder
open au.id.cxd.MathOSXUI.FileBrowser

module ProjectTree =
    /// <summary>
    /// A node used to represent items within the project
    /// </summary>
    type ProjectNode(name:string, act) =
        inherit NSObject()

        let nodeName = name

        let activity = act

        let mutable children = List.empty

        member p.Activity with get() = activity

        member p.Name with get() = name

        member p.Children with get() = children and set(lst) = children <- lst

        member p.Find(name:string) =
                   List.filter (fun (item:ProjectNode) -> item.Name.Equals(name)) (p.Children)
            
            
    /// <summary>
    /// The tree data source
    /// </summary>
    type ProjectTreeDataSource(root:ProjectNode) =
        inherit NSOutlineViewDataSource()

        let rootNode = root

        override d.GetChildrenCount( outlineView:NSOutlineView, item:NSObject) =
            match item with
                | :? ProjectNode ->
                    let node = (item :?> ProjectNode)
                    node.Children |> List.length
                | _ -> root.Children |> List.length
           
        override d.ItemExpandable(outlineView:NSOutlineView, item:NSObject) =
            (d.GetChildrenCount(outlineView, item) > 0)

        override d.GetChild(outlineView:NSOutlineView, childIndex, ofItem:NSObject) =
            logfmt "get child {0}" [|childIndex|]
            logfmt "child item {0}" [|ofItem|]
            match ofItem with
                | :? ProjectNode -> 
                    let node = (ofItem :?> ProjectNode)
                    let result = List.nth (node.Children) childIndex
                    
                    (result :> NSObject)
                | _ -> if ((rootNode.Children |> List.length > 0) && (rootNode.Children |> List.length > childIndex) ) then
                          let result = List.nth (rootNode.Children) childIndex
                          (result :> NSObject)
                       else null

        override d.GetObjectValue(tableView:NSOutlineView, tableColumn:NSTableColumn, item:NSObject) =
            logfmt "get object value {0}" [| item |]
            if (item = null) then
                (new NSString(rootNode.Name)) :> NSObject
            else
                let node = (item :?> ProjectNode)
                (new NSString(node.Name)) :> NSObject
                

    /// <summary>
    /// Delegate for the tree view.
    /// </summary>
    type ProjectTreeDelegate(state:ApplicationState, root:ProjectNode, view:NSOutlineView) =
        inherit NSOutlineViewDelegate()
        
        let outlineView  = view
        let rootNode = root
        let appState = state

        
        override d.ShouldEditTableColumn(outlineView:NSOutlineView, tableColumn:NSTableColumn, item:NSObject) =
            true

        override d.ShouldSelectItem (outlineView:NSOutlineView, item:NSObject) =
            log [|"ShouldSelectItem"|] 
            true

        override d.ShouldSelectTableColumn(outlineView:NSOutlineView, tableColumn:NSTableColumn) =
            true

        override d.SelectionShouldChange(outlineView:NSOutlineView) =
            true

        override d.SelectionDidChange(notificaton:NSNotification) =
            let indexes = outlineView.SelectedRows
            if (indexes.Count = 0) then ()
            let first = Convert.ToInt32(indexes.FirstIndex)
            let item = outlineView.ItemAtRow(first)
            logfmt "Selected {0}" [|item|]
            appState.RaisePerformActivity((item :?> ProjectNode).Activity)
            ()

        override d.GetCell(outlineView:NSOutlineView, tableColumn:NSTableColumn, item:NSObject) =
            logfmt "type [{0}]" [| item |]
            match item with
                | :? ProjectNode ->
                    let node = (item :?> ProjectNode)
                    logfmt "Node Name {0}" [| node.Name |]
                    let cell = new NSTextFieldCell(node.Name) :> NSCell
                    cell
                | :? NSString ->
                    let items = rootNode.Find(item.ToString())
                    if (List.length items > 0) then
                        let node = List.nth items 0
                        let cell = new NSTextFieldCell(node.Name) :> NSCell
                        cell
                    else null
                | _ -> null
        
    
    
    /// <summary>
    /// Build the file menu
    /// </summary>
    let projectTree (app:ApplicationState) =
        let outline = new NSOutlineView()
        outline.AutoresizesOutlineColumn <- true
        outline.AllowsMultipleSelection <- false
        outline.SelectionHighlightStyle <- NSTableViewSelectionHighlightStyle.Regular
        let root = new ProjectNode("Project", NoActivity)
        let datasource = new ProjectTreeDataSource(root)
        let outlineDelegate = new ProjectTreeDelegate(app, root, outline)
        outline.DataSource <- datasource
        outline.Delegate <- outlineDelegate
        app.ActivityChanged.Add(fun activity ->
                                    let name = activityName activity
                                    logfmt "Add Activity Name {0}" [| name |]  
                                    root.Children <- List.append [new ProjectNode(name, activity)] root.Children
                                    app.RaisePerformActivity(activity)
                                    outline.ReloadData()
                                    ())
        outline
        
        
        
        

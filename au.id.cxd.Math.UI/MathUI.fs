namespace au.id.cxd.Math.UI

open System
open System.Windows
open System.Windows.Controls
open System.Printing
open System.Windows.Documents
open System.Windows.Xps.Packaging
open System.IO
open System.IO.Packaging
open Microsoft.Win32
open au.id.cxd.Math.TextIO
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.UIBuilder.MathUIBuilder
open MathUIProject
open StateM
open UIApplication
open ChartUI
open DualChartUI
open MathUIAttributeGrid

module MathUI = 
    
    /// <summary>
    /// Present a prompt for a data source for the project.
    /// </summary>
    let promptForRawData w : ProjectActivity = 
        let dialog = new OpenFileDialog()
        dialog.Filter <- "CSV Files (*.csv)|*.csv"
        dialog.CheckFileExists <- true
        let result = dialog.ShowDialog()
        if (result.HasValue && result.Value) then
            LoadData ( { DataSource = dialog.FileName } )
        else NoActivity

    /// <summary>
    /// Save a project as a binary file.
    /// </summary>
    let promptToSaveProject w (app:UIState) =
        if (app.HasFile) then
            app.Save()
        else 
            let dialog = new SaveFileDialog()
            dialog.Filter <- "Data UI Project (*.dui)|*.dui"
            let result = dialog.ShowDialog()
            if (result.HasValue && result.Value) then
                app.Save(dialog.FileName)
            else ()

    /// <summary>
    /// Save a project as a binary file.
    /// </summary>
    let promptToOpenProject w (app:UIState) =
        let dialog = new OpenFileDialog()
        dialog.Filter <- "Data UI Project (*.dui)|*.dui"
        dialog.CheckFileExists <- true
        let result = dialog.ShowDialog()
        if (result.HasValue && result.Value) then
            app.Load(dialog.FileName)
        else ()

        /// <summary>
    /// Populate the data grid with data from the app data.
    /// </summary>
    let populateAppData (appState:UIState) (dataGrid:DataGrid) = 
        let data = convertToDataSet appState.Data
        dataGrid.DataContext <- data.Tables.[0]
        dataGrid.ItemsSource <- data.Tables.[0].DefaultView

    /// <summary>
    /// Populate the data grid with data from the csv file.
    /// </summary>
    let populateRawData (appState:UIState) file (dataGrid:DataGrid) = 
        appState.Data <- readFromCsv [|','|] file 
        populateAppData appState dataGrid

    
    /// <summary>
    /// populate tree from app state
    /// </summary>
    let populateTree (appState:UIState) (treeview:TreeView) handler =
        clear treeview
        List.iter(fun activity ->
                    let treeitem = new TreeViewItem(Tag = activity, Header = (activityName activity))
                    treeview.Items.Add(treeitem) |> ignore
                    handler activity treeitem) appState.Activities

    /// <summary>
    /// print the control
    /// </summary>
    let printUI (elem:UIElement) =
        let dialog = new PrintDialog()
        if dialog.ShowDialog() <> Nullable(true) then ()
        else 
            let queue = dialog.PrintQueue
            let writer = PrintQueue.CreateXpsDocumentWriter(queue)
            // to do scale the visual
            writer.Write(elem)
        ()

    /// <summary>
    /// print the control
    /// </summary>
    let printDoc (doc:FlowDocument) =
        let dialog = new PrintDialog()
        if dialog.ShowDialog() <> Nullable(true) then ()
        else 
            doc.PageHeight <- dialog.PrintableAreaHeight
            doc.PageWidth <- dialog.PrintableAreaWidth
            doc.PagePadding <- new Thickness(25.0)

            doc.ColumnGap <- 0.0

            doc.ColumnWidth <- (doc.PageWidth - 
                                   doc.ColumnGap - 
                                   doc.PagePadding.Left -  
                                   doc.PagePadding.Right)
            
            let queue = dialog.PrintQueue
            let writer = PrintQueue.CreateXpsDocumentWriter(queue)
            // to do scale the visual
            let elem = (doc :> IDocumentPaginatorSource).DocumentPaginator
            writer.Write(elem)
        ()

    /// <summary>
    /// Save XPS document
    /// </summary>
    let saveXps (doc:FlowDocument) = 
        let dialog = new SaveFileDialog()
        dialog.Filter <- "XML Paper (*.xps)|*.xps"
        let result = dialog.ShowDialog()
        if (result.HasValue && result.Value) then
            if (File.Exists(dialog.FileName)) then File.Delete(dialog.FileName)
            let xps = new XpsDocument(dialog.FileName, FileAccess.ReadWrite, CompressionOption.NotCompressed)
            let writer = XpsDocument.CreateXpsDocumentWriter(xps)
            writer.Write((doc :> IDocumentPaginatorSource).DocumentPaginator)
            xps.Close()
        else ()

    // build the main interface.                                    
    //val ui : State<UIApplication, unit> -> Window
    let ui (app:UIState) = 

        // the root window.
        let w = new Window(Title = "MathUI")

        let tabcontrol = new TabControl(VerticalAlignment = VerticalAlignment.Stretch)
        let reviewtab = new TabItem(Header = "Single Value Analysis")
        let bivaluetab = new TabItem(Header = "Dual Value Analysis")
        let multivaluetab = new TabItem(Header = "Multi Value Analysis")
        
        let attrtab = new TabItem(Header = "Define Attributes")
        let modeltab = new TabItem(Header = "Model Summary")

        // source data grid.
        let sourceDataGrid = new DataGrid()
        sourceDataGrid.HeadersVisibility <- DataGridHeadersVisibility.Column
        let sourcedataUI = childui sourceDataGrid
                                   nochange

        // a container for the list items available for selection in each column
        let attributePanel = new StackPanel()
        ScrollViewer.SetVerticalScrollBarVisibility(attributePanel, ScrollBarVisibility.Auto)   
        let attributePanelUI = childui attributePanel nochange

        // singular value analysis report.
        let graphReport = new FlowDocumentReader()


        let dualgraphviewer = new ScrollViewer()
        let dualgraphviewui = childui (dualgraphviewer) nochange
        


        // treeview used for data.
        let treeview = new TreeView()
        
        /// <summary>
        /// Assign an event handler to the tree view.
        /// <summary> 
        let assignTreeItemHandler activity (treeitem:TreeViewItem) =
            match activity with
            | LoadData data -> 
                ()
            | ReviewData columns -> 
                treeitem.Selected.AddHandler(fun snd evt ->
                                                 // draw the graph.
                                                 let report = chart app true
                                                 graphReport.Document <- report
                                                 tabcontrol.SelectedItem <- reviewtab)
            | DualReviewData columns ->
                treeitem.Selected.AddHandler(fun snd evt ->
                                                 dualchart app dualgraphviewer
                                                 tabcontrol.SelectedItem <- bivaluetab)
            | _ -> ()

        // the file menu items
        let filemenu = childui (new MenuItem(Header = "File")) (fun parent child ->
                                let filemenu = add child
                                
                                let fileopen = childui (new MenuItem(Header = "_Open")) 
                                                       (fun parent (child:UIElement) ->
                                                            (child :?> MenuItem)
                                                                .Click
                                                                .AddHandler(fun snder evt ->
                                                                                promptToOpenProject w app
                                                                                // populate the tree view
                                                                                populateTree app treeview assignTreeItemHandler
                                                                                // populate the data grid.
                                                                                populateAppData app sourceDataGrid
                                                                                // prepare the attribute panel.
                                                                                uicomponent attributePanel |> populateAttributeOptions app)
                                                            parent)
                                
                                let filesave = childui (new MenuItem(Header = "_Save")) 
                                                       (fun parent (child:UIElement) -> 
                                                              (child :?> MenuItem)
                                                                .Click
                                                                .AddHandler(fun snder evt ->
                                                                                promptToSaveProject w app)
                                                              parent) 
                                
                                let filesaveas = childui (new MenuItem(Header = "Save As")) 
                                                         (fun parent (child:UIElement) -> 
                                                              (child :?> MenuItem)
                                                                .Click
                                                                .AddHandler(fun snder evt ->
                                                                                promptToSaveProject w app)
                                                              parent)
                                
                                let fileclose = childui (new MenuItem(Header = "Close")) nochange

                                let fileprint = childui (new MenuItem(Header = "Print")) 
                                                        (fun parent (child:UIElement) ->
                                                             (child :?> MenuItem)
                                                                .Click
                                                                .AddHandler(fun sn evt -> 
                                                                             if (tabcontrol.SelectedItem = (reviewtab :> obj)) then
                                                                                let doc = graphReport.Document
                                                                                printDoc doc
                                                                                ()
                                                                             else ())

                                                             parent)

                                let filereload = childui (new MenuItem(Header = "_Reload")) 
                                                         (fun parent (child:UIElement) ->
                                                            (child :?> MenuItem)
                                                                .Click
                                                                .AddHandler(fun snder evt ->
                                                                                promptToOpenProject w app
                                                                                // populate the tree view
                                                                                populateTree app treeview assignTreeItemHandler
                                                                                // populate the data grid.
                                                                                populateAppData app sourceDataGrid
                                                                                // prepare the attribute panel.
                                                                                uicomponent attributePanel |> populateAttributeOptions app)
                                                            parent)
                                
                                let fileexit = childui (new MenuItem(Header = "E_xit")) 
                                                      (fun parent (child:UIElement) ->
                                                            (child :?> MenuItem).Click.Add (fun evt -> w.Close()
                                                                                                       app.Instance.Shutdown())
                                                            parent)
                               
                                // compose the menu.
                                (fileopen filemenu) |>
                                    (appendUI 
                                    >> filesave 
                                    >> appendUI 
                                    >> filesaveas 
                                    >> appendUI 
                                    >> fileclose
                                    >> appendUI
                                    >> fileprint 
                                    >> appendUI 
                                    >> filereload 
                                    >> appendUI 
                                    >> fileexit 
                                    >> appendUI) |> ignore
                                
                                parent)
        
        // edit menu items
        let editmenu = childui (new MenuItem(Header = "Edit")) (fun parent child ->
                                let parentmenu = add child
                                let copy = childui (new MenuItem(Header = "_Copy")) nochange 
                                let cut = childui (new MenuItem(Header = "_Cut")) 
                                                  (fun parent (child:UIElement) -> 
                                                       
                                                       parent)
                                let paste = childui (new MenuItem(Header = "Paste")) 
                                                    (fun parent (child:UIElement) -> 
                                                         
                                                         parent)
                                // compose the menu.
                                (copy parentmenu) |>
                                    (appendUI 
                                    >> cut 
                                    >> appendUI 
                                    >> paste 
                                    >> appendUI ) |> ignore
                                
                                parent)
        
        
        // wrapped tree
        let tree = childui treeview 
                           (fun parent (child:UIElement) ->
                                (child :?> TreeView).MinWidth <- 100.0
                                DockPanel.SetDock(child, Dock.Left)
                                parent)
        
        
        // construct the project menu and its actions.
        let projectmenu = childui (new MenuItem(Header = "Project")) 
                                  (fun parent child ->
                                        let parentmenu = add child
                                        
                                        // project menu
                                        let newproject = childui (new MenuItem(Header = "New Project"))
                                                                 (fun parent (child:UIElement) ->
                                                                      (child :?> MenuItem).Click.Add(
                                                                        (fun evt -> 
                                                                              app.ClearActivities()
                                                                              treeview.Items.Clear()
                                                                              treeview.UpdateLayout())
                                                                            )
                                                                      parent)
                                
                                        // activities menu.
                                        let activities = childui (new MenuItem(Header = "Activities")) 
                                                                 (fun parent child -> 
                                                                        let menuFn = add child

                                                                        // load raw data into the view
                                                                        let loaddata = childui (new MenuItem(Header = "Load Data"))
                                                                                               (fun parent (child:UIElement) ->
                                                                                                    (child :?> MenuItem).Click.Add(
                                                                                                        (fun evt ->
                                                                                                            let data = promptForRawData w
                                                                                                            app.AddActivity(data)
                                                                                                            let treeitem = new TreeViewItem(Tag = data, Header = (activityName data))
                                                                                                            treeview.Items.Add(treeitem) |> ignore
                                                                                                            // populate the data grid.
                                                                                                            populateRawData app (dataSource data) sourceDataGrid
                                                                                                            // prepare the attribute panel.
                                                                                                            uicomponent attributePanel |> populateAttributeOptions app
                                                                                                            ))
                                                                                                    parent)

                                                                        
                                                                        //  enable the user to review the data attributes.
                                                                        let reviewdata = childui (new MenuItem(Header = "Review Data"))
                                                                                                 (fun parent (child:UIElement) ->
                                                                                                      (child :?> MenuItem).Click.Add(
                                                                                                        fun evt ->
                                                                                                              let data = ReviewData List.empty
                                                                                                              app.AddActivity(data)
                                                                                                              let treeitem = new TreeViewItem(Tag = data, Header = (activityName data))
                                                                                                              treeview.Items.Add(treeitem) |> ignore
                                                                                                              assignTreeItemHandler data treeitem)
                                                                                                      parent)

                                                                        //  enable the user to review the data attributes.
                                                                        let dualdata = childui (new MenuItem(Header = "Review Two Variables"))
                                                                                                 (fun parent (child:UIElement) ->
                                                                                                      (child :?> MenuItem).Click.Add(
                                                                                                        fun evt ->
                                                                                                              let data = DualReviewData List.empty
                                                                                                              app.AddActivity(data)
                                                                                                              let treeitem = new TreeViewItem(Tag = data, Header = (activityName data))
                                                                                                              treeview.Items.Add(treeitem) |> ignore
                                                                                                              assignTreeItemHandler data treeitem)
                                                                                                      parent)

                                                                        // generate the actions
                                                                        (loaddata menuFn) |>
                                                                            (appendUI 
                                                                            >> reviewdata 
                                                                            >> appendUI 
                                                                            >> dualdata
                                                                            >> appendUI) |> ignore
                                                                        
                                                                        parent)

                                        // compose the menu.
                                        (newproject parentmenu) |>
                                            (appendUI 
                                            >> activities 
                                            >> appendUI) |> ignore
                                
                                        parent)


        // the menu bar that contains all drop down menus
        let menu = childui (new Menu())
                           (fun parent child ->
                                DockPanel.SetDock(child, Dock.Top)
                                let menuFn = add child
                                (filemenu menuFn) |> 
                                    (appendUI 
                                    >> editmenu 
                                    >> appendUI 
                                    >> projectmenu 
                                    >> appendUI) |> ignore

                                parent)
        
      
       


        // tab container for detail views
        let tab = childui (tabcontrol)
                                (fun parent child ->
                                        let intab = add child

                                        let testview = childui (new TabItem(Header = "Test Data")) nochange 
                                        let trainview = childui (new TabItem(Header = "Training Data")) nochange 
                                        let modelview = childui (modeltab) 
                                                                 nochange
                                        let datareview = childui (reviewtab)
                                                                 (fun parent child ->
                                                                      let addin = add child
                                                                      let reportUI = childui graphReport nochange
                                                                      appendUI (reportUI addin) |> ignore
                                                                      parent)

                                        let doubleview = childui (bivaluetab) 
                                                                 (fun parent child ->
                                                                    let addin = add child
                                                                    dualgraphviewui addin
                                                                    |> appendUI |> ignore
                                                                    parent)

                                        let multiview = childui (multivaluetab) nochange

                                        let configview = childui (attrtab) 
                                                                  (fun parent child -> 
                                                                       let addin = add child
                                                                       let scroller = childui (new ScrollViewer()) 
                                                                                              (fun parent (child:UIElement) -> 
                                                                                                    let addin' = add child
                                                                                                    appendUI (attributePanelUI addin') |> ignore
                                                                                                    parent)
                                                                       appendUI (scroller addin) |> ignore
                                                                       parent)
                                        let dataview = childui (new TabItem(Header = "Source Data")) 
                                                               (fun parent child ->
                                                                    let addin = add child
                                                                    appendUI (sourcedataUI addin) |> ignore
                                                                    parent)
                                            
                                        // example of function composition
                                        // (appendUI4 (dataview (appendUI3 (modelview (appendUI2 (trainview (appendUI1 (tab, nochange, testview)))))
                                            
                                        (dataview intab) |>
                                            (appendUI //1 
                                            >> configview
                                            >> appendUI
                                            >> datareview
                                            >> appendUI
                                            >> doubleview
                                            >> appendUI
                                            >> multiview
                                            >> appendUI
                                            >> trainview
                                            >> appendUI //2
                                            >> testview
                                            >> appendUI //3
                                            >> modelview 
                                            >> appendUI) |> ignore
                                            
                                        parent)

        /// build the dock for inner elements of the UI.
        let dock = uicomponent (new DockPanel(VerticalAlignment = VerticalAlignment.Stretch))
        let dock' = childui dock (fun parent child ->
                                        let dockFn = add child 
                                        
                                        (tree dockFn) |> 
                                            (appendUI 
                                            >> tab 
                                            >> appendUI) |> ignore
                                    
                                        parent)

        /// this is the external wrapper of the UI
        let outerpanel = uicomponent (new DockPanel(VerticalAlignment = VerticalAlignment.Stretch))
        let panel' = childui outerpanel (fun parent child ->
                                         let addto = add child
                                         menu addto |> 
                                            (appendUI 
                                            >> dock' 
                                            >> appendUI) |> ignore
                                         parent)

        // add the contents to the window.
        let addToWindow = add w

        // pack it all
        appendUI (panel' addToWindow) |> ignore
            
        // window
        w

namespace au.id.cxd.Math.UI

open System
open System.Windows
open System.Windows.Controls
open Microsoft.Win32
open au.id.cxd.Math.TextIO
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData
open au.id.cxd.Math.UIBuilder.MathUIBuilder
open MathUIProject
open StateM
open UIApplication

module MathUIAttributeGrid = 
    /// <summary>
    /// Populate the attribute stack panel
    /// each option is a label a text entry field and a combobox for each column in the data set 
    /// and a tickbox used to select the possible classification label.
    /// </summary>
    let rec populateAttributeOptions (appState:UIState) ui = 
        clear ui
        let addin = add ui
            
        let options = [(0,NumericOrdinal); (1,String); (2, Continuous); (3, Bool)]
        
        if (appState.Attributes.IsEmpty) then
            let attList = List.map (fun i -> { AttributeLabel = "Unnamed"; Column = i; AttributeType = String; }) [0..(appState.Data.Columns - 1)]
            appState.Attributes <- attList
        
        // assign a list of names to the application attributes.
        let assignAttrNames names =
            let attList = List.mapi (fun i n -> 
                                         let attr = attributeAt i appState.Attributes
                                         let attr' =
                                                    { AttributeLabel = n;
                                                      Column = i;
                                                      AttributeType = attr.AttributeType }
                                         attr') names
            appState.Attributes <- attList
        
        let addColumn (attr:Attribute) = 
            let gridelem = new Grid()
            let coldef1 = new ColumnDefinition(Width = new GridLength(150.0, GridUnitType.Auto))
            let coldef2 = new ColumnDefinition(Width = new GridLength(300.0, GridUnitType.Star))
            gridelem.ColumnDefinitions.Add(coldef1)
            gridelem.ColumnDefinitions.Add(coldef2)
            let row1 = new RowDefinition()
            let row2 = new RowDefinition()
            let row3 = new RowDefinition()
            let row4 = new RowDefinition()
            gridelem.RowDefinitions.Add(row1)
            gridelem.RowDefinitions.Add(row2)
            gridelem.RowDefinitions.Add(row3)
            gridelem.RowDefinitions.Add(row4)
           

            let grid = childui (gridelem)
                               (fun parent child -> 
                                    let addin = add child
                                    let label = childui (new Label(Content = "Column " + (attr.Column).ToString() + " named: ") :> UIElement) 
                                                        (fun (parent:UIElement) (child:UIElement) ->
                                                            Grid.SetColumn(child, 0)
                                                            Grid.SetRow(child, 0)
                                                            parent)
                                    let nametxt = childui (new TextBox(Text = attr.AttributeLabel)) 
                                                          (fun parent (child:UIElement) ->
                                                            Grid.SetColumn(child, 1)
                                                            Grid.SetRow(child, 0)
                                                            (child :?> TextBox).TextChanged.AddHandler(fun sender evt -> 
                                                                                                        let attr' = attributeAt attr.Column appState.Attributes
                                                                                                        appState.Attributes <-
                                                                                                            { AttributeLabel = (child :?> TextBox).Text; 
                                                                                                              Column = attr'.Column;
                                                                                                              AttributeType = attr'.AttributeType } |>
                                                                                                              setAttributeAt attr'.Column appState.Attributes
                                                                                                        ())
                                                            parent)
                                    // define a list of possible attribute types.
                                    let label2 = childui (new Label(Content = " is of kind ")) 
                                                         (fun (parent:UIElement) (child:UIElement) ->
                                                            Grid.SetColumn(child, 0)
                                                            Grid.SetRow(child, 1)
                                                            parent)
                                    let listoptions = childui (new ComboBox()) 
                                                              (fun parent (child:UIElement) ->
                                                                   Grid.SetColumn(child, 1)
                                                                   Grid.SetRow(child, 1)
                                                                   List.iter (fun option -> 
                                                                                let name = labelOfDefinition (snd option)
                                                                                let item = new ComboBoxItem(Content = name)
                                                                                item.Selected.AddHandler(fun sender evt ->
                                                                                                            let attr' = attributeAt attr.Column appState.Attributes
                                                                                                            appState.Attributes <-
                                                                                                                { AttributeLabel = attr'.AttributeLabel; 
                                                                                                                  Column = attr'.Column;
                                                                                                                  AttributeType = snd option } |>
                                                                                                                  setAttributeAt attr'.Column appState.Attributes
                                                                                                            ())
                                                                                (child :?> ComboBox).Items.Add(item) |> ignore
                                                                                if (labelOfDefinition attr.AttributeType).Equals(name) then
                                                                                   (child :?> ComboBox).SelectedItem  <- item
                                                                                ()) options
                                                                   parent)
                                    let label3 = childui (new Label(Content = " is class label ")) 
                                                         (fun (parent:UIElement) (child:UIElement) ->
                                                            Grid.SetColumn(child, 0)
                                                            Grid.SetRow(child, 2)
                                                            parent)
                                    let classoption = childui (new CheckBox())
                                                              (fun parent (child:UIElement) ->
                                                                Grid.SetColumn(child, 1)
                                                                Grid.SetRow(child, 2)
                                                                (child :?> CheckBox).Checked.AddHandler(fun snd evt -> appState.SetClassColumn(attr))
                                                                (child :?> CheckBox).Unchecked.AddHandler(fun snd evt -> appState.ClassColumn <- { Label = ""; Column = -1; })
                                                                if (attr.Column = appState.ClassColumn.Column) then (child :?> CheckBox).IsChecked <- new Nullable<bool>(true)
                                                                parent)

                                    let label4 = childui (new Label(Content = " ignore column "))
                                                         (fun parent (child:UIElement) ->
                                                              Grid.SetColumn(child, 0)
                                                              Grid.SetRow(child, 3)
                                                              parent)

                                    let ignoreoption = childui (new CheckBox())
                                                               (fun parent (child:UIElement) ->
                                                                    Grid.SetColumn(child, 1)
                                                                    Grid.SetRow(child, 3)
                                                                    
                                                                    if (appState.IsIgnored(attr.Column)) then
                                                                        (child :?> CheckBox).IsChecked <- new Nullable<bool>(true)
                                                                    
                                                                    (child :?> CheckBox).Checked.AddHandler(fun sndr evt -> 
                                                                                                            appState.AddIgnoreColumn(attr.Column))
                                                                    (child :?> CheckBox).Unchecked.AddHandler(fun sndr evt -> 
                                                                                                                appState.RemoveIgnoreColumn(attr.Column))
                                                                    parent)
                                    
                                    label addin |>
                                       (appendUI
                                        >> nametxt
                                        >> appendUI
                                        >> label2
                                        >> appendUI
                                        >> listoptions
                                        >> appendUI
                                        >> label3
                                        >> appendUI
                                        >> classoption
                                        >> appendUI
                                        >> label4
                                        >> appendUI
                                        >> ignoreoption
                                        >> appendUI) |> ignore
                                    parent)
            appendUI (grid addin) |> ignore            
        
        let grid = new Grid()
        let gridelem = new Grid()
        let coldef1 = new ColumnDefinition(Width = new GridLength(150.0, GridUnitType.Auto))
        let coldef2 = new ColumnDefinition(Width = new GridLength(300.0, GridUnitType.Star))
        gridelem.ColumnDefinitions.Add(coldef1)
        gridelem.ColumnDefinitions.Add(coldef2)
        let row1 = new RowDefinition()
        gridelem.RowDefinitions.Add(row1)
        let uielem = childui (gridelem)
                             (fun parent child ->
                                let label = childui (new Label(Content = "First Row of Data Contains Column Names"))
                                                    nochange
                                let check = childui (new CheckBox())
                                                    (fun parent (child:UIElement) ->
                                                        if (appState.DataWithColumnHeaders) then
                                                           (child :?> CheckBox).IsChecked <- new Nullable<bool>(true)
                                                                    
                                                        (child :?> CheckBox).Checked.AddHandler(fun sndr evt -> 
                                                                                                            let firstRow = appState.Data.RawData |> Seq.head
                                                                                                            assignAttrNames firstRow
                                                                                                            appState.DataWithColumnHeaders <- true
                                                                                                            populateAttributeOptions (appState:UIState) ui)
                                                        (child :?> CheckBox).Unchecked.AddHandler(fun sndr evt -> 
                                                                                                            appState.DataWithColumnHeaders <- false)
                                                                    
                                                        parent)
                                                    
                                label addin
                                |> (appendUI
                                    >> check
                                    >> appendUI)
                                |> ignore                      
                                
                                parent) 
        
        appendUI (uielem addin) |> ignore

        List.iter addColumn appState.Attributes

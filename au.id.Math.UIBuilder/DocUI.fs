namespace au.id.cxd.Math.UIBuilder

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Documents
open MathUIBuilder


/// <summary>
/// A module to support the construction of tables
/// </summary>
module DocUI =

    /// <summary>
    /// Make a paragraph element childDoc function.
    /// Supply the font and text size and content to add
    /// to the paragraph.
    /// </summary>
    let para font sz content =
        childDoc (new Paragraph(FontFamily = new Media.FontFamily(font), FontSize = sz) :> ContentElement)
                 (fun parent (child:ContentElement) ->
                    List.iter (fun item ->  
                                item (addDoc child) |> appendUI |> ignore)
                              content
                    parent)
      
    /// <summary>
    /// Make a paragraph appendUI function 
    /// with a font family a size and a text
    /// </summary>             
    let textPara font sz (txt:string) =
        childDoc (new Paragraph(FontFamily = new Media.FontFamily(font), FontSize = sz))
                 (fun parent (child:ContentElement) ->
                      let p = (child :?> Paragraph)
                      p.Inlines.Add(txt)
                      parent)  
    /// <summary>
    /// Define a bold string of text
    /// </summary>
    let textBold (txt:string) =
        childDoc (new Bold() :> ContentElement)
                 (fun parent (child:ContentElement) ->
                      let b = child :?> Bold
                      b.Inlines.Add(txt)
                      parent)

    /// <summary>
    /// Make a bordered table.
    /// </summary>
    let tableBordered cellspace borderbrush borderwidth pad rowContent colContent =
        childDoc (new Table(CellSpacing = cellspace, 
                            BorderBrush = borderbrush, 
                            BorderThickness = new Thickness(borderwidth),
                            Padding = new Thickness(pad)) :> ContentElement)
                 (fun parent (child:ContentElement) ->
                        rowContent (addDoc child) |> appendUI |> ignore
                        parent)
    
    /// <summary>
    /// Make a table.
    /// </summary>
    let table cellspace pad rowContent colContent =
        childDoc (new Table(CellSpacing = cellspace,
                            Padding = new Thickness(pad)) :> ContentElement)
                 (fun parent (child:ContentElement) ->
                        let t = (child :?> Table)
                        List.iter (fun col ->
                                    col (addDoc child) |> appendUI |> ignore)
                                  colContent
                        rowContent (addDoc child) |> appendUI |> ignore
                        parent)
    /// <summary>
    /// Make a bordered cell
    /// </summary>
    let cellBordered brush width pad content =
        childDoc (new TableCell(BorderBrush = brush, 
                                BorderThickness = new Thickness(width),
                                Padding = new Thickness(pad)))
                 (fun parent (child:ContentElement) ->
                    content (addDoc child) |> appendUI |> ignore
                    parent)
        
        
    /// <summary>
    /// Make an unbordered cell
    /// </summary>
    let cell pad content =
        childDoc (new TableCell(Padding = new Thickness(pad)))
                 (fun parent (child:ContentElement) ->
                    content (addDoc child) |> appendUI |> ignore
                    parent)
    
    /// <summary>
    /// Make a row with a list of cells.
    /// </summary>                
    let row content =
        childDoc (new TableRow())
                 (fun parent child ->
                      List.iter (fun cell ->
                                    cell (addDoc child) |> appendUI |> ignore)
                                content
                      parent)

   
    /// <summary>
    /// Make a shaded row with a list of cells.
    /// </summary>                
    let rowShaded shade content =
        childDoc (new TableRow(Background = shade))
                 (fun parent child ->
                      List.iter (fun cell ->
                                    cell (addDoc child) |> appendUI |> ignore)
                                content
                      parent)
    /// <summary>
    /// Add all rows to the row group
    /// </summary>
    let rowGroup content =
        childDoc (new TableRowGroup() :> ContentElement)
                 (fun (parent:ContentElement) child ->
                      List.iter (fun row ->
                                    row (addDoc child) |> appendUI |> ignore)
                                content
                      parent)
    ()


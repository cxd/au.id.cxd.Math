namespace au.id.cxd.Math.UI

open System
open System.IO
open System.Windows
open System.Windows.Controls
open System.Windows.Markup

open StateM
open MathUI
open UIApplication


module Program = 
    
    [<STAThread>]
    [<EntryPoint>]
    let main(_) =
        //let app = Application.LoadComponent(new System.Uri("/au.id.cxd.Math.UI;component/Application.xaml", UriKind.Relative)) :?> Application
        let app = new Application()
        let st = new UIState(app)
        app.Run(ui st)
        


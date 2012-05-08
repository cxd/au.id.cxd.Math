namespace au.id.cxd.Math.UI 

    open System
    open System.Windows

    type MainWindow() as this = 
        
        inherit Window()

        member this.OnExit((sender:Object), (evt:RoutedEventArgs)) = Application.Current.Shutdown()
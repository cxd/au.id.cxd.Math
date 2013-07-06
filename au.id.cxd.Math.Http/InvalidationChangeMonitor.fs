namespace au.id.cxd.Math.Http

open System
open System.Runtime.Caching

type InvalidationChangeMonitor(id:Guid) =
    inherit ChangeMonitor() do base.InitializationComplete()
     
    override m.UniqueId with get() = id.ToString()
    
    override m.Dispose(disposing:bool) = 
        ()
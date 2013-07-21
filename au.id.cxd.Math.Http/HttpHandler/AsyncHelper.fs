namespace au.id.cxd.Math.Http

open System
open System.Web

module AsyncHelper =

     let runAsync(context:HttpContext, fn:(HttpContext -> unit)) = async { return fn(context) }
        
     let beginAction, endAction, _ = Async.AsBeginEnd(runAsync)
        
     let processRequest(context:HttpContext) = ()
     
     let writeJsonToContext (context:HttpContext) (item:string) =
        context.Response.ContentType <- "application/json"
        context.Response.StatusCode <- 200 
        context.Response.Write(item)
        context.Response.Flush()
        
     // write string to context.
     let writeStringToContext (context:HttpContext) (mediaType:string) (item:string) =
        context.Response.ContentType <- mediaType
        context.Response.StatusCode <- 200
        context.Response.Write(item)
        context.Response.Flush()
      

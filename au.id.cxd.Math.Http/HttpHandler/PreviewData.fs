namespace au.id.cxd.Math.Http

open System
open System.IO
open System.Web
open Handler
open AsyncHelper
open Json
open CookieHelper
open Filesystem
open DataState
open Newtonsoft.Json.Linq


module PreviewData =

    /// list the names of existing projects
    let processRequest(context:HttpContext) =
        let data = DataState.readPagedFile 0 100
        match data with
        | Some (descriptor, rawData) ->
            let dataJson = 
                if (descriptor.IncludesHeader) then
                    let headers = Seq.take 1 rawData 
                                    |> Seq.toList 
                                    |> List.head
                    let data = Seq.skip 1 rawData
                    Json.dataSequenceToJson headers data
                 else
                    let unknownHeaders (cnt, accum) item = (cnt+1, String.Format("Untitled{0}", cnt)::accum) 
                    let headers = Seq.take 1 rawData 
                                    |> Seq.toList 
                                    |> List.head 
                                    |> List.fold unknownHeaders (1, []) 
                                    |> (fun (a, accum) -> List.rev accum)
                    Json.dataSequenceToJson headers rawData
            dataJson :> JToken 
            |> Json.makeSuccessObject "records" 
            |> toString 
            |> AsyncHelper.writeJsonToContext context
        | None ->
            Json.makeError "No data available" 
            |> toString 
            |> AsyncHelper.writeJsonToContext context
            
/// list projects
type PreviewDataHandler() =
    inherit HandlerInstance(PreviewData.processRequest)

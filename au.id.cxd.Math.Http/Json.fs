namespace au.id.cxd.Math.Http

open System
open Newtonsoft.Json.Linq
open au.id.cxd.Math.MathProject
open au.id.cxd.Math.RawData
open au.id.cxd.Math.TrainingData


module Json =

    let makeSuccess (message:string) =
        JObject(
            JProperty("message", message),
            JProperty("status", true)
        )

    let makeSuccessObject (propName:string) (json:JToken) =
        JObject(
            JProperty("success", true),
            JProperty(propName, json)
        )


    let makeError (message:string) = 
        JObject(
            JProperty("message", message),
            JProperty("status", false)
        )
        
    let toString (item:JToken) = item.ToString()
    
    /// convert a list of items to a json array
    let toJsonArray (items:'a list) (convertFn:'a -> JToken) =
        List.map convertFn items
        |> List.toArray
        |> (fun jitems -> JArray(jitems))
        
    /// convert from a string to json
    let stringToJson (item:string) = JValue(item) :> JToken
    
    /// convert a string list to json
    let stringListToJson (items:string list) = toJsonArray items stringToJson
    
    
    /// convert an attribute to json
    let attributeToJson (attribute:Attribute) =
        JObject(
           JProperty("label", attribute.AttributeLabel),
           JProperty("type", labelOfDefinition attribute.AttributeType),
           JProperty("column", attribute.Column.ToString())
        ) :> JToken
    
    /// convert from json to an attribute.  
    let jsonToAttribute (json:JObject) =
        { AttributeLabel = json.GetValue("label").ToString();
          AttributeType = labelToDefinition (json.GetValue("type").ToString());
          Column = Convert.ToInt32(json.GetValue("column").ToString()); }
          
    /// convert a list of attributes to json objects
    let attributeListToJson (attributes:Attribute list) =
        toJsonArray attributes attributeToJson
        
    /// convert from a jarray to a list of attributes
    let jsonArrayToAttributeList (json:JArray) =
        Seq.map jsonToAttribute (seq { for child in json do yield (child :?> JObject) })
        |> Seq.toList
    
    /// convert a class attribute to json    
    let classAttributeToJson (clsAttribute:ClassificationAttribute) =
        JObject(
            JProperty("label", clsAttribute.Label),
            JProperty("column", clsAttribute.Column)
        )
        

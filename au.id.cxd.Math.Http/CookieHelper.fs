namespace au.id.cxd.Math.Http

open System
open System.Web

module CookieHelper =

    
    let setCookie (context:HttpContext) (name:string) (value:string) =
        let cookie = new HttpCookie(name)
        cookie.Value <- value
        context.Response.AppendCookie(cookie)

    let expireCookie (context:HttpContext) (name:string) =
        let cookie = new HttpCookie(name)
        cookie.Expires <- DateTime.Now.AddDays(-1.0)
        context.Response.AppendCookie(cookie)
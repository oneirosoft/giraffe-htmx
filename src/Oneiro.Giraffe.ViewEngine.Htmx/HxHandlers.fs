/// <summary>
/// Auto-opened module providing HTTP handlers optimized for HTMX applications.
/// Includes handlers that automatically adapt response based on whether request is from HTMX or full page load.
/// </summary>
[<AutoOpen>]
module Giraffe.ViewEngine.Htmx.Handlers

open Microsoft.AspNetCore.Http
open Giraffe
open Giraffe.ViewEngine

/// <summary>
/// Creates an HTTP handler that adapts its response based on HTMX request context.
/// For HTMX requests, returns only the content fragment.
/// For full page requests, wraps content in the provided layout.
/// </summary>
/// <param name="layout">Layout function that wraps content in full HTML document</param>
/// <param name="content">Function that generates the content to display</param>
/// <returns>HTTP handler that adapts response based on request type</returns>
/// <example>
/// <code>
/// let handler = htmx myLayout (fun () -> 
///     div [] [str "This content adapts to HTMX vs full page requests"]
/// )
/// </code>
/// </example>
let htmx (layout: (XmlNode list -> XmlNode)) (content: unit -> XmlNode): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let content = content()
        if ctx.IsHtmxRequest() then htmlView content next ctx
        else htmlView (layout [content]) next ctx


/// <summary>
/// Auto-opened module providing HttpContext extension methods for HTMX request introspection.
/// These extensions allow server-side code to access HTMX-specific request headers and context.
/// </summary>
[<AutoOpen>]
module Giraffe.ViewEngine.Htmx.HttpContextExtensions

open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Http

/// <summary>
/// Extension methods for HttpContext to access HTMX-specific information from request headers.
/// HTMX automatically sends various headers with requests that provide context about the client state.
/// </summary>
[<Extension>]
type HttpContextHtmxExtensions =
    
    /// <summary>
    /// Checks if the current request is an HTMX request by looking for the HX-Request header.
    /// This is the primary method to determine if a request came from HTMX vs a full page load.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>True if request came from HTMX, false otherwise</returns>
    [<Extension>]
    static member IsHtmxRequest(ctx: HttpContext) : bool =
        ctx.Request.Headers.ContainsKey("HX-Request")
    
    /// <summary>
    /// Gets the HTMX target element ID that will receive the response content.
    /// This corresponds to the hx-target attribute or the triggering element itself.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>Some target ID if header present, None otherwise</returns>
    [<Extension>]
    static member HtmxTarget(ctx: HttpContext) : string option =
        match ctx.Request.Headers.TryGetValue("HX-Target") with
        | true, value -> Some(value.ToString())
        | false, _ -> None
    
    /// <summary>
    /// Gets the HTMX trigger element ID that initiated the request.
    /// This is the ID of the element that triggered the HTMX request.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>Some trigger element ID if header present, None otherwise</returns>
    [<Extension>]
    static member HtmxTrigger(ctx: HttpContext) : string option =
        match ctx.Request.Headers.TryGetValue("HX-Trigger") with
        | true, value -> Some(value.ToString())
        | false, _ -> None
    
    /// <summary>
    /// Gets the HTMX trigger name (name attribute) of the element that initiated the request.
    /// Useful for form elements where you need to know which specific input triggered the request.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>Some trigger name if header present, None otherwise</returns>
    [<Extension>]
    static member HtmxTriggerName(ctx: HttpContext) : string option =
        match ctx.Request.Headers.TryGetValue("HX-Trigger-Name") with
        | true, value -> Some(value.ToString())
        | false, _ -> None
    
    /// <summary>
    /// Gets the current URL from the client-side perspective.
    /// This is the URL that was in the browser address bar when the HTMX request was made.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>Some current URL if header present, None otherwise</returns>
    [<Extension>]
    static member HtmxCurrentUrl(ctx: HttpContext) : string option =
        match ctx.Request.Headers.TryGetValue("HX-Current-URL") with
        | true, value -> Some(value.ToString())
        | false, _ -> None
    
    /// <summary>
    /// Gets the user's response to an HTMX prompt dialog.
    /// This is only present when hx-prompt is used and the user enters a value.
    /// </summary>
    /// <param name="ctx">HttpContext to examine</param>
    /// <returns>Some prompt response if header present, None otherwise</returns>
    [<Extension>]
    static member HtmxPrompt(ctx: HttpContext) : string option =
        match ctx.Request.Headers.TryGetValue("HX-Prompt") with
        | true, value -> Some(value.ToString())
        | false, _ -> None

    /// <summary>
    /// Sets an HTMX-specific response header to control client-side behavior.
    /// This is a low-level method for setting any HTMX response header.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="headerName">Name of the HTMX header (e.g., "HX-Redirect")</param>
    /// <param name="value">Value for the header</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxHeader(ctx: HttpContext, headerName: string, value: string) : HttpContext =
        ctx.Response.Headers.Add(headerName, value)
        ctx
    
    /// <summary>
    /// Sets the HX-Redirect header to trigger a client-side redirect.
    /// The browser will navigate to the specified URL, abandoning the current page.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="url">URL to redirect to</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxRedirect(ctx: HttpContext, url: string) : HttpContext =
        ctx.SetHtmxHeader("HX-Redirect", url)
    
    /// <summary>
    /// Sets the HX-Push-Url header to update the browser URL without a full page load.
    /// This updates the address bar and browser history.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="url">URL to push to browser history</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxPushUrl(ctx: HttpContext, url: string) : HttpContext =
        ctx.SetHtmxHeader("HX-Push-Url", url)
    
    /// <summary>
    /// Sets the HX-Replace-Url header to replace the current URL in browser history.
    /// This updates the address bar without adding a new history entry.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="url">URL to replace in browser history</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxReplaceUrl(ctx: HttpContext, url: string) : HttpContext =
        ctx.SetHtmxHeader("HX-Replace-Url", url)
    
    /// <summary>
    /// Sets the HX-Refresh header to trigger a full page refresh.
    /// The browser will reload the current page completely.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxRefresh(ctx: HttpContext) : HttpContext =
        ctx.SetHtmxHeader("HX-Refresh", "true")
    
    /// <summary>
    /// Sets the HX-Trigger header to fire custom client-side events.
    /// This allows server responses to trigger JavaScript events on the client.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="eventName">Name of the event to trigger</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxTrigger(ctx: HttpContext, eventName: string) : HttpContext =
        ctx.SetHtmxHeader("HX-Trigger", eventName)
    
    /// <summary>
    /// Sets the HX-Trigger header with event data in JSON format.
    /// This allows triggering client-side events with additional payload data.
    /// </summary>
    /// <param name="ctx">HttpContext to modify</param>
    /// <param name="eventData">JSON string containing event name and data</param>
    /// <returns>The modified HttpContext for chaining</returns>
    [<Extension>]
    static member SetHtmxTriggerWithData(ctx: HttpContext, eventData: string) : HttpContext =
        ctx.SetHtmxHeader("HX-Trigger", eventData)
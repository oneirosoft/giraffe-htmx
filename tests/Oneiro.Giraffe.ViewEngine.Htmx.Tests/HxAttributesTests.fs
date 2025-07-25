module Oneiro.Giraffe.ViewEngine.Htmx.Tests.HtmxAttributesTests

open Xunit
open FsUnit.Xunit
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open System.Net

// Helper function to extract attribute values from XML nodes
let getAttrValue (node: XmlNode) (attrName: string) =
    // In Giraffe.ViewEngine, the node is rendered to a string and then parsed
    // to check attributes since XmlNode doesn't expose attributes directly
    let nodeStr = RenderView.AsString.htmlNode node
    
    // Simple check to see if the attribute exists in the rendered HTML
    nodeStr.Contains(sprintf "%s=\"" attrName) || nodeStr.Contains(sprintf "%s='" attrName)

// Helper to get the attribute value from the rendered node
let getAttrValueContent (node: XmlNode) (attrName: string) =
    let nodeStr = RenderView.AsString.htmlNode node
    
    // Basic parsing to extract attribute value using regex - check for both double and single quotes
    let doubleQuotePattern = sprintf "%s=\"([^\"]+)\"" attrName
    let singleQuotePattern = sprintf "%s='([^']+)'" attrName
    
    let doubleQuoteMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, doubleQuotePattern)
    let singleQuoteMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, singleQuotePattern)
    
    if doubleQuoteMatch.Success && doubleQuoteMatch.Groups.Count > 1 then
        Some doubleQuoteMatch.Groups.[1].Value
    elif singleQuoteMatch.Success && singleQuoteMatch.Groups.Count > 1 then
        Some singleQuoteMatch.Groups.[1].Value
    else
        // Also check for empty string values
        let emptyPattern = sprintf "%s=\"\"" attrName
        let emptyMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, emptyPattern)
        if emptyMatch.Success then
            Some ""
        else
            None

// Helper to decode HTML entities
let decodeHtml (html: string) =
    WebUtility.HtmlDecode(html)

[<Fact>]
let ``HtmxAttributes apply correct attribute names`` () =
    let div = div [_hxGet "/api/data"] []
    
    // Check if the hx-get attribute exists and has the correct value
    getAttrValue div "hx-get" |> should be True
    getAttrValueContent div "hx-get" |> should equal (Some "/api/data")

[<Theory>]
[<InlineData("hx-get", "/api/data")>]
[<InlineData("hx-post", "/api/submit")>]
[<InlineData("hx-put", "/api/update")>]
[<InlineData("hx-delete", "/api/remove")>]
[<InlineData("hx-patch", "/api/patch")>]
let ``Basic HTMX attributes have correct name and value`` (attrName: string, url: string) =
    let attrs =
        match attrName with
        | "hx-get" -> [_hxGet url]
        | "hx-post" -> [_hxPost url]
        | "hx-put" -> [_hxPut url]
        | "hx-delete" -> [_hxDelete url]
        | "hx-patch" -> [_hxPatch url]
        | _ -> failwith "Invalid attribute name"
        
    let div = div attrs []
    
    getAttrValue div attrName |> should be True
    getAttrValueContent div attrName |> should equal (Some url)

[<Theory>]
[<InlineData("click")>]
[<InlineData("mouseover")>]
[<InlineData("load")>]
[<InlineData("click once")>]
[<InlineData("keyup[key=='Enter']")>]
let ``_hxTrigger attribute should have correct value`` (triggerValue: string) =
    let div = div [_hxTrigger triggerValue] []
    
    getAttrValue div "hx-trigger" |> should be True
    
    // Get the actual value and decode any HTML entities
    let actualValue = getAttrValueContent div "hx-trigger"
    match actualValue with
    | Some value -> 
        let decoded = decodeHtml(value)
        decoded |> should equal triggerValue
    | None -> 
        failwith "Expected attribute value but got None"

[<Fact>]
let ``Boolean attributes should have empty string value`` () =
    let div = div [_hxDisable; _hxOob] []
    
    // For boolean attributes, we check that the attribute exists
    getAttrValue div "hx-disable" |> should be True
    getAttrValue div "hx-oob" |> should be True
    
    // And that they have empty string values
    getAttrValueContent div "hx-disable" |> should equal (Some "")
    getAttrValueContent div "hx-oob" |> should equal (Some "")

[<Fact>]
let ``Multiple HTMX attributes can be combined`` () =
    let button = 
        button [
            _hxGet "/api/data"
            _hxTarget "#result"
            _hxSwap "outerHTML"
            _hxTrigger "click"
            _class "btn" // Non-HTMX attribute
        ] [str "Click me"]
    
    // Check that all attributes exist
    getAttrValue button "hx-get" |> should be True
    getAttrValue button "hx-target" |> should be True
    getAttrValue button "hx-swap" |> should be True
    getAttrValue button "hx-trigger" |> should be True
    getAttrValue button "class" |> should be True
    
    // Check their values
    getAttrValueContent button "hx-get" |> should equal (Some "/api/data")
    getAttrValueContent button "hx-target" |> should equal (Some "#result")
    getAttrValueContent button "hx-swap" |> should equal (Some "outerHTML")
    getAttrValueContent button "hx-trigger" |> should equal (Some "click")
    getAttrValueContent button "class" |> should equal (Some "btn")
    
[<Fact>]
let ``_hxOn attribute should format properly`` () =
    let button = button [_hxOn ("click", "alert('clicked')")] []
    
    getAttrValue button "hx-on:click" |> should be True
    
    // Get the actual value and decode any HTML entities
    let actualValue = getAttrValueContent button "hx-on:click"
    match actualValue with
    | Some value -> 
        let decoded = decodeHtml(value)
        decoded |> should equal "alert('clicked')"
    | None -> 
        failwith "Expected attribute value but got None"

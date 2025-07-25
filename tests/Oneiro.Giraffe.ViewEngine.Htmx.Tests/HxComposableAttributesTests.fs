module Oneiro.Giraffe.ViewEngine.Htmx.Tests.HtmxComposableAttributesTests

open Xunit
open FsUnit.Xunit
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open System.Net

// Helper functions to extract attribute values from XML nodes
let getAttrValue (node: XmlNode) (attrName: string) =
    let nodeStr = RenderView.AsString.htmlNode node
    nodeStr.Contains(sprintf "%s=\"" attrName) || nodeStr.Contains(sprintf "%s='" attrName)

let getAttrValueContent (node: XmlNode) (attrName: string) =
    let nodeStr = RenderView.AsString.htmlNode node
    
    let doubleQuotePattern = sprintf "%s=\"([^\"]+)\"" attrName
    let singleQuotePattern = sprintf "%s='([^']+)'" attrName
    
    let doubleQuoteMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, doubleQuotePattern)
    let singleQuoteMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, singleQuotePattern)
    
    if doubleQuoteMatch.Success && doubleQuoteMatch.Groups.Count > 1 then
        Some doubleQuoteMatch.Groups.[1].Value
    elif singleQuoteMatch.Success && singleQuoteMatch.Groups.Count > 1 then
        Some singleQuoteMatch.Groups.[1].Value
    else
        let emptyPattern = sprintf "%s=\"\"" attrName
        let emptyMatch = System.Text.RegularExpressions.Regex.Match(nodeStr, emptyPattern)
        if emptyMatch.Success then
            Some ""
        else
            None

let decodeHtml (html: string) = WebUtility.HtmlDecode(html)

// Tests for HtmxTrigger types and conversions
[<Theory>]
[<InlineData("Click", "click")>]
[<InlineData("Submit", "submit")>]
[<InlineData("Change", "change")>]
[<InlineData("KeyUp", "keyup")>]
[<InlineData("KeyDown", "keydown")>]
[<InlineData("MouseOver", "mouseover")>]
[<InlineData("MouseOut", "mouseout")>]
[<InlineData("Load", "load")>]
let ``Basic HtmxTrigger types should convert to correct strings`` (triggerName: string, expectedValue: string) =
    let trigger = 
        match triggerName with
        | "Click" -> Click
        | "Submit" -> Submit
        | "Change" -> Change
        | "KeyUp" -> KeyUp
        | "KeyDown" -> KeyDown
        | "MouseOver" -> MouseOver
        | "MouseOut" -> MouseOut
        | "Load" -> Load
        | _ -> failwith $"Unknown trigger: {triggerName}"
    
    let result = triggerToString trigger
    result |> should equal expectedValue

[<Fact>]
let ``HtmxTrigger Custom should return custom string`` () =
    let trigger = HtmxTrigger.Custom "my-custom-event"
    let result = triggerToString trigger
    result |> should equal "my-custom-event"

[<Fact>]
let ``HtmxTrigger KeyboardFilter should format correctly`` () =
    let trigger = KeyboardFilter(KeyUp, "key=='Enter'")
    let result = triggerToString trigger
    result |> should equal "keyup[key=='Enter']"

[<Fact>]
let ``HtmxTrigger Once should format correctly`` () =
    let trigger = Once(Click)
    let result = triggerToString trigger
    result |> should equal "click once"

[<Fact>]
let ``HtmxTrigger Delay should format correctly`` () =
    let trigger = Delay(KeyUp, 300)
    let result = triggerToString trigger
    result |> should equal "keyup delay:300ms"

[<Fact>]
let ``HtmxTrigger Throttle should format correctly`` () =
    let trigger = Throttle(Change, 500)
    let result = triggerToString trigger
    result |> should equal "change throttle:500ms"

[<Fact>]
let ``HtmxTrigger From should format correctly`` () =
    let trigger = From(Click, "#submit-btn")
    let result = triggerToString trigger
    result |> should equal "click from:#submit-btn"

[<Fact>]
let ``Complex HtmxTrigger combinations should format correctly`` () =
    let trigger = KeyboardFilter(KeyUp, "key=='Enter'") |> Once
    let result = triggerToString trigger
    result |> should equal "keyup[key=='Enter'] once"

// Tests for HtmxSwap types and conversions
[<Theory>]
[<InlineData("InnerHTML", "innerHTML")>]
[<InlineData("OuterHTML", "outerHTML")>]
[<InlineData("BeforeBegin", "beforebegin")>]
[<InlineData("AfterBegin", "afterbegin")>]
[<InlineData("BeforeEnd", "beforeend")>]
[<InlineData("AfterEnd", "afterend")>]
[<InlineData("Delete", "delete")>]
[<InlineData("NoSwap", "none")>]
let ``Basic HtmxSwap types should convert to correct strings`` (swapName: string, expectedValue: string) =
    let swap = 
        match swapName with
        | "InnerHTML" -> InnerHTML
        | "OuterHTML" -> OuterHTML
        | "BeforeBegin" -> BeforeBegin
        | "AfterBegin" -> AfterBegin
        | "BeforeEnd" -> BeforeEnd
        | "AfterEnd" -> AfterEnd
        | "Delete" -> Delete
        | "NoSwap" -> NoSwap
        | _ -> failwith $"Unknown swap: {swapName}"
    
    let result = swapToString swap
    result |> should equal expectedValue

[<Fact>]
let ``HtmxSwap Custom should return custom string`` () =
    let swap = HtmxSwap.Custom "my-custom-swap"
    let result = swapToString swap
    result |> should equal "my-custom-swap"

[<Fact>]
let ``HtmxSwap WithModifiers should format transition correctly`` () =
    let swap = WithModifiers(InnerHTML, [Transition true])
    let result = swapToString swap
    result |> should equal "innerHTML transition:true"

[<Fact>]
let ``HtmxSwap WithModifiers should format swap delay correctly`` () =
    let swap = WithModifiers(OuterHTML, [HtmxSwapModifier.Swap 100])
    let result = swapToString swap
    result |> should equal "outerHTML swap:100ms"

[<Fact>]
let ``HtmxSwap WithModifiers should format settle delay correctly`` () =
    let swap = WithModifiers(InnerHTML, [Settle 200])
    let result = swapToString swap
    result |> should equal "innerHTML settle:200ms"

[<Fact>]
let ``HtmxSwap WithModifiers should format scroll correctly`` () =
    let swap = WithModifiers(InnerHTML, [Scroll "top"])
    let result = swapToString swap
    result |> should equal "innerHTML scroll:top"

[<Fact>]
let ``HtmxSwap WithModifiers should format show correctly`` () =
    let swap = WithModifiers(InnerHTML, [Show "#result"])
    let result = swapToString swap
    result |> should equal "innerHTML show:#result"

[<Fact>]
let ``HtmxSwap WithModifiers should format multiple modifiers correctly`` () =
    let swap = WithModifiers(OuterHTML, [
        Transition true
        HtmxSwapModifier.Swap 100
        Settle 200
        Scroll "top"
    ])
    let result = swapToString swap
    result |> should equal "outerHTML transition:true swap:100ms settle:200ms scroll:top"

// Tests for _hxTriggerTyped attribute generation
[<Fact>]
let ``_hxTriggerTyped should generate correct hx-trigger attribute`` () =
    let div = div [_hxTriggerTyped Click] []
    
    getAttrValue div "hx-trigger" |> should be True
    getAttrValueContent div "hx-trigger" |> should equal (Some "click")

[<Fact>]
let ``_hxTriggerTyped with complex trigger should generate correct attribute`` () =
    let trigger = KeyUp |> HtmxTrigger.withKey "Enter" |> HtmxTrigger.delay 300 |> HtmxTrigger.once
    let div = div [_hxTriggerTyped trigger] []
    
    getAttrValue div "hx-trigger" |> should be True
    let actualValue = getAttrValueContent div "hx-trigger"
    match actualValue with
    | Some value -> 
        let decoded = decodeHtml(value)
        decoded |> should equal "keyup[key=='Enter'] delay:300ms once"
    | None -> 
        failwith "Expected attribute value but got None"

// Tests for _hxSwapTyped attribute generation
[<Fact>]
let ``_hxSwapTyped should generate correct hx-swap attribute`` () =
    let div = div [_hxSwapTyped InnerHTML] []
    
    getAttrValue div "hx-swap" |> should be True
    getAttrValueContent div "hx-swap" |> should equal (Some "innerHTML")

[<Fact>]
let ``_hxSwapTyped with modifiers should generate correct attribute`` () =
    let swap = InnerHTML |> HtmxSwap.withTransition |> HtmxSwap.withSwapDelay 100
    let div = div [_hxSwapTyped swap] []
    
    getAttrValue div "hx-swap" |> should be True
    getAttrValueContent div "hx-swap" |> should equal (Some "innerHTML transition:true swap:100ms")

// Tests for HtmxTrigger module builders
[<Fact>]
let ``HtmxTrigger module should provide correct basic triggers`` () =
    HtmxTrigger.click |> should equal Click
    HtmxTrigger.submit |> should equal Submit
    HtmxTrigger.change |> should equal Change
    HtmxTrigger.keyUp |> should equal KeyUp
    HtmxTrigger.keyDown |> should equal KeyDown
    HtmxTrigger.mouseOver |> should equal MouseOver
    HtmxTrigger.mouseOut |> should equal MouseOut
    HtmxTrigger.load |> should equal Load

[<Fact>]
let ``HtmxTrigger custom should create custom trigger`` () =
    let trigger = HtmxTrigger.custom "my-event"
    trigger |> should equal (HtmxTrigger.Custom "my-event")

[<Fact>]
let ``HtmxTrigger withKey should create keyboard filter`` () =
    let trigger = HtmxTrigger.withKey "Enter" Click
    trigger |> should equal (KeyboardFilter(Click, "key=='Enter'"))

[<Fact>]
let ``HtmxTrigger withKeyCode should create keyboard filter`` () =
    let trigger = HtmxTrigger.withKeyCode 13 KeyUp
    trigger |> should equal (KeyboardFilter(KeyUp, "keyCode==13"))

[<Fact>]
let ``HtmxTrigger fluent API should chain correctly`` () =
    let trigger = 
        HtmxTrigger.keyUp
        |> HtmxTrigger.withKey "Enter"
        |> HtmxTrigger.delay 300
        |> HtmxTrigger.once
    
    trigger |> should equal (Once(Delay(KeyboardFilter(KeyUp, "key=='Enter'"), 300)))

// Tests for HtmxSwap module builders
[<Fact>]
let ``HtmxSwap module should provide correct basic swaps`` () =
    HtmxSwap.innerHTML |> should equal InnerHTML
    HtmxSwap.outerHTML |> should equal OuterHTML
    HtmxSwap.beforeBegin |> should equal BeforeBegin
    HtmxSwap.afterBegin |> should equal AfterBegin
    HtmxSwap.beforeEnd |> should equal BeforeEnd
    HtmxSwap.afterEnd |> should equal AfterEnd
    HtmxSwap.delete |> should equal Delete
    HtmxSwap.none |> should equal NoSwap

[<Fact>]
let ``HtmxSwap custom should create custom swap`` () =
    let swap = HtmxSwap.custom "my-swap"
    swap |> should equal (HtmxSwap.Custom "my-swap")

[<Fact>]
let ``HtmxSwap withTransition should add transition modifier`` () =
    let swap = HtmxSwap.withTransition InnerHTML
    swap |> should equal (WithModifiers(InnerHTML, [Transition true]))

[<Fact>]
let ``HtmxSwap withSwapDelay should add swap delay modifier`` () =
    let swap = HtmxSwap.withSwapDelay 100 OuterHTML
    swap |> should equal (WithModifiers(OuterHTML, [HtmxSwapModifier.Swap 100]))

[<Fact>]
let ``HtmxSwap withSettleDelay should add settle delay modifier`` () =
    let swap = HtmxSwap.withSettleDelay 200 InnerHTML
    swap |> should equal (WithModifiers(InnerHTML, [Settle 200]))

[<Fact>]
let ``HtmxSwap withScroll should add scroll modifier`` () =
    let swap = HtmxSwap.withScroll "top" InnerHTML
    swap |> should equal (WithModifiers(InnerHTML, [Scroll "top"]))

[<Fact>]
let ``HtmxSwap withShow should add show modifier`` () =
    let swap = HtmxSwap.withShow "#result" InnerHTML
    swap |> should equal (WithModifiers(InnerHTML, [Show "#result"]))

[<Fact>]
let ``HtmxSwap withModifiers should add multiple modifiers`` () =
    let modifiers = [Transition true; HtmxSwapModifier.Swap 100; Settle 200]
    let swap = HtmxSwap.withModifiers modifiers InnerHTML
    swap |> should equal (WithModifiers(InnerHTML, modifiers))

// Integration tests with real HTML elements
[<Fact>]
let ``Button with composable attributes should generate correct HTML`` () =
    let button = 
        button [
            _hxPost "/api/submit"
            _hxTriggerTyped (HtmxTrigger.click |> HtmxTrigger.once)
            _hxSwapTyped (HtmxSwap.outerHTML |> HtmxSwap.withTransition)
            _hxTarget "#result"
        ] [str "Submit"]
    
    // Check all attributes are present
    getAttrValue button "hx-post" |> should be True
    getAttrValue button "hx-trigger" |> should be True
    getAttrValue button "hx-swap" |> should be True
    getAttrValue button "hx-target" |> should be True
    
    // Check attribute values
    getAttrValueContent button "hx-post" |> should equal (Some "/api/submit")
    getAttrValueContent button "hx-trigger" |> should equal (Some "click once")
    getAttrValueContent button "hx-swap" |> should equal (Some "outerHTML transition:true")
    getAttrValueContent button "hx-target" |> should equal (Some "#result")

[<Fact>]
let ``Input with keyboard trigger should generate correct HTML`` () =
    let triggerWithModifiers = 
        HtmxTrigger.keyUp 
        |> HtmxTrigger.withKey "Enter"
        |> HtmxTrigger.delay 300
        |> HtmxTrigger.throttle 500
    
    let inputElement = 
        tag "input" [
            _type "text"
            _hxPost "/api/search"
            _hxTriggerTyped triggerWithModifiers
            _hxTarget "#search-results"
        ] []
    
    getAttrValue inputElement "hx-trigger" |> should be True
    let actualValue = getAttrValueContent inputElement "hx-trigger"
    match actualValue with
    | Some value -> 
        let decoded = decodeHtml(value)
        decoded |> should equal "keyup[key=='Enter'] delay:300ms throttle:500ms"
    | None -> 
        failwith "Expected attribute value but got None"

[<Fact>]
let ``Form with from trigger should generate correct HTML`` () =
    let div = 
        div [
            _hxPost "/api/update"
            _hxTriggerTyped (
                HtmxTrigger.click
                |> HtmxTrigger.from "#external-button"
            )
        ] []
    
    getAttrValue div "hx-trigger" |> should be True
    getAttrValueContent div "hx-trigger" |> should equal (Some "click from:#external-button")

// Edge case tests
[<Fact>]
let ``Empty custom trigger should work`` () =
    let trigger = HtmxTrigger.Custom ""
    let result = triggerToString trigger
    result |> should equal ""

[<Fact>]
let ``Empty custom swap should work`` () =
    let swap = HtmxSwap.Custom ""
    let result = swapToString swap
    result |> should equal ""

[<Fact>]
let ``Multiple nested trigger modifiers should format correctly`` () =
    let trigger = 
        Click
        |> HtmxTrigger.once
        |> HtmxTrigger.delay 100
        |> HtmxTrigger.throttle 200
        |> HtmxTrigger.from "#button"
    
    let result = triggerToString trigger
    result |> should equal "click once delay:100ms throttle:200ms from:#button"

[<Fact>]
let ``Backward compatibility with string-based attributes should work`` () =
    let button1 = button [_hxTrigger "click once"] []
    let button2 = button [_hxTriggerTyped (HtmxTrigger.click |> HtmxTrigger.once)] []
    
    let value1 = getAttrValueContent button1 "hx-trigger"
    let value2 = getAttrValueContent button2 "hx-trigger"
    
    value1 |> should equal value2

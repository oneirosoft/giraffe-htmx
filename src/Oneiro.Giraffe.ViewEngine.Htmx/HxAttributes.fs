[<AutoOpen>]
module Giraffe.ViewEngine.Htmx.Attributes
    
    open Giraffe.ViewEngine
    
    /// <summary>
    /// Represents different types of HTMX trigger events that can initiate requests.
    /// Provides type-safe alternatives to string-based trigger definitions.
    /// </summary>
    type HtmxTrigger = 
        /// Triggered on mouse click
        | Click
        /// Triggered on form submission
        | Submit
        /// Triggered when form element value changes
        | Change
        /// Triggered on key up event
        | KeyUp
        /// Triggered on key down event
        | KeyDown
        /// Triggered when mouse enters element
        | MouseOver
        /// Triggered when mouse leaves element
        | MouseOut
        /// Triggered when element loads
        | Load
        /// Custom trigger event with arbitrary string
        | Custom of string
        /// Trigger with keyboard filter (e.g., KeyUp with "key=='Enter'")
        | KeyboardFilter of HtmxTrigger * string
        /// Trigger that fires only once
        | Once of HtmxTrigger
        /// Trigger with delay in milliseconds
        | Delay of HtmxTrigger * int
        /// Trigger with throttling in milliseconds
        | Throttle of HtmxTrigger * int
        /// Trigger from a specific CSS selector
        | From of HtmxTrigger * string
        
    /// <summary>
    /// Represents different HTMX swap strategies for how response content replaces DOM elements.
    /// Provides type-safe alternatives to string-based swap definitions.
    /// </summary>
    type HtmxSwap =
        /// Replace inner HTML of target element
        | InnerHTML
        /// Replace the entire target element
        | OuterHTML
        /// Insert before the target element
        | BeforeBegin
        /// Insert as first child of target element
        | AfterBegin
        /// Insert as last child of target element
        | BeforeEnd
        /// Insert after the target element
        | AfterEnd
        /// Delete the target element
        | Delete
        /// Do not swap content
        | NoSwap
        /// Custom swap strategy with arbitrary string
        | Custom of string
        /// Swap strategy with additional modifiers
        | WithModifiers of HtmxSwap * HtmxSwapModifier list
        
    /// <summary>
    /// Modifiers that can be applied to swap operations to control timing and behavior.
    /// </summary>
    and HtmxSwapModifier =
        /// Control CSS transitions during swap
        | Transition of bool
        /// Delay before performing swap in milliseconds
        | Swap of int
        /// Delay before settling content in milliseconds
        | Settle of int
        /// Whether to ignore title updates
        | IgnoreTitle of bool
        /// Scroll to specific element or position
        | Scroll of string
        /// Show specific element or position
        | Show of string
        
    /// <summary>
    /// Converts an HtmxTrigger union case to its string representation for use in HTML attributes.
    /// Handles complex trigger compositions including filters, delays, and modifiers.
    /// </summary>
    /// <param name="trigger">The HtmxTrigger to convert</param>
    /// <returns>String representation suitable for hx-trigger attribute</returns>
    let rec triggerToString = function
        | Click -> "click"
        | Submit -> "submit" 
        | Change -> "change"
        | KeyUp -> "keyup"
        | KeyDown -> "keydown"
        | MouseOver -> "mouseover"
        | MouseOut -> "mouseout"
        | Load -> "load"
        | HtmxTrigger.Custom s -> s
        | KeyboardFilter(trigger, filter) -> sprintf "%s[%s]" (triggerToString trigger) filter
        | Once(trigger) -> sprintf "%s once" (triggerToString trigger)
        | Delay(trigger, ms) -> sprintf "%s delay:%dms" (triggerToString trigger) ms
        | Throttle(trigger, ms) -> sprintf "%s throttle:%dms" (triggerToString trigger) ms
        | From(trigger, selector) -> sprintf "%s from:%s" (triggerToString trigger) selector
        
    /// <summary>
    /// Converts an HtmxSwap union case to its string representation for use in HTML attributes.
    /// Handles swap strategies and their modifiers for precise DOM manipulation control.
    /// </summary>
    /// <param name="swap">The HtmxSwap to convert</param>
    /// <returns>String representation suitable for hx-swap attribute</returns>
    let rec swapToString = function
        | InnerHTML -> "innerHTML"
        | OuterHTML -> "outerHTML"
        | BeforeBegin -> "beforebegin"
        | AfterBegin -> "afterbegin"
        | BeforeEnd -> "beforeend"
        | AfterEnd -> "afterend"
        | Delete -> "delete"
        | NoSwap -> "none"
        | HtmxSwap.Custom s -> s
        | WithModifiers(swap, modifiers) ->
            let baseSwap = swapToString swap
            let modifierStrings = 
                modifiers
                |> List.map (function
                    | Transition true -> "transition:true"
                    | Transition false -> "transition:false"
                    | HtmxSwapModifier.Swap ms -> sprintf "swap:%dms" ms
                    | Settle ms -> sprintf "settle:%dms" ms
                    | IgnoreTitle true -> "ignoreTitle:true"
                    | IgnoreTitle false -> "ignoreTitle:false"
                    | Scroll selector -> sprintf "scroll:%s" selector
                    | Show selector -> sprintf "show:%s" selector)
                |> String.concat " "
            sprintf "%s %s" baseSwap modifierStrings
    
    // ===== Core HTMX Attribute Functions =====
    // String-based functions for backward compatibility and direct usage
    
    /// <summary>Creates an hx-get attribute for HTTP GET requests</summary>
    /// <param name="url">The URL to send the GET request to</param>
    /// <returns>An HTML attribute for hx-get</returns>
    let _hxGet (url: string) = attr "hx-get" url
    
    /// <summary>Creates an hx-post attribute for HTTP POST requests</summary>
    /// <param name="url">The URL to send the POST request to</param>
    /// <returns>An HTML attribute for hx-post</returns>
    let _hxPost (url: string) = attr "hx-post" url
    
    /// <summary>Creates an hx-put attribute for HTTP PUT requests</summary>
    /// <param name="url">The URL to send the PUT request to</param>
    /// <returns>An HTML attribute for hx-put</returns>
    let _hxPut (url: string) = attr "hx-put" url
    
    /// <summary>Creates an hx-delete attribute for HTTP DELETE requests</summary>
    /// <param name="url">The URL to send the DELETE request to</param>
    /// <returns>An HTML attribute for hx-delete</returns>
    let _hxDelete (url: string) = attr "hx-delete" url
    
    /// <summary>Creates an hx-patch attribute for HTTP PATCH requests</summary>
    /// <param name="url">The URL to send the PATCH request to</param>
    /// <returns>An HTML attribute for hx-patch</returns>
    let _hxPatch (url: string) = attr "hx-patch" url
    
    /// <summary>Creates an hx-trigger attribute with custom trigger specification</summary>
    /// <param name="trigger">The trigger event specification string</param>
    /// <returns>An HTML attribute for hx-trigger</returns>
    let _hxTrigger (trigger: string) = attr "hx-trigger" trigger
    
    /// <summary>Creates an hx-target attribute specifying where to place response content</summary>
    /// <param name="target">CSS selector for the target element</param>
    /// <returns>An HTML attribute for hx-target</returns>
    let _hxTarget (target: string) = attr "hx-target" target
    
    /// <summary>Creates an hx-swap attribute specifying how to swap response content</summary>
    /// <param name="swap">The swap strategy specification string</param>
    /// <returns>An HTML attribute for hx-swap</returns>
    let _hxSwap (swap: string) = attr "hx-swap" swap
    
    /// <summary>Creates an hx-vals attribute for additional values to submit</summary>
    /// <param name="vals">JSON string or JavaScript object for additional values</param>
    /// <returns>An HTML attribute for hx-vals</returns>
    let _hxVals (vals: string) = attr "hx-vals" vals
    
    /// <summary>Creates an hx-boost attribute to enable progressive enhancement</summary>
    /// <param name="boost">Whether to enable boosting for this element</param>
    /// <returns>An HTML attribute for hx-boost</returns>
    let _hxBoost (boost: bool) = attr "hx-boost" (if boost then "true" else "false")
    
    /// <summary>Creates an hx-push-url attribute to update browser URL</summary>
    /// <param name="pushUrl">URL to push to browser history, or "true"/"false"</param>
    /// <returns>An HTML attribute for hx-push-url</returns>
    let _hxPushUrl (pushUrl: string) = attr "hx-push-url" pushUrl
    
    /// <summary>Creates an hx-select attribute to select specific content from response</summary>
    /// <param name="select">CSS selector for content to extract from response</param>
    /// <returns>An HTML attribute for hx-select</returns>
    let _hxSelect (select: string) = attr "hx-select" select
    
    /// <summary>Creates an hx-select-oob attribute for out-of-band content selection</summary>
    /// <param name="selectOob">CSS selector for out-of-band content</param>
    /// <returns>An HTML attribute for hx-select-oob</returns>
    let _hxSelectOob (selectOob: string) = attr "hx-select-oob" selectOob
    
    /// <summary>Creates an hx-include attribute to include additional elements in request</summary>
    /// <param name="include_">CSS selector for elements to include in request</param>
    /// <returns>An HTML attribute for hx-include</returns>
    let _hxInclude (include_: string) = attr "hx-include" include_
    
    /// <summary>Creates an hx-params attribute to filter parameters sent in request</summary>
    /// <param name="params_">Parameter filter specification ("*", "none", or comma-separated list)</param>
    /// <returns>An HTML attribute for hx-params</returns>
    let _hxParams (params_: string) = attr "hx-params" params_
    
    /// <summary>Creates an hx-indicator attribute to specify loading indicator</summary>
    /// <param name="indicator">CSS selector for the loading indicator element</param>
    /// <returns>An HTML attribute for hx-indicator</returns>
    let _hxIndicator (indicator: string) = attr "hx-indicator" indicator
    
    /// <summary>Creates an hx-history attribute to control history behavior</summary>
    /// <param name="history">Whether to enable history support</param>
    /// <returns>An HTML attribute for hx-history</returns>
    let _hxHistory (history: bool) = attr "hx-history" (if history then "true" else "false")
    
    /// <summary>Creates an hx-sync attribute to synchronize requests</summary>
    /// <param name="sync">Synchronization strategy specification</param>
    /// <returns>An HTML attribute for hx-sync</returns>
    let _hxSync (sync: string) = attr "hx-sync" sync
    
    /// <summary>Creates an hx-encoding attribute to specify request encoding</summary>
    /// <param name="encoding">Encoding type (e.g., "multipart/form-data")</param>
    /// <returns>An HTML attribute for hx-encoding</returns>
    let _hxEncoding (encoding: string) = attr "hx-encoding" encoding
    
    /// <summary>Creates an hx-ext attribute to enable HTMX extensions</summary>
    /// <param name="ext">Extension name or comma-separated list of extensions</param>
    /// <returns>An HTML attribute for hx-ext</returns>
    let _hxExt (ext: string) = attr "hx-ext" ext
    
    /// <summary>Creates an hx-disable attribute to disable HTMX processing</summary>
    /// <returns>An HTML attribute for hx-disable</returns>
    let _hxDisable = attr "hx-disable" ""
    
    /// <summary>Creates an hx-confirm attribute to show confirmation dialog</summary>
    /// <param name="confirm">Confirmation message to display</param>
    /// <returns>An HTML attribute for hx-confirm</returns>
    let _hxConfirm (confirm: string) = attr "hx-confirm" confirm
    
    /// <summary>Creates an hx-sse attribute for Server-Sent Events</summary>
    /// <param name="sse">SSE configuration specification</param>
    /// <returns>An HTML attribute for hx-sse</returns>
    let _hxSse (sse: string) = attr "hx-sse" sse
    
    /// <summary>Creates an hx-ws attribute for WebSocket connections</summary>
    /// <param name="ws">WebSocket configuration specification</param>
    /// <returns>An HTML attribute for hx-ws</returns>
    let _hxWs (ws: string) = attr "hx-ws" ws
    
    /// <summary>Creates an hx-oob attribute for out-of-band swaps</summary>
    /// <returns>An HTML attribute for hx-oob</returns>
    let _hxOob = attr "hx-oob" ""

    // ===== Additional Newer HTMX Attributes =====
    
    /// <summary>Creates an hx-swap-oob attribute for out-of-band content swapping</summary>
    /// <param name="value">OOB swap specification (e.g., "true", CSS selector)</param>
    /// <returns>An HTML attribute for hx-swap-oob</returns>
    let _hxSwapOob (value: string) = attr "hx-swap-oob" value
    
    /// <summary>Creates an hx-preserve attribute to preserve elements during swaps</summary>
    /// <returns>An HTML attribute for hx-preserve</returns>
    let _hxPreserve = attr "hx-preserve" ""
    
    /// <summary>Creates an hx-headers attribute for custom request headers</summary>
    /// <param name="headers">JSON object string defining headers to add</param>
    /// <returns>An HTML attribute for hx-headers</returns>
    let _hxHeaders (headers: string) = attr "hx-headers" headers
    
    /// <summary>Creates an hx-disinherit attribute to prevent inheritance of certain attributes</summary>
    /// <param name="disinherit">Space-separated list of attributes to not inherit</param>
    /// <returns>An HTML attribute for hx-disinherit</returns>
    let _hxDisinherit (disinherit: string) = attr "hx-disinherit" disinherit
    
    /// <summary>Creates an hx-replace-url attribute to replace browser URL without history entry</summary>
    /// <param name="url">URL to replace in browser location, or "true"/"false"</param>
    /// <returns>An HTML attribute for hx-replace-url</returns>
    let _hxReplaceUrl (url: string) = attr "hx-replace-url" url
    
    /// <summary>Creates an hx-request attribute for request configuration</summary>
    /// <param name="request">JSON object string configuring the request</param>
    /// <returns>An HTML attribute for hx-request</returns>
    let _hxRequest (request: string) = attr "hx-request" request
    
    /// <summary>Creates an hx-on attribute for inline event handling</summary>
    /// <param name="event">The event name (e.g., "click", "htmx:afterRequest")</param>
    /// <param name="code">JavaScript code to execute</param>
    /// <returns>An HTML attribute for hx-on:{event}</returns>
    let _hxOn (event: string, code: string) = attr (sprintf "hx-on:%s" event) code
    
    // ===== Boolean Convenience Variants =====
    
    /// <summary>Creates an hx-boost attribute set to true for progressive enhancement</summary>
    /// <returns>An HTML attribute for hx-boost="true"</returns>
    let _hxBoostTrue = _hxBoost true
    
    /// <summary>Creates an hx-history attribute set to true to enable history support</summary>
    /// <returns>An HTML attribute for hx-history="true"</returns>
    let _hxHistoryTrue = _hxHistory true
    
    /// <summary>Creates an hx-oob attribute set to true for out-of-band swapping</summary>
    /// <returns>An HTML attribute for hx-oob="true"</returns>
    let _hxOobTrue = attr "hx-oob" "true"
    
    /// <summary>Creates an hx-preserve attribute set to true to preserve element during swaps</summary>
    /// <returns>An HTML attribute for hx-preserve="true"</returns>
    let _hxPreserveTrue = attr "hx-preserve" "true"
    
    // ===== Type-Safe Composable Versions =====
    
    /// <summary>Creates an hx-trigger attribute using type-safe HtmxTrigger union</summary>
    /// <param name="trigger">Strongly-typed trigger specification</param>
    /// <returns>An HTML attribute for hx-trigger with type-safe value</returns>
    let _hxTriggerTyped (trigger: HtmxTrigger) = attr "hx-trigger" (triggerToString trigger)
    
    /// <summary>Creates an hx-swap attribute using type-safe HtmxSwap union</summary>
    /// <param name="swap">Strongly-typed swap specification</param>
    /// <returns>An HTML attribute for hx-swap with type-safe value</returns>
    let _hxSwapTyped (swap: HtmxSwap) = attr "hx-swap" (swapToString swap)
    
    // ===== Fluent Builders for Complex Scenarios =====
    
    /// <summary>
    /// Fluent builder module for constructing complex HtmxTrigger compositions.
    /// Provides type-safe way to build triggers with modifiers like delays, filters, and conditions.
    /// </summary>
    module HtmxTrigger =
        /// Basic click trigger
        let click = Click
        /// Basic form submission trigger  
        let submit = Submit
        /// Basic change event trigger
        let change = Change
        /// Basic key up trigger
        let keyUp = KeyUp
        /// Basic key down trigger
        let keyDown = KeyDown
        /// Basic mouse over trigger
        let mouseOver = MouseOver
        /// Basic mouse out trigger
        let mouseOut = MouseOut
        /// Basic load event trigger
        let load = Load
        /// Custom trigger with arbitrary event name
        let custom s = HtmxTrigger.Custom s
        
        /// <summary>Adds keyboard filter to trigger (e.g., specific key)</summary>
        /// <param name="key">Key to filter on (e.g., "Enter", "Escape")</param>
        /// <param name="trigger">Base trigger to add filter to</param>
        /// <returns>Trigger with keyboard filter applied</returns>
        let withKey key trigger = KeyboardFilter(trigger, sprintf "key=='%s'" key)
        
        /// <summary>Adds keyboard filter to trigger using key code</summary>
        /// <param name="code">Key code to filter on</param>
        /// <param name="trigger">Base trigger to add filter to</param>
        /// <returns>Trigger with key code filter applied</returns>
        let withKeyCode code trigger = KeyboardFilter(trigger, sprintf "keyCode==%d" code)
        
        /// <summary>Makes trigger fire only once</summary>
        /// <param name="trigger">Base trigger to modify</param>
        /// <returns>Trigger that fires only once</returns>
        let once trigger = Once trigger
        
        /// <summary>Adds delay to trigger</summary>
        /// <param name="ms">Delay in milliseconds</param>
        /// <param name="trigger">Base trigger to add delay to</param>
        /// <returns>Trigger with delay applied</returns>
        let delay ms trigger = Delay(trigger, ms)
        
        /// <summary>Adds throttling to trigger</summary>
        /// <param name="ms">Throttle interval in milliseconds</param>
        /// <param name="trigger">Base trigger to throttle</param>
        /// <returns>Trigger with throttling applied</returns>
        let throttle ms trigger = Throttle(trigger, ms)
        
        /// <summary>Makes trigger listen from different element</summary>
        /// <param name="selector">CSS selector for element to listen on</param>
        /// <param name="trigger">Base trigger to modify</param>
        /// <returns>Trigger that listens from specified element</returns>
        let from selector trigger = From(trigger, selector)
        
    /// <summary>
    /// Fluent builder module for constructing complex HtmxSwap compositions.
    /// Provides type-safe way to build swap strategies with modifiers for timing and behavior.
    /// </summary>
    module HtmxSwap =
        /// Replace inner HTML of target
        let innerHTML = InnerHTML
        /// Replace entire target element
        let outerHTML = OuterHTML
        /// Insert before target element
        let beforeBegin = BeforeBegin
        /// Insert as first child of target
        let afterBegin = AfterBegin
        /// Insert as last child of target
        let beforeEnd = BeforeEnd
        /// Insert after target element
        let afterEnd = AfterEnd
        /// Delete the target element
        let delete = Delete
        /// Do not swap any content
        let none = NoSwap
        /// Custom swap strategy with arbitrary specification
        let custom s = HtmxSwap.Custom s
        
        /// <summary>Enables CSS transitions during swap</summary>
        /// <param name="swap">Base swap strategy to add transition to</param>
        /// <returns>Swap strategy with transitions enabled</returns>
        let withTransition swap = WithModifiers(swap, [Transition true])
        
        /// <summary>Adds delay before performing swap</summary>
        /// <param name="ms">Delay in milliseconds</param>
        /// <param name="swap">Base swap strategy to add delay to</param>
        /// <returns>Swap strategy with swap delay applied</returns>
        let withSwapDelay ms swap = WithModifiers(swap, [HtmxSwapModifier.Swap ms])
        
        /// <summary>Adds delay before settling content after swap</summary>
        /// <param name="ms">Settle delay in milliseconds</param>
        /// <param name="swap">Base swap strategy to add settle delay to</param>
        /// <returns>Swap strategy with settle delay applied</returns>
        let withSettleDelay ms swap = WithModifiers(swap, [Settle ms])
        
        /// <summary>Configures scrolling behavior after swap</summary>
        /// <param name="selector">CSS selector, "top", "bottom", or scroll target</param>
        /// <param name="swap">Base swap strategy to add scrolling to</param>
        /// <returns>Swap strategy with scroll behavior configured</returns>
        let withScroll selector swap = WithModifiers(swap, [Scroll selector])
        
        /// <summary>Configures showing behavior after swap</summary>
        /// <param name="selector">CSS selector, "top", "bottom", or show target</param>
        /// <param name="swap">Base swap strategy to add show behavior to</param>
        /// <returns>Swap strategy with show behavior configured</returns>
        let withShow selector swap = WithModifiers(swap, [Show selector])
        
        /// <summary>Applies multiple modifiers to swap strategy</summary>
        /// <param name="modifiers">List of swap modifiers to apply</param>
        /// <param name="swap">Base swap strategy to modify</param>
        /// <returns>Swap strategy with all modifiers applied</returns>
        let withModifiers modifiers swap = WithModifiers(swap, modifiers)

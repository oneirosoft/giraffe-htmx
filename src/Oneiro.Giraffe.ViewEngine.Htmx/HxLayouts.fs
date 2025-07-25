/// <summary>
/// Module for creating HTML layouts optimized for HTMX applications.
/// Provides type-safe layout builders with HTMX script integration and configuration options.
/// </summary>
module Giraffe.ViewEngine.Htmx.Layouts

open Microsoft.FSharp.Reflection
open Giraffe.ViewEngine

/// <summary>
/// Represents supported HTMX library versions for CDN script inclusion.
/// Supports both predefined versions and custom version strings.
/// </summary>
type HtmxVersion = 
    /// HTMX version 2.0.0
    | V2_0_0 
    /// HTMX version 2.0.1
    | V2_0_1 
    /// HTMX version 2.0.2
    | V2_0_2 
    /// HTMX version 2.0.3
    | V2_0_3
    /// HTMX version 2.0.4
    | V2_0_4 
    /// HTMX version 2.0.5
    | V2_0_5 
    /// HTMX version 2.0.6
    | V2_0_6
    /// Custom version string for specific builds or local versions
    | Custom of string
    with 
        override x.ToString() =
            match x with
            | Custom s -> s
            | _ ->
                FSharpValue.GetUnionFields (x, x.GetType())
                |> fst
                |> fun case -> case.Name.Replace("_", ".")
                |> fun value -> value[1..]

/// <summary>
/// Configuration options for HTMX-enabled HTML layouts.
/// Provides comprehensive control over document structure and HTMX integration.
/// </summary>
type HtmxLayoutOptions = 
        { /// Optional page title for the HTML document
          Title: string option
          /// Additional script elements to include in the head
          Scripts: XmlNode list
          /// CSS style declarations or external stylesheet URLs
          Styles: string list
          /// HTML attributes to apply to the body element
          BodyAttributes: XmlAttribute list
          /// Additional elements to include in the head section
          HeadExtras: XmlNode list
          /// HTMX library version to include
          Version: HtmxVersion }
    with 
        /// <summary>Default layout options with HTMX 2.0.6 and minimal configuration</summary>
        static member Default =
            { Title = None
              Scripts = []
              Styles = []
              BodyAttributes = []
              HeadExtras = []
              Version = V2_0_6 }

/// <summary>
/// Computation expression builder for creating HTMX layout configurations.
/// Provides a fluent API for configuring HTML layout options including scripts, styles, and HTMX settings.
/// </summary>
type HtmxLayoutBuilder() =
    /// <summary>Sets the page title for the HTML document</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="title">Page title to set</param>
    /// <returns>Updated layout options with title set</returns>
    [<CustomOperation("title")>]
    member _.WithTitle(state: HtmxLayoutOptions, title) =
        { state with Title = Some title }
    
    /// <summary>Sets the HTMX library version to include</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="version">HTMX version to use</param>
    /// <returns>Updated layout options with version set</returns>
    [<CustomOperation("version")>]
    member _.WithVersion(state: HtmxLayoutOptions, version) =
        { state with Version = version }
    
    /// <summary>Adds CSS stylesheets to the layout</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="styleUrls">List of CSS URLs or inline styles to add</param>
    /// <returns>Updated layout options with styles added</returns>
    [<CustomOperation("styles")>]
    member _.WithStyles(state: HtmxLayoutOptions, styleUrls) =
        { state with Styles = state.Styles @ styleUrls }
    
    /// <summary>Adds JavaScript scripts to the layout</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="scripts">List of script elements to add</param>
    /// <returns>Updated layout options with scripts added</returns>
    [<CustomOperation("scripts")>]
    member _.WithScripts(state: HtmxLayoutOptions, scripts) =
        { state with Scripts = state.Scripts @ scripts }
        
    /// <summary>Sets HTML attributes for the body element</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="attrs">List of HTML attributes to apply to body</param>
    /// <returns>Updated layout options with body attributes set</returns>
    [<CustomOperation("bodyAttr")>]
    member _.WithBodyAttributes(state: HtmxLayoutOptions, attrs) =
        { state with BodyAttributes = attrs }
        
    /// <summary>Adds additional elements to the HTML head section</summary>
    /// <param name="state">Current layout options</param>
    /// <param name="head">List of elements to add to head</param>
    /// <returns>Updated layout options with head extras added</returns>
    [<CustomOperation("head")>]
    member _.WithHeadExtras(state: HtmxLayoutOptions, head) =
        { state with HeadExtras = head }
        
    member _.Yield _ = 
        HtmxLayoutOptions.Default
    member _.Zero() = HtmxLayoutOptions.Default
    
    /// <summary>
    /// Builds the final layout function from the configured options.
    /// Creates a complete HTML document with HTMX integration and all specified options.
    /// </summary>
    /// <param name="state">Configured layout options</param>
    /// <returns>Function that takes content and returns complete HTML document</returns>
    member _.Run(state: HtmxLayoutOptions) =
        let htmxScriptUrl = $"https://unpkg.com/htmx.org@{state.Version}/dist/htmx.min.js"
        let titleTag =
            [state.Title |> Option.defaultValue "" |> str]
            |> tag "title" []
        let styles = 
            state.Styles 
            |> List.map (fun url -> link [ _rel "stylesheet"; _href url ])
        
        let htmxScript = script [ _src htmxScriptUrl ] []
        let allScripts = htmxScript :: state.Scripts

        fun (content: XmlNode list) ->
            html [] [
                head [] [ 
                    titleTag
                    meta [ _charset "utf-8" ]
                    meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
                    yield! allScripts 
                    yield! styles
                    yield! state.HeadExtras
                ]
                body state.BodyAttributes content
            ]

/// <summary>
/// Global instance of HtmxLayoutBuilder for creating HTMX-enabled HTML layouts.
/// Use this builder with computation expression syntax to configure layout options.
/// </summary>
/// <example>
/// <code>
/// let myLayout = htmxLayout {
///     title "My HTMX App"
///     version V2_0_6
///     styles ["https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css"]
/// }
/// </code>
/// </example>
let hxLayout = HtmxLayoutBuilder()
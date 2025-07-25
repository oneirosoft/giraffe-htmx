module Oneiro.Giraffe.ViewEngine.Htmx.Tests.HtmxLayoutTests

open Xunit
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx.Layouts
open AngleSharp.Html.Parser
open Microsoft.FSharp.Reflection

/// Helper to parse the rendered HTML and perform assertions on the structure
let parseHtml (html: string) =
    let parser = HtmlParser()
    parser.ParseDocument html

/// Create our own htmxLayout instance since the module-level one has initialization issues
let htmxLayout = HtmxLayoutBuilder()
    
/// Helper to recreate the version string conversion (since it's internal)
let getVersionString (version: HtmxVersion) =
    match version with
    | HtmxVersion.Custom s -> s
    | _ ->
        FSharpValue.GetUnionFields (version, version.GetType())
        |> fst
        |> fun case -> case.Name.Replace("_", ".")
        |> fun value -> value[1..]


[<Fact>]
let ``Should Have Correct Title`` () =
    let layout = 
        htmxLayout {
            title "My App"
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    Assert.Equal("My App", document.Title)

[<Fact>]
let ``Should Generate Default Layout When No Options Set`` () =
    let layout = htmxLayout { () }
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    // Should have default title (empty)
    Assert.Equal("", document.Title)
    
    // Should have HTMX script with default version (2.0.6)
    let scripts = document.QuerySelectorAll("script[src]")
    Assert.True(scripts.Length > 0)
    let htmxScript = scripts |> Seq.find (fun s -> s.GetAttribute("src").Contains("htmx.org@2.0.6"))
    Assert.NotNull(htmxScript)

[<Fact>]
let ``Should Set Custom HTMX Version`` () =
    let layout = 
        htmxLayout {
            version V2_0_0
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let scripts = document.QuerySelectorAll("script[src]")
    let htmxScript = scripts |> Seq.find (fun s -> s.GetAttribute("src").Contains("htmx.org@2.0.0"))
    Assert.NotNull(htmxScript)

[<Fact>]
let ``Should Set Custom Version String`` () =
    let layout = 
        htmxLayout {
            version (HtmxVersion.Custom "1.9.10")
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let scripts = document.QuerySelectorAll("script[src]")
    let htmxScript = scripts |> Seq.find (fun s -> s.GetAttribute("src").Contains("htmx.org@1.9.10"))
    Assert.NotNull(htmxScript)

[<Fact>]
let ``Should Add Custom Stylesheets`` () =
    let layout = 
        htmxLayout {
            styles ["styles.css"; "theme.css"]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let stylesheets = document.QuerySelectorAll("link[rel='stylesheet']")
    Assert.True(stylesheets.Length >= 2)
    
    let styleUrls = stylesheets |> Seq.map (fun link -> link.GetAttribute("href")) |> Seq.toList
    Assert.Contains("styles.css", styleUrls)
    Assert.Contains("theme.css", styleUrls)

[<Fact>]
let ``Should Add Custom Scripts`` () =
    let customScript = script [] [str "console.log('custom');"]
    let layout = 
        htmxLayout {
            scripts [customScript]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let scripts = document.QuerySelectorAll("script")
    Assert.True(scripts.Length >= 2) // HTMX script + custom script
    
    let customScriptElement = scripts |> Seq.tryFind (fun s -> s.TextContent.Contains("console.log") && s.TextContent.Contains("custom"))
    Assert.True(customScriptElement.IsSome, "Could not find custom script element")

[<Fact>]
let ``Should Add Body Attributes`` () =
    let layout = 
        htmxLayout {
            bodyAttr [_class "my-class"; _id "main-body"]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let body = document.Body
    Assert.Equal("my-class", body.GetAttribute("class"))
    Assert.Equal("main-body", body.GetAttribute("id"))

[<Fact>]
let ``Should Add Head Extras`` () =
    let metaTag = meta [_name "description"; _content "My app description"]
    let layout = 
        htmxLayout {
            head [metaTag]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let metaDescription = document.QuerySelector("meta[name='description']")
    Assert.NotNull(metaDescription)
    Assert.Equal("My app description", metaDescription.GetAttribute("content"))

[<Fact>]
let ``Should Include Standard Meta Tags`` () =
    let layout = htmxLayout { () }
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    // Should have charset meta tag
    let charsetMeta = document.QuerySelector("meta[charset]")
    Assert.NotNull(charsetMeta)
    Assert.Equal("utf-8", charsetMeta.GetAttribute("charset"))
    
    // Should have viewport meta tag
    let viewportMeta = document.QuerySelector("meta[name='viewport']")
    Assert.NotNull(viewportMeta)
    Assert.Equal("width=device-width, initial-scale=1", viewportMeta.GetAttribute("content"))

[<Fact>]
let ``Should Combine All Options`` () =
    let customScript = script [] [str "console.log('test');"]
    let metaTag = meta [_name "author"; _content "Test Author"]
    
    let layout = 
        htmxLayout {
            title "Full Test App"
            version V2_0_3
            styles ["app.css"]
            scripts [customScript]
            bodyAttr [_class "app-body"]
            head [metaTag]
        }
    
    let view = layout [div [] [str "Main Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    // Check title
    Assert.Equal("Full Test App", document.Title)
    
    // Check HTMX version
    let scripts = document.QuerySelectorAll("script[src]")
    let htmxScript = scripts |> Seq.find (fun s -> s.GetAttribute("src").Contains("htmx.org@2.0.3"))
    Assert.NotNull(htmxScript)
    
    // Check stylesheet
    let stylesheet = document.QuerySelector("link[href='app.css']")
    Assert.NotNull(stylesheet)
    
    // Check custom script
    let allScripts = document.QuerySelectorAll("script")
    let customScriptElement = allScripts |> Seq.tryFind (fun s -> s.TextContent.Contains("console.log") && s.TextContent.Contains("test"))
    Assert.True(customScriptElement.IsSome, "Could not find custom script element")
    
    // Check body attributes
    Assert.Equal("app-body", document.Body.GetAttribute("class"))
    
    // Check head extras
    let authorMeta = document.QuerySelector("meta[name='author']")
    Assert.NotNull(authorMeta)
    Assert.Equal("Test Author", authorMeta.GetAttribute("content"))
    
    // Check content
    let content = document.QuerySelector("div")
    Assert.NotNull(content)
    Assert.Equal("Main Content", content.TextContent)

// Tests for HtmxVersion.ToString()
[<Theory>]
[<InlineData("V2_0_0", "2.0.0")>]
[<InlineData("V2_0_1", "2.0.1")>]
[<InlineData("V2_0_2", "2.0.2")>]
[<InlineData("V2_0_3", "2.0.3")>]
[<InlineData("V2_0_4", "2.0.4")>]
[<InlineData("V2_0_5", "2.0.5")>]
[<InlineData("V2_0_6", "2.0.6")>]
let ``HtmxVersion ToString Should Format Version Numbers Correctly`` (versionName: string, expected: string) =
    let version = 
        match versionName with
        | "V2_0_0" -> V2_0_0
        | "V2_0_1" -> V2_0_1
        | "V2_0_2" -> V2_0_2
        | "V2_0_3" -> V2_0_3
        | "V2_0_4" -> V2_0_4
        | "V2_0_5" -> V2_0_5
        | "V2_0_6" -> V2_0_6
        | _ -> failwith $"Unknown version: {versionName}"
    
    Assert.Equal(expected, version.ToString())

[<Fact>]
let ``HtmxVersion Custom Should Return Custom String`` () =
    let customVersion = HtmxVersion.Custom "1.9.10"
    Assert.Equal("1.9.10", customVersion.ToString())

[<Fact>]
let ``HtmxLayoutOptions Default Should Have Correct Values`` () =
    let defaultOptions = HtmxLayoutOptions.Default
    
    Assert.True(defaultOptions.Title.IsNone)
    Assert.Empty(defaultOptions.Scripts)
    Assert.Empty(defaultOptions.Styles)
    Assert.Empty(defaultOptions.BodyAttributes)
    Assert.Empty(defaultOptions.HeadExtras)
    Assert.Equal(V2_0_6, defaultOptions.Version)

[<Fact>]
let ``Multiple Styles Should Be Appended`` () =
    let layout = 
        htmxLayout {
            styles ["first.css"]
            styles ["second.css"; "third.css"]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let stylesheets = document.QuerySelectorAll("link[rel='stylesheet']")
    let styleUrls = stylesheets |> Seq.map (fun link -> link.GetAttribute("href")) |> Seq.toList
    
    Assert.Contains("first.css", styleUrls)
    Assert.Contains("second.css", styleUrls)
    Assert.Contains("third.css", styleUrls)

[<Fact>]
let ``Multiple Scripts Should Be Appended`` () =
    let script1 = script [] [str "console.log('first');"]
    let script2 = script [] [str "console.log('second');"]
    
    let layout = 
        htmxLayout {
            scripts [script1]
            scripts [script2]
        }
    
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let allScripts = document.QuerySelectorAll("script")
    let customScripts = allScripts |> Seq.filter (fun s -> s.TextContent.Contains("console.log"))
    
    Assert.True(customScripts |> Seq.exists (fun s -> s.TextContent.Contains("first")))
    Assert.True(customScripts |> Seq.exists (fun s -> s.TextContent.Contains("second")))

[<Fact>]
let ``Layout Should Render Provided Content`` () =
    let layout = htmxLayout { title "Test" }
    let testContent = [
        h1 [] [str "Welcome"]
        p [] [str "This is a test paragraph"]
        div [_class "footer"] [str "Footer content"]
    ]
    
    let view = layout testContent
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    let h1Element = document.QuerySelector("h1")
    Assert.NotNull(h1Element)
    Assert.Equal("Welcome", h1Element.TextContent)
    
    let pElement = document.QuerySelector("p")
    Assert.NotNull(pElement)
    Assert.Equal("This is a test paragraph", pElement.TextContent)
    
    let footerDiv = document.QuerySelector("div.footer")
    Assert.NotNull(footerDiv)
    Assert.Equal("Footer content", footerDiv.TextContent)

[<Fact>]
let ``Layout Should Generate Valid HTML Structure`` () =
    let layout = htmxLayout { title "Valid HTML Test" }
    let view = layout [div [] [str "Content"]]
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    // Should have proper HTML structure
    Assert.NotNull(document.DocumentElement)
    Assert.Equal("html", document.DocumentElement.TagName.ToLower())
    
    // Should have head and body
    Assert.NotNull(document.Head)
    Assert.NotNull(document.Body)
    
    // Head should contain proper elements
    let titleElement = document.Head.QuerySelector("title")
    Assert.NotNull(titleElement)
    
    let charsetMeta = document.Head.QuerySelector("meta[charset]")
    Assert.NotNull(charsetMeta)
    
    let viewportMeta = document.Head.QuerySelector("meta[name='viewport']")
    Assert.NotNull(viewportMeta)
    
    let htmxScript = document.Head.QuerySelector("script[src*='htmx.org']")
    Assert.NotNull(htmxScript)

[<Fact>]
let ``Layout Should Handle Empty Content`` () =
    let layout = htmxLayout { title "Empty Content Test" }
    let view = layout []
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    Assert.Equal("Empty Content Test", document.Title)
    Assert.NotNull(document.Body)
    // Body should be present but empty (except for whitespace)
    let bodyText = document.Body.TextContent.Trim()
    Assert.Equal("", bodyText)

[<Fact>]
let ``HtmxLayoutBuilder Should Support Zero Method`` () =
    let builder = HtmxLayoutBuilder()
    let defaultState = builder.Zero()
    
    Assert.True(defaultState.Title.IsNone)
    Assert.Empty(defaultState.Scripts)
    Assert.Empty(defaultState.Styles)
    Assert.Empty(defaultState.BodyAttributes)
    Assert.Empty(defaultState.HeadExtras)
    Assert.Equal(V2_0_6, defaultState.Version)

[<Fact>]
let ``HtmxLayoutBuilder Should Support Yield Method`` () =
    let builder = HtmxLayoutBuilder()
    let defaultState = builder.Yield()
    
    Assert.True(defaultState.Title.IsNone)
    Assert.Empty(defaultState.Scripts)
    Assert.Empty(defaultState.Styles)
    Assert.Empty(defaultState.BodyAttributes)
    Assert.Empty(defaultState.HeadExtras)
    Assert.Equal(V2_0_6, defaultState.Version)

[<Fact>]
let ``Should Handle Complex Content Structure`` () =
    let layout = htmxLayout { title "Complex Content" }
    let complexContent = [
        header [_class "main-header"] [
            nav [] [
                ul [] [
                    li [] [a [_href "/"] [str "Home"]]
                    li [] [a [_href "/about"] [str "About"]]
                ]
            ]
        ]
        main [] [
            section [_id "content"] [
                article [] [
                    h1 [] [str "Main Article"]
                    p [] [str "Article content goes here."]
                ]
            ]
        ]
        footer [] [str "© 2025 Test Site"]
    ]
    
    let view = layout complexContent
    let html = RenderView.AsString.htmlNode view
    let document = parseHtml html
    
    // Check header
    let headerElement = document.QuerySelector("header.main-header")
    Assert.NotNull(headerElement)
    
    // Check navigation
    let navLinks = document.QuerySelectorAll("nav a")
    Assert.Equal(2, navLinks.Length)
    
    // Check main content
    let mainElement = document.QuerySelector("main")
    Assert.NotNull(mainElement)
    
    let articleTitle = document.QuerySelector("article h1")
    Assert.NotNull(articleTitle)
    Assert.Equal("Main Article", articleTitle.TextContent)
    
    // Check footer
    let footerElement = document.QuerySelector("footer")
    Assert.NotNull(footerElement)
    Assert.Contains("© 2025 Test Site", footerElement.TextContent)

[<Fact>]
let ``Should Generate Correct HTMX Script URL`` () =
    let testCases = [
        (V2_0_0, "https://unpkg.com/htmx.org@2.0.0/dist/htmx.min.js")
        (V2_0_6, "https://unpkg.com/htmx.org@2.0.6/dist/htmx.min.js")
        (HtmxVersion.Custom "1.9.10", "https://unpkg.com/htmx.org@1.9.10/dist/htmx.min.js")
    ]
    
    for ver, expectedUrl in testCases do
        let layout = htmxLayout { version ver }
        let view = layout [div [] [str "Test"]]
        let html = RenderView.AsString.htmlNode view
        let document = parseHtml html
        
        let htmxScript = document.QuerySelector($"script[src='{expectedUrl}']")
        Assert.NotNull(htmxScript)
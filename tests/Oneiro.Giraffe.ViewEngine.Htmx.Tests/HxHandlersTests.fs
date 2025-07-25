module Oneiro.Giraffe.ViewEngine.Htmx.Tests.HtmxHandlersTests

open Xunit
open FsUnit.Xunit
open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Microsoft.Extensions.Primitives
open System.Collections.Generic
open NSubstitute
open System.IO
open System.Text

// Helper functions to test handlers
let runHandler (ctx: HttpContext) (handler: HttpHandler) =
    task {
        let nextFunc: HttpFunc = fun _ -> Task.FromResult(Some ctx)
        let! result = handler nextFunc ctx
        return result
    }

let createMockContext (isHtmxRequest: bool) (responseBody: Stream) =
    let ctx = Substitute.For<HttpContext>()
    let request = Substitute.For<HttpRequest>()
    let response = Substitute.For<HttpResponse>()
    
    // Set up request headers
    let headers = Dictionary<string, StringValues>()
    if isHtmxRequest then
        headers.Add("HX-Request", StringValues("true"))
    
    let reqHeaderDict = HeaderDictionary()
    for item in headers do
        reqHeaderDict.Add(item.Key, item.Value)
        
    request.Headers.Returns(reqHeaderDict) |> ignore
    ctx.Request.Returns(request) |> ignore
    
    // Set up response
    response.Body.Returns(responseBody) |> ignore
    
    let responseHeaders = Dictionary<string, StringValues>()
    let respHeaderDict = HeaderDictionary()
    response.Headers.Returns(respHeaderDict) |> ignore
    ctx.Response.Returns(response) |> ignore
    
    ctx

[<Fact>]
let ``htmx handler uses content directly for HTMX requests`` () =
    // Arrange
    use responseStream = new MemoryStream()
    let ctx = createMockContext true responseStream
    
    let content = div [_id "test"] [str "HTMX Content"]
    let layout (nodes: XmlNode list) = 
        html [] [
            head [] [title [] [str "Layout Title"]]
            body [] nodes
        ]
    
    // Act
    let result = runHandler ctx (htmx layout (fun _ -> content))
    
    // Assert
    result.Result.IsSome |> should be True
    
    // Check the rendered content
    responseStream.Position <- 0L
    use reader = new StreamReader(responseStream)
    let responseBody = reader.ReadToEnd()
    
    // Should contain just the content, not the layout
    responseBody.Contains("<div id=\"test\">HTMX Content</div>") |> should be True
    responseBody.Contains("<title>Layout Title</title>") |> should be False

[<Fact>]
let ``htmx handler uses layout for non-HTMX requests`` () =
    // Arrange
    use responseStream = new MemoryStream()
    let ctx = createMockContext false responseStream
    
    let content = div [_id "test"] [str "Content"]
    let layout (nodes: XmlNode list) = 
        html [] [
            head [] [title [] [str "Layout Title"]]
            body [] nodes
        ]
    
    // Act
    let result = runHandler ctx (htmx layout (fun _ -> content))
    
    // Assert
    result.Result.IsSome |> should be True
    
    // Check the rendered content
    responseStream.Position <- 0L
    use reader = new StreamReader(responseStream)
    let responseBody = reader.ReadToEnd()
    
    // Should contain the layout with content
    responseBody.Contains("<title>Layout Title</title>") |> should be True
    responseBody.Contains("<div id=\"test\">Content</div>") |> should be True

[<Fact>]
let ``Content with multiple nodes gets properly wrapped`` () =
    // Test a handler with content that has multiple nodes
    use responseStream = new MemoryStream()
    let ctx = createMockContext true responseStream
    
    // Multiple nodes content
    let content = div [] [
        h1 [] [str "Heading"]
        p [] [str "Paragraph"]
    ]
    
    let layout (nodes: XmlNode list) = 
        html [] [
            head [] [title [] [str "Layout"]]
            body [] nodes
        ]
    
    // Act
    let result = runHandler ctx (htmx layout (fun _ -> content))
    
    // Assert
    result.Result.IsSome |> should be True
    
    // Check the rendered content
    responseStream.Position <- 0L
    use reader = new StreamReader(responseStream)
    let responseBody = reader.ReadToEnd()
    
    responseBody.Contains("<h1>Heading</h1>") |> should be True
    responseBody.Contains("<p>Paragraph</p>") |> should be True

[<Fact>]
let ``Layout function gets proper list of nodes`` () =
    // This test verifies the layout function receives content properly
    let mutable layoutReceived = false
    let mutable contentReceived = []
    
    let layout (nodes: XmlNode list) =
        layoutReceived <- true
        contentReceived <- nodes
        html [] [body [] nodes]
    
    let content = div [_id "test"] [str "Test"]
    
    use responseStream = new MemoryStream()
    let ctx = createMockContext false responseStream
    
    // Act
    let result = runHandler ctx (htmx layout (fun _ -> content))
    
    // Assert
    result.Result.IsSome |> should be True
    layoutReceived |> should be True
    contentReceived |> should haveLength 1
    
    // Render the node to string and check its content
    let renderedNode = RenderView.AsString.htmlNode contentReceived.[0]
    renderedNode |> should startWith "<div"
    renderedNode.Contains("id=\"test\"") |> should be True
    renderedNode.Contains(">Test</div>") |> should be True

module Oneiro.Giraffe.ViewEngine.Htmx.Tests.TestHelpers

open Giraffe
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open System.IO
open System.Text
open System.Collections.Generic
open Microsoft.Extensions.Primitives
open NSubstitute
open AngleSharp.Html.Parser

// Helper to run Giraffe HttpHandlers in tests
let runHandler (ctx: HttpContext) (handler: HttpHandler) =
    task {
        let nextFunc: HttpFunc = fun _ -> Task.FromResult(Some ctx)
        let! result = handler nextFunc ctx
        return result
    }

// Helper to create a mock HttpContext for testing
let createMockContext (isHtmxRequest: bool) (responseBody: Stream) =
    let ctx = Substitute.For<HttpContext>()
    let request = Substitute.For<HttpRequest>()
    let response = Substitute.For<HttpResponse>()
    
    // Set up request headers
    let headers = Dictionary<string, StringValues>()
    if isHtmxRequest then
        headers.Add("HX-Request", StringValues("true"))
    request.Headers.Returns(HeaderDictionary(headers)) |> ignore
    ctx.Request.Returns(request) |> ignore
    
    // Set up response
    response.Body.Returns(responseBody) |> ignore
    let responseHeaders = Dictionary<string, StringValues>()
    response.Headers.Returns(HeaderDictionary(responseHeaders)) |> ignore
    ctx.Response.Returns(response) |> ignore
    
    ctx

// Helper to get HTML string content from a stream
let getHtmlContent (stream: MemoryStream) =
    stream.Position <- 0L
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

// Helper to parse HTML and return a document
let parseHtml (html: string) =
    let parser = HtmlParser()
    parser.ParseDocument(html)

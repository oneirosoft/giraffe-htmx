open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.ViewEngine.Htmx.Layouts
open TodoData
open Handlers

let htmxLayout = HtmxLayoutBuilder()

// Layout with custom styles
let layout =
    htmxLayout {
        title "Todo App - HTMX + Giraffe"
        version V2_0_6
        styles [ "/style.css" ]
    }

// Create handlers with the layout
let (indexHandler, filterHandler) = IndexHandler.createHandlers layout
let (addTodoHandler, completeTodoHandler, uncompleteTodoHandler, deleteTodoHandler, clearCompletedHandler) = 
    TodoHandlers.createHandlers layout

// Routing
let webApp =
    choose [
        GET >=> choose [
            route "/" >=> indexHandler All
            route "/all" >=> filterHandler All
            route "/active" >=> filterHandler Active
            route "/completed" >=> filterHandler Completed
        ]
        POST >=> choose [
            route "/todos" >=> addTodoHandler
        ]
        PUT >=> choose [
            routef "/todos/%i/complete" completeTodoHandler
            routef "/todos/%i/uncomplete" uncompleteTodoHandler
        ]
        DELETE >=> choose [
            routef "/todos/%i" deleteTodoHandler
            route "/todos/completed" >=> clearCompletedHandler
        ]
        setStatusCode 404 >=> text "Not Found"
    ]

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    builder.Services.AddGiraffe() |> ignore
    
    let app = builder.Build()
    
    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore
    
    // Initialize sample data
    initializeSampleData()
    
    app.UseStaticFiles() |> ignore
    app.UseGiraffe(webApp)

    app.Run()

    0 // Exit code


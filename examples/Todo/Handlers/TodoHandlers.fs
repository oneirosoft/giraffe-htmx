module Handlers.TodoHandlers

open System
open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData
open Views.TodoApp
open Views.TodoItem
open Views.TodoList

// Form models
[<CLIMutable>]
type AddTodoForm = { text: string }

let createHandlers layout =
    // For add todo - always returns todoApp since it's always called via HTMX
    let addTodoHandler : HttpHandler =
        fun next ctx ->
            task {
                let! form = ctx.BindFormAsync<AddTodoForm>()
                if not (String.IsNullOrWhiteSpace(form.text)) then
                    addTodo form.text |> ignore
                
                return! htmx layout (fun () -> todoApp All) next ctx
            }

    let completeTodoHandler (id: int) : HttpHandler =
        fun next ctx ->
            toggleTodo id |> ignore
            let todoView = 
                match getTodo id with
                | Some todo -> todoItem todo
                | None -> div [] []
            htmlView todoView next ctx

    let uncompleteTodoHandler (id: int) : HttpHandler =
        fun next ctx ->
            toggleTodo id |> ignore
            let todoView = 
                match getTodo id with
                | Some todo -> todoItem todo
                | None -> div [] []
            htmlView todoView next ctx

    let deleteTodoHandler (id: int) : HttpHandler =
        fun next ctx ->
            deleteTodo id |> ignore
            htmlString "" next ctx

    let clearCompletedHandler : HttpHandler =
        fun next ctx ->
            getAllTodos()
            |> List.filter (fun t -> t.IsCompleted)
            |> List.iter (fun t -> deleteTodo t.Id |> ignore)
            
            htmlView (todoList All) next ctx

    (addTodoHandler, completeTodoHandler, uncompleteTodoHandler, deleteTodoHandler, clearCompletedHandler)

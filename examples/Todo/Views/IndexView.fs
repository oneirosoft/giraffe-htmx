module Views.IndexView

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData
open Views.TodoApp

let indexView filter =
    div [ _class "container" ] [
        div [ _class "header" ] [
            h1 [] [ str "Todo App" ]
            p [] [ str "Built with F#, Giraffe, and HTMX" ]
        ]
        div [ _class "todo-app" ] [
            todoApp filter
        ]
    ]

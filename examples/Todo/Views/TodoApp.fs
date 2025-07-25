module Views.TodoApp

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData
open Views.AddTodoForm
open Views.TodoStats
open Views.TodoList
open Views.FilterTabs

let todoApp filter =
    div [ _id "todo-app" ] [
        addTodoForm
        todoStats filter
        todoList filter
        filterTabs filter
    ]

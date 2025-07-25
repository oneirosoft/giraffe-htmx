module Views.TodoList

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData
open Views.TodoItem

let todoList filter =
    let filteredTodos = getFilteredTodos filter
    
    div [ _class "todo-list"; _id "todo-list" ] [
        if List.isEmpty filteredTodos then
            let (heading, message) = 
                match filter with
                | All -> ("No todos yet", "Add your first todo above to get started!")
                | Active -> ("All done!", "You have no active todos.")
                | Completed -> ("No completed todos", "Complete some todos to see them here.")
            
            div [ _class "empty-state" ] [
                h3 [] [ str heading ]
                p [] [ str message ]
            ]
        else
            yield! filteredTodos |> List.map todoItem
    ]

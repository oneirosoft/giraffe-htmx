module Views.TodoStats

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData

let todoStats filter =
    let allTodos = getAllTodos()
    let totalCount = List.length allTodos
    let activeCount = allTodos |> List.filter (fun t -> not t.IsCompleted) |> List.length
    let completedCount = totalCount - activeCount
    
    div [ _class "todo-stats" ] [
        span [] [ 
            str $"Total: {totalCount} | Active: {activeCount} | Completed: {completedCount}"
        ]
        if completedCount > 0 then
            button [ 
                _class "btn btn-small btn-danger"
                _hxDelete "/todos/completed"
                _hxTarget "#todo-list"
                _hxSwapTyped (HtmxSwap.withTransition OuterHTML)
                _hxConfirm "Are you sure you want to delete all completed todos?"
                _hxTriggerTyped Click
            ] [ str "Clear Completed" ]
    ]

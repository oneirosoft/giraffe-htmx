module Views.TodoItem

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData

let todoItem (todo: Todo) =
    let itemClass = if todo.IsCompleted then "todo-item completed" else "todo-item"
    
    div [ _class itemClass; _id $"todo-{todo.Id}" ] [
        p [ _class "todo-text" ] [ str todo.Text ]
        div [ _class "todo-actions" ] [
            if not todo.IsCompleted then
                button [ 
                    _class "btn btn-small btn-success"
                    _hxPut $"/todos/{todo.Id}/complete"
                    _hxTarget $"#todo-{todo.Id}"
                    _hxSwapTyped (HtmxSwap.withTransition OuterHTML)
                    _title "Mark as completed"
                ] [ str "✓" ]
            else
                button [ 
                    _class "btn btn-small"
                    _hxPut $"/todos/{todo.Id}/uncomplete"
                    _hxTarget $"#todo-{todo.Id}"
                    _hxSwapTyped (HtmxSwap.withTransition OuterHTML)
                    _title "Mark as active"
                ] [ str "↶" ]
            
            button [ 
                _class "btn btn-small btn-danger"
                _hxDelete $"/todos/{todo.Id}"
                _hxTarget $"#todo-{todo.Id}"
                _hxSwapTyped (HtmxSwap.withTransition OuterHTML)
                _hxConfirm "Are you sure you want to delete this todo?"
                _title "Delete todo"
            ] [ str "✕" ]
        ]
    ]

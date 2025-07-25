module Views.AddTodoForm

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let addTodoForm =
    form [ 
        _class "add-todo-form"
        _hxPost "/todos"
        _hxTarget "#todo-app"
        _hxSwapTyped (HtmxSwap.withTransition InnerHTML)
        _hxTriggerTyped Submit
        _hxIndicator ".htmx-indicator"
    ] [
        input [ 
            _class "add-todo-input"
            _type "text"
            _name "text"
            _placeholder "What needs to be done?"
            _required
            _autofocus
        ]
        button [ _class "btn btn-primary"; _type "submit" ] [ 
            str "Add Todo"
            span [ _class "htmx-indicator" ] [ str " ..." ]
        ]
    ]

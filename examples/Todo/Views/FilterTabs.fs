module Views.FilterTabs

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open TodoData

let filterTabs currentFilter =
    let tabClass filter = 
        if filter = currentFilter then "filter-tab active" else "filter-tab"
    
    div [ _class "filter-tabs" ] [
        a [ 
            _class (tabClass All)
            _href "/all"
            _hxGet "/all"
            _hxTarget "#todo-app"
            _hxSwapTyped (HtmxSwap.withTransition InnerHTML)
            _hxTriggerTyped Click
        ] [ str "All" ]
        a [ 
            _class (tabClass Active)
            _href "/active"
            _hxGet "/active"
            _hxTarget "#todo-app"
            _hxSwapTyped (HtmxSwap.withTransition InnerHTML)
            _hxTriggerTyped Click
        ] [ str "Active" ]
        a [ 
            _class (tabClass Completed)
            _href "/completed"
            _hxGet "/completed"
            _hxTarget "#todo-app"
            _hxSwapTyped (HtmxSwap.withTransition InnerHTML)
            _hxTriggerTyped Click
        ] [ str "Completed" ]
    ]

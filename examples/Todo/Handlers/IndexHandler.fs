module Handlers.IndexHandler

open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Views.IndexView
open Views.TodoApp

// We'll need to pass the layout from Program.fs
let createHandlers layout =
    let todoHtmx = htmx layout

    // Handlers - clean composition using htmx handler  
    let indexHandler filter : HttpHandler =
        todoHtmx (fun () -> indexView filter)

    // For filter navigation - returns just todoApp content when called via HTMX
    let filterHandler filter : HttpHandler =
        todoHtmx (fun () -> todoApp filter)
    
    indexHandler, filterHandler

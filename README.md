# Oneiro.Giraffe.ViewEngine.Htmx

A comprehensive F# library that extends Giraffe.ViewEngine with type-safe HTMX attributes and handlers, enabling you to build modern, interactive web applications with minimal JavaScript.

## Features

- 🔒 **Type-safe HTMX attributes** - Strongly-typed alternatives to string-based HTMX attributes
- 🚀 **Smart HTTP handlers** - Automatically adapt responses for HTMX vs full page requests
- 🎨 **Layout builders** - Fluent API for creating HTMX-enabled HTML layouts
- 📡 **Request introspection** - Easy access to HTMX request headers and context
- 🔧 **Response control** - Set HTMX response headers for client-side behavior
- 📖 **Comprehensive documentation** - Detailed XML docs for all public functions

## Installation

```bash
dotnet add package Oneiro.Giraffe.ViewEngine.Htmx
```

## Quick Start

### 1. Basic Setup

```fsharp
open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx.Attributes
open Giraffe.ViewEngine.Htmx.Layouts
open Giraffe.ViewEngine.Htmx.Handlers

// Create an HTMX-enabled layout
let appLayout = htmxLayout {
    title "My HTMX App"
    version V2_0_6
    styles [
        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
        "https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css"
    ]
}

// Create a partial handler that adapts to HTMX context
let homeHandler = htmx appLayout (fun () ->
    div [ _class "container mt-4" ] [
        h1 [] [ str "Welcome to HTMX with F#!" ]
        p [] [ str "This content automatically adapts for HTMX requests." ]
    ])
```

### 2. Configure Your Giraffe App

```fsharp
let webApp =
    choose [
        GET >=> route "/" >=> homeHandler
        // Add more routes here
    ]

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseGiraffe(webApp)
                |> ignore)
        .Build()
        .Run()
    0
```

## Core Concepts

### HTMX Handlers

The `htmx` handler automatically detects whether a request comes from HTMX or a full page load:

```fsharp
// For HTMX requests: returns only the content fragment
// For full page requests: wraps content in the provided layout
let myHandler = htmx layout (fun () ->
    div [] [ str "Dynamic content" ]
)

// Using currying for reusable layouts
let withMainLayout = htmx appLayout
let homeHandler = withMainLayout (fun () -> homeView())
let aboutHandler = withMainLayout (fun () -> aboutView())
```

### Type-Safe HTMX Attributes

Replace string-based HTMX attributes with strongly-typed alternatives:

```fsharp
// String-based (traditional)
button [ _hxPost "/api/users"; _hxTarget "#result" ] [ str "Create User" ]

// Type-safe alternative
button [ 
    _hxPost "/api/users"
    _hxTarget "#result"
    _hxTriggerTyped (HtmxTrigger.click |> HtmxTrigger.once)
    _hxSwapTyped (HtmxSwap.innerHTML |> HtmxSwap.withTransition)
] [ str "Create User" ]
```

## Examples

### Interactive Todo List

```fsharp
type Todo = { Id: int; Text: string; Completed: bool }

let mutable todos = [
    { Id = 1; Text = "Learn F#"; Completed = true }
    { Id = 2; Text = "Build HTMX app"; Completed = false }
    { Id = 3; Text = "Deploy to production"; Completed = false }
]

// Layout with custom styling
let todoLayout = htmxLayout {
    title "F# HTMX Todo App"
    styles [
        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
    ]
    head [
        style [] [
            rawText """
            .todo-item { transition: all 0.3s ease; }
            .todo-item.completed { opacity: 0.6; text-decoration: line-through; }
            .htmx-indicator { opacity: 0; transition: opacity 0.3s; }
            .htmx-request .htmx-indicator { opacity: 1; }
            """
        ]
    ]
}

// Todo item component
let todoItem todo =
    div [ 
        _class "todo-item d-flex align-items-center p-2 border-bottom"
        _id $"todo-{todo.Id}"
    ] [
        input [
            _type "checkbox"
            _class "form-check-input me-2"
            _checked todo.Completed
            _hxPut $"/todos/{todo.Id}/toggle"
            _hxTarget $"#todo-{todo.Id}"
            _hxSwap "outerHTML"
        ]
        span [ 
            _class (if todo.Completed then "completed" else "")
        ] [ str todo.Text ]
        
        button [
            _class "btn btn-sm btn-outline-danger ms-auto"
            _hxDelete $"/todos/{todo.Id}"
            _hxTarget $"#todo-{todo.Id}"
            _hxSwap "outerHTML"
            _hxConfirm "Are you sure you want to delete this todo?"
        ] [
            i [ _class "bi bi-trash" ] []
        ]
    ]

// Todo list view
let todoListView () =
    div [ _id "todo-list" ] [
        yield! todos |> List.map todoItem
    ]

// Add todo form
let addTodoForm () =
    form [
        _hxPost "/todos"
        _hxTarget "#todo-list"
        _hxSwap "beforeend"
        _class "mb-4"
    ] [
        div [ _class "input-group" ] [
            input [
                _type "text"
                _name "text"
                _class "form-control"
                _placeholder "Add a new todo..."
                _required
            ]
            button [
                _type "submit"
                _class "btn btn-primary"
            ] [
                span [ _class "htmx-indicator spinner-border spinner-border-sm me-2" ] []
                str "Add Todo"
            ]
        ]
    ]

// Main todo page
let todoPage () =
    div [ _class "container mt-4" ] [
        h1 [ _class "mb-4" ] [ str "📝 F# HTMX Todo App" ]
        addTodoForm()
        todoListView()
    ]

// Handlers using curried layout
let withTodoLayout = htmx todoLayout

let todoHandlers = [
    GET >=> route "/" >=> withTodoLayout todoPage
    
    POST >=> route "/todos" >=> fun next ctx -> task {
        let! form = ctx.BindFormAsync<{| text: string |}>()
        let newTodo = { 
            Id = (todos |> List.maxBy (_.Id)).Id + 1
            Text = form.text
            Completed = false 
        }
        todos <- todos @ [newTodo]
        return! htmlView (todoItem newTodo) next ctx
    }
    
    PUT >=> routef "/todos/%d/toggle" (fun id -> fun next ctx -> task {
        todos <- todos |> List.map (fun t -> 
            if t.Id = id then { t with Completed = not t.Completed } else t)
        let todo = todos |> List.find (fun t -> t.Id = id)
        return! htmlView (todoItem todo) next ctx
    })
    
    DELETE >=> routef "/todos/%d" (fun id -> fun next ctx -> task {
        todos <- todos |> List.filter (fun t -> t.Id <> id)
        return! text "" next ctx
    })
]
```

### Advanced Form with Validation

```fsharp
type UserForm = {
    Name: string
    Email: string
    Age: int option
}

let userFormView (form: UserForm option) (errors: string list) =
    let form = defaultArg form { Name = ""; Email = ""; Age = None }
    
    div [ _class "row justify-content-center" ] [
        div [ _class "col-md-6" ] [
            h2 [] [ str "User Registration" ]
            
            // Error display
            if not (List.isEmpty errors) then
                div [ _class "alert alert-danger" ] [
                    ul [ _class "mb-0" ] [
                        yield! errors |> List.map (fun error ->
                            li [] [ str error ]
                        )
                    ]
                ]
            
            form [
                _hxPost "/users/validate"
                _hxTarget "#form-container"
                _hxSwap "outerHTML"
                _class "needs-validation"
                _novalidate
            ] [
                div [ _class "mb-3" ] [
                    label [ _for "name"; _class "form-label" ] [ str "Name" ]
                    input [
                        _type "text"
                        _id "name"
                        _name "name"
                        _class "form-control"
                        _value form.Name
                        _required
                        _hxPost "/users/validate-field"
                        _hxTrigger "blur"
                        _hxTarget "#name-feedback"
                    ]
                    div [ _id "name-feedback"; _class "invalid-feedback" ] []
                ]
                
                div [ _class "mb-3" ] [
                    label [ _for "email"; _class "form-label" ] [ str "Email" ]
                    input [
                        _type "email"
                        _id "email"
                        _name "email"
                        _class "form-control"
                        _value form.Email
                        _required
                    ]
                ]
                
                div [ _class "mb-3" ] [
                    label [ _for "age"; _class "form-label" ] [ str "Age (optional)" ]
                    input [
                        _type "number"
                        _id "age"
                        _name "age"
                        _class "form-control"
                        _min "1"
                        _max "120"
                        match form.Age with
                        | Some age -> _value (string age)
                        | None -> ()
                    ]
                ]
                
                button [
                    _type "submit"
                    _class "btn btn-primary"
                ] [
                    span [ _class "htmx-indicator spinner-border spinner-border-sm me-2" ] []
                    str "Register"
                ]
            ]
        ]
    ]
```

### Real-time Updates with Server-Sent Events

```fsharp
let dashboardView () =
    div [ _class "container mt-4" ] [
        h1 [] [ str "📊 Real-time Dashboard" ]
        
        // Auto-updating metrics
        div [
            _id "metrics"
            _hxGet "/api/metrics"
            _hxTrigger "every 2s"
            _hxSwap "innerHTML"
        ] [
            str "Loading metrics..."
        ]
        
        // Live notifications
        div [
            _id "notifications"
            _hxExt "sse"
            _hxSse "connect:/events"
        ] [
            div [ 
                _hxSse "swap:notification"
                _hxSwap "afterbegin"
            ] []
        ]
        
        // Interactive chart that updates on click
        div [ _class "mt-4" ] [
            canvas [
                _id "chart"
                _hxGet "/api/chart-data"
                _hxTrigger "click from:body"
                _hxTarget "this"
                _hxSwap "outerHTML"
            ] []
        ]
    ]
```

### Type-Safe Trigger Compositions

```fsharp
open Giraffe.ViewEngine.Htmx.Attributes.HtmxTrigger
open Giraffe.ViewEngine.Htmx.Attributes.HtmxSwap

// Complex trigger with multiple modifiers
let searchInput =
    input [
        _type "text"
        _name "query"
        _class "form-control"
        _placeholder "Search..."
        _hxGet "/search"
        _hxTarget "#results"
        _hxTriggerTyped (
            keyup 
            |> withKey "Enter"
            |> delay 300
            |> throttle 500
        )
        _hxSwapTyped (
            innerHTML 
            |> withTransition
            |> withSwapDelay 100
            |> withScroll "top"
        )
    ]

// Multiple triggers
let advancedButton =
    button [
        _class "btn btn-primary"
        _hxPost "/api/action"
        _hxTriggerTyped (
            // Trigger on click OR Enter key
            click |> once  // Only fire once
        )
        _hxConfirm "Are you sure?"
    ] [ str "Advanced Action" ]
```

## Advanced Features

### Request Context Access

Access HTMX-specific request information:

```fsharp
let smartHandler: HttpHandler = fun next ctx ->
    if ctx.IsHtmxRequest() then
        // HTMX request - return fragment
        let target = ctx.HtmxTarget() |> Option.defaultValue "unknown"
        let trigger = ctx.HtmxTrigger() |> Option.defaultValue "unknown"
        
        htmlView (
            div [] [
                str $"HTMX Request - Target: {target}, Trigger: {trigger}"
            ]
        ) next ctx
    else
        // Full page request
        htmlView (appLayout [
            h1 [] [ str "Full Page" ]
        ]) next ctx
```

### Response Headers

Control client-side behavior with response headers:

```fsharp
let actionHandler: HttpHandler = fun next ctx -> task {
    // Perform some action
    
    // Set HTMX response headers
    ctx.SetHtmxTrigger("refreshData") |> ignore
    ctx.SetHtmxPushUrl("/new-url") |> ignore
    
    return! htmlView (
        div [ _class "alert alert-success" ] [
            str "Action completed successfully!"
        ]
    ) next ctx
}
```

### Custom Layout Configurations

```fsharp
// Development layout with debugging tools
let devLayout = htmxLayout {
    title "Dev Mode - My App"
    version V2_0_6
    styles [
        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
        "/css/dev-tools.css"
    ]
    scripts [
        script [ _src "/js/dev-tools.js" ] []
    ]
    head [
        meta [ _name "environment"; _content "development" ]
    ]
    bodyAttr [ _class "dev-mode"; attr "data-debug" "true" ]
}

// Production layout optimized for performance
let prodLayout = htmxLayout {
    title "My Production App"
    version V2_0_6
    styles [
        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
        "/css/app.min.css"
    ]
    head [
        meta [ _name "description"; _content "Fast, modern web app built with F# and HTMX" ]
        link [ _rel "icon"; _href "/favicon.ico" ]
    ]
}
```

## API Reference

### Core Attributes

| Function | Description | Example |
|----------|-------------|---------|
| `_hxGet` | HTTP GET request | `_hxGet "/api/data"` |
| `_hxPost` | HTTP POST request | `_hxPost "/api/create"` |
| `_hxPut` | HTTP PUT request | `_hxPut "/api/update"` |
| `_hxDelete` | HTTP DELETE request | `_hxDelete "/api/delete"` |
| `_hxTarget` | Target element | `_hxTarget "#results"` |
| `_hxSwap` | Swap strategy | `_hxSwap "innerHTML"` |
| `_hxTrigger` | Trigger event | `_hxTrigger "click"` |

### Type-Safe Alternatives

| Function | Description | Type |
|----------|-------------|------|
| `_hxTriggerTyped` | Type-safe triggers | `HtmxTrigger` |
| `_hxSwapTyped` | Type-safe swaps | `HtmxSwap` |

### Context Extensions

| Method | Description | Return Type |
|--------|-------------|-------------|
| `IsHtmxRequest()` | Check if HTMX request | `bool` |
| `HtmxTarget()` | Get target element | `string option` |
| `HtmxTrigger()` | Get trigger element | `string option` |
| `SetHtmxRedirect()` | Set redirect header | `HttpContext` |
| `SetHtmxPushUrl()` | Update browser URL | `HttpContext` |

## Best Practices

### 1. Use Curried Layouts
```fsharp
let withMainLayout = htmx mainLayout
let withApiLayout = htmx apiLayout

// Clean, reusable handlers
let homeHandler = withMainLayout (fun () -> homeView())
let profileHandler = withMainLayout (fun () -> profileView())
```

### 2. Organize by Feature
```fsharp
module Users =
    let private withLayout = htmx userLayout
    
    let listHandler = withLayout (fun () -> userListView())
    let detailHandler id = withLayout (fun () -> userDetailView id)
    let createHandler = withLayout (fun () -> userCreateView())
```

### 3. Type-Safe Compositions
```fsharp
// Prefer type-safe builders for complex scenarios
let complexTrigger = 
    HtmxTrigger.keyup
    |> HtmxTrigger.withKey "Enter"
    |> HtmxTrigger.delay 300
    |> HtmxTrigger.once

button [ _hxTriggerTyped complexTrigger ] [ str "Submit" ]
```

### 4. Progressive Enhancement
```fsharp
// Always provide fallbacks
form [ 
    _action "/users"  // Works without JavaScript
    _method "POST"
    _hxPost "/users"  // Enhanced with HTMX
    _hxTarget "#result"
] [
    (* form content *)
]
```

## Contributing

Contributions are welcome! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [Giraffe](https://github.com/giraffe-fsharp/Giraffe) - F# web framework
- [HTMX](https://htmx.org/) - High power tools for HTML
- [Giraffe.ViewEngine](https://github.com/giraffe-fsharp/Giraffe.ViewEngine) - F# HTML DSL

---

Built with ❤️ by the F# community

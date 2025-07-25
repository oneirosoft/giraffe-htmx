module TodoData

open System
open System.Collections.Concurrent

// Domain model
type TodoId = int

type Todo = {
    Id: TodoId
    Text: string
    IsCompleted: bool
    CreatedAt: DateTime
}

type TodoFilter = All | Active | Completed

// In-memory storage
let private todos = ConcurrentDictionary<TodoId, Todo>()
let mutable private nextId = 1

// Helper functions
let getAllTodos () = 
    todos.Values |> Seq.sortBy (fun t -> t.CreatedAt) |> Seq.toList

let getFilteredTodos = function
    | All -> getAllTodos()
    | Active -> getAllTodos() |> List.filter (fun t -> not t.IsCompleted)
    | Completed -> getAllTodos() |> List.filter (fun t -> t.IsCompleted)

let addTodo text =
    let id = System.Threading.Interlocked.Increment(&nextId)
    let todo = { Id = id; Text = text; IsCompleted = false; CreatedAt = DateTime.Now }
    todos.[id] <- todo
    todo

let toggleTodo id =
    match todos.TryGetValue(id) with
    | true, todo -> 
        let updated = { todo with IsCompleted = not todo.IsCompleted }
        todos.[id] <- updated
        Some updated
    | false, _ -> None

let deleteTodo id =
    let (removed, _) = todos.TryRemove(id)
    removed

let getTodo id =
    match todos.TryGetValue(id) with
    | true, todo -> Some todo
    | false, _ -> None

let parseFilter = function
    | "active" -> Active
    | "completed" -> Completed
    | _ -> All

// Initialize with sample data
let initializeSampleData () =
    [
        "Learn F# and functional programming"
        "Build a web app with Giraffe"
        "Explore HTMX for dynamic interactions"
        "Write comprehensive tests"
        "Deploy to production"
    ]
    |> List.iter (fun text -> addTodo text |> ignore)
    
    // Mark the first two as completed
    toggleTodo 2 |> ignore
    toggleTodo 3 |> ignore

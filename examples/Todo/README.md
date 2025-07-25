# Todo App Example

This is a complete todo application example demonstrating the usage of the Oneiro.Giraffe.ViewEngine.Htmx library with F#, Giraffe, and HTMX.

## Features

- ✅ Add new todos
- ✅ Mark todos as complete/incomplete
- ✅ Delete individual todos
- ✅ Filter todos (All, Active, Completed)
- ✅ Clear all completed todos
- ✅ Responsive design with modern CSS
- ✅ Smooth HTMX transitions
- ✅ Type-safe HTMX attributes using the library

## Library Usage

This example showcases the proper usage of the Oneiro.Giraffe.ViewEngine.Htmx library:

### HTMX Layout Builder

```fsharp
let layout =
    htmxLayout {
        title "Todo App - HTMX + Giraffe"
        version V2_0_6
        styles [ "/style.css" ]
        scripts [ /* custom scripts */ ]
    }
```

### HTMX Handler

The `htmx` handler automatically detects HTMX requests and returns either partial content for HTMX requests or the full page for regular requests:

```fsharp
let indexHandler filter : HttpHandler =
    htmx layout (indexView filter)
```

### Type-Safe HTMX Attributes

Instead of using string-based HTMX attributes, the library provides type-safe alternatives:

```fsharp
// Type-safe swap types
_hxSwapTyped (HtmxSwap.withTransition OuterHTML)

// Type-safe trigger types
_hxTriggerTyped Click

// Composable trigger modifiers
_hxTriggerTyped (HtmxTrigger.withKey "Enter" KeyUp)
```

### HTMX Swap Modifiers

The library provides fluent builders for complex swap behaviors:

```fsharp
// Add transitions to swaps
_hxSwapTyped (HtmxSwap.withTransition InnerHTML)

// Combine multiple modifiers
_hxSwapTyped (HtmxSwap.withModifiers [Transition true; Settle 200] OuterHTML)
```

## Running the Application

1. Build the application:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser to `https://localhost:5001` or `http://localhost:5000`

## Architecture

- **Domain Model**: Simple `Todo` record with `Id`, `Text`, `IsCompleted`, and `CreatedAt`
- **Storage**: In-memory `ConcurrentDictionary` (replace with database in production)
- **Views**: Functional view composition using Giraffe.ViewEngine
- **Handlers**: Type-safe handlers using the `htmx` helper
- **Routing**: Clean RESTful routes with Giraffe

## HTMX Features Demonstrated

- **Progressive Enhancement**: Works with and without JavaScript
- **Partial Updates**: Only updates necessary DOM elements
- **Form Handling**: Auto-submit forms with validation
- **Filtering**: Dynamic content filtering without page reloads
- **Confirmations**: Built-in confirmation dialogs
- **Loading Indicators**: Visual feedback during requests
- **Transitions**: Smooth animations between state changes

## CSS Features

- **Modern Design**: Clean, professional interface
- **Responsive Layout**: Works on all device sizes
- **CSS Custom Properties**: Consistent theming
- **Accessibility**: Proper focus states and semantic HTML
- **Smooth Animations**: CSS transitions for state changes

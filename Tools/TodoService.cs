using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

public static class TodoService
{
    public static void MapEndPoints(this IEndpointRouteBuilder app)
    {        
        app.MapGroup("/todoitems")
            .WithTags("Todo Items")
            .WithSummary("Manage Todo Items")
            .WithDescription("APIs for managing todo items.");            

        app.MapGet("/", TodoTools.GetAllTodos)
            .WithSummary("Get All Todo Items")
            .WithDescription("Retrieve all todo items, optionally filtered by status query parameter (e.g., ?status=completed).");

        app.MapPost("/complete", TodoTools.CompleteTodoItem)
            .WithSummary("Complete Todo Item")
            .WithDescription("Mark a todo item as complete by its ID.");

        app.MapGet("/completed", TodoTools.GetCompletedTodos)
            .WithSummary("Get Completed Todo Items")
            .WithDescription("Retrieve all completed todo items.");

        app.MapGet("/{id}", TodoTools.GetTodoById)
            .WithSummary("Get Todo Item by ID")
            .WithDescription("Retrieve a todo item by its ID.");

        app.MapGet("/ids", TodoTools.GetTodosByIds)
            .WithSummary("Get Todo Items by IDs")
            .WithDescription("Retrieve multiple todo items by their IDs.");

        app.MapGet("/search/{name}", TodoTools.SearchTodosByName)
            .WithSummary("Search Todo Items by Name")
            .WithDescription("Search todo items by name.");

        app.MapPost("/", TodoTools.AddTodoItem)
            .WithSummary("Add Todo Item")
            .WithDescription("Add a new todo item.");

        app.MapPatch("/{id}", TodoTools.UpdateTodoItem)
            .WithSummary("Update Todo Item")
            .WithDescription("Update an existing todo item by id.");

        // POST /batch
        app.MapPost("/batch", TodoTools.BatchUpdateTodoItems)
            .WithSummary("Batch Update Todo Items")
            .WithDescription("Batch update todo items.");

        app.MapDelete("/{id}", TodoTools.DeleteTodoItem)
            .WithSummary("Delete Todo Item")
            .WithDescription("Delete a todo item by its ID.");

        app.MapGet("/exists/{id}", TodoTools.TodoExists)
            .WithSummary("Check if Todo Item Exists")
            .WithDescription("Check if a todo item exists by its ID.");
    }
}


/// <summary>
/// Todo Tools for managing todo items over MCP endpoint.
/// </summary>
[McpServerToolType]
public static class TodoTools
{
    /// <summary>
    /// Get all todo items
    /// </summary>
    /// <param name="status"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    [McpServerTool(Name = "get_all_todos"), Description("Retrieve all todo items optionally filtered by status.")]
    public static async Task<IResult> GetAllTodos(string? status, TodoDb db)
    {
        if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
        }
        else
        {
            return TypedResults.Ok(await db.Todos.ToListAsync());
        }
    }

    // Get a todo item by ID
    [McpServerTool(Name = "get_todo_by_id"), Description("Retrieve a todo item by its ID.")]
    public static async Task<IResult> GetTodoById(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);
        return todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound();
    }

    // Get todos by multiple IDs
    [McpServerTool(Name = "get_todos_by_ids"), Description("Retrieve multiple todo items by their IDs.")]
    public static async Task<IResult> GetTodosByIds(int[] ids, TodoDb db)
    {
        var todos = await db.Todos.Where(t => ids.Contains(t.Id)).ToListAsync();
        return TypedResults.Ok(todos);
    }

    // Search todos by name
    [McpServerTool(Name = "search_todos_by_name"), Description("Search todo items by name.")]
    public static async Task<IResult> SearchTodosByName(string name, TodoDb db)
    {
        var todos = await db.Todos
            .Where(t => t.Name != null && t.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();
        return TypedResults.Ok(todos);
    }

    // Add a new todo item
    [McpServerTool(Name = "add_todo_item"), Description("Add a new todo item.")]
    public static async Task<IResult> AddTodoItem(Todo todo, TodoDb db)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return TypedResults.Ok(todo);
    }

    // Update an existing todo item
    [McpServerTool(Name = "update_todo_item"), Description("Update an existing todo item by id.")]
    public static async Task<IResult> UpdateTodoItem(int id, Todo inputTodo, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();
        todo.Name = inputTodo.Name;
        todo.IsComplete = inputTodo.IsComplete;
        todo.Tag = inputTodo.Tag;

        await db.SaveChangesAsync();

        return TypedResults.Ok(todo);
    }

    // Batch update todo items
    [McpServerTool(Name = "batch_update_todo_items"), Description("Batch update todo items.")]
    public static async Task<IResult> BatchUpdateTodoItems(Todo[] todos, TodoDb db)
    {
        var updatedTodos = new List<Todo>();

        foreach (var inputTodo in todos)
        {
            var todo = await db.Todos.FindAsync(inputTodo.Id);
            if (todo != null)
            {
                todo.Name = inputTodo.Name;
                todo.IsComplete = inputTodo.IsComplete;
                updatedTodos.Add(todo);
            }
        }

        await db.SaveChangesAsync();
        return TypedResults.Ok(updatedTodos);
    }

    // Delete a todo item by ID
    [McpServerTool(Name = "delete_todo_item"), Description("Delete a todo item by its ID.")]
    public static async Task<IResult> DeleteTodoItem(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo != null)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return TypedResults.Ok(todo);
        }
        return TypedResults.NotFound();
    }

    // Get completed todo items
    [McpServerTool(Name = "get_completed_todos"), Description("Retrieve all completed todo items.")]
    public static async Task<IResult> GetCompletedTodos(TodoDb db)
    {
        var todos = await db.Todos.Where(t => t.IsComplete).ToListAsync();
        return TypedResults.Ok(todos);
    }

    // Complete a todo item by ID
    [McpServerTool(Name = "complete_todo_item"), Description("Mark a todo item as complete by its ID.")]
    public static async Task<IResult> CompleteTodoItem(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo != null)
        {
            todo.IsComplete = true;
            await db.SaveChangesAsync();
            return TypedResults.Ok(todo);
        }
        return TypedResults.NotFound();
    }

    // Helper method to check if a Todo item exists    
    [McpServerTool(Name = "todo_exists"), Description("Check if a todo item exists by its ID.")]
    public static async Task<IResult> TodoExists(int id, TodoDb db)
    {
        var exists = await db.Todos.AnyAsync(e => e.Id == id);
        return TypedResults.Ok(exists);
    }
}

using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

public static class TodoService
{
    /// <summary>
    /// Map the endpoints for managing todo items.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/todoitems")
            .WithTags("Todo Items")
            .WithSummary("Manage Todo Items")
            .WithDescription("APIs for managing todo items.");

        app.MapGet("/", TodoTools.GetAllTodos)
            .WithSummary("Get All Todo Items")
            .WithDescription("Retrieve all todo items, optionally filtered by status query parameter (e.g., ?status=completed).");

        app.MapPost("/{id}/changestatus", TodoTools.ChangeStatusTodoItem)
            .WithSummary("Change Todo Item Status")
            .WithDescription("Change the status of a todo item by its ID with optional isComplete parameter (default is true).");

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

        app.MapPost("/add", TodoTools.AddTodoItem)
            .WithSummary("Add Todo Item")
            .WithDescription("Add a new todo item.");

        app.MapPatch("/update/{id}", TodoTools.UpdateTodoItem)
            .WithSummary("Update Todo Item")
            .WithDescription("Update an existing todo item by id.");

        // POST /batch
        app.MapPost("/batchupdate", TodoTools.BatchUpdateTodoItems)
            .WithSummary("Batch Update Todo Items")
            .WithDescription("Batch update todo items.");

        app.MapDelete("/delete/{id}", TodoTools.DeleteTodoItem)
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
    /// Get all todo items optionally filtered by status.
    /// </summary>
    /// <param name="status">The status to filter by (e.g., "completed").</param>
    /// <param name="db">The database context.</param>
    /// <returns>A list of todo items.</returns>
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

    /// <summary>
    /// Get a todo item by its ID.
    /// </summary>
    /// <param name="id">The ID of the todo item.</param>
    /// <param name="db">The database context.</param>
    /// <returns>The todo item if found; otherwise, a not found result.</returns>    
    [McpServerTool(Name = "get_todo_by_id"), Description("Retrieve a todo item by its ID.")]
    public static async Task<IResult> GetTodoById(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);
        return todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound();
    }

    /// <summary>
    /// Get todo items by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the todo items.</param>
    /// <param name="db">The database context.</param>
    /// <returns>A list of todo items.</returns>
    [McpServerTool(Name = "get_todos_by_ids"), Description("Retrieve multiple todo items by their IDs.")]
    public static async Task<IResult> GetTodosByIds(int[] ids, TodoDb db)
    {
        var todos = await db.Todos.Where(t => ids.Contains(t.Id)).ToListAsync();
        return TypedResults.Ok(todos);
    }

    /// <summary>
    /// Search todo items by name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="db">The database context.</param>
    /// <returns>A list of todo items that match the search criteria.</returns>
    [McpServerTool(Name = "search_todos_by_name"), Description("Search todo items by name.")]
    public static async Task<IResult> SearchTodosByName(string name, TodoDb db)
    {
        var todos = await db.Todos
            .Where(t => t.Name != null && t.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();
        return TypedResults.Ok(todos);
    }

    /// <summary>
    /// Get all completed todo items.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <returns>A list of completed todo items.</returns>
    [McpServerTool(Name = "get_completed_todos"), Description("Retrieve all completed todo items.")]
    public static async Task<IResult> GetCompletedTodos(TodoDb db)
    {
        var todos = await db.Todos.Where(t => t.IsComplete).ToListAsync();
        return TypedResults.Ok(todos);
    }

    /// <summary>
    /// Change the isComplete status of a todo item by its ID with optional isComplete parameter (default is true). For example mark them completed or incomplete.
    /// </summary>
    /// <param name="id">The ID of the todo item.</param>
    /// <param name="db">The database context.</param>
    /// <param name="isComplete">The new isComplete status (default is true).</param>
    /// <returns>The updated todo item if found; otherwise, a not found result.</returns>
    [McpServerTool(Name = "change_iscomplete_status_todo_item"), Description("Change the iscompplete status of a todo item by its ID with optional isComplete parameter (default is true). For example mark them completed or incomplete.")]
    public static async Task<IResult> ChangeStatusTodoItem(int id, TodoDb db, bool isComplete = true)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo != null)
        {
            todo.IsComplete = isComplete;
            await db.SaveChangesAsync();
            return TypedResults.Ok(todo);
        }
        return TypedResults.NotFound();
    }

    /// <summary>
    /// Helper method to check if a Todo item exists by its ID.
    /// </summary>
    /// <param name="id">The ID of the todo item.</param>
    /// <param name="db">The database context.</param>
    /// <returns>True if the todo item exists; otherwise, false.</returns>
    [McpServerTool(Name = "todo_exists"), Description("Check if a todo item exists by its ID.")]
    public static async Task<IResult> TodoExists(int id, TodoDb db)
    {
        var exists = await db.Todos.AnyAsync(e => e.Id == id);
        return TypedResults.Ok(exists);
    }

    /// <summary>
    /// Add a new todo item.
    /// </summary>
    /// <param name="todo">The todo item to add.</param>
    /// <param name="db">The database context.</param>
    /// <returns>The added todo item.</returns>
    [McpServerTool(Name = "add_todo_item"), Description("Add a new todo item.")]
    public static async Task<IResult> AddTodoItem(Todo todo, TodoDb db)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return TypedResults.Ok(todo);
    }

    /// <summary>
    /// Update an existing todo item by its ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to update.</param>
    /// <param name="inputTodo">The updated todo item data.</param>
    /// <param name="db">The database context.</param>
    /// <returns>The updated todo item if found; otherwise, a not found result.</returns>
    [McpServerTool(Name = "update_todo_item"), Description("Update an existing todo item by id.")]
    public static async Task<IResult> UpdateTodoItem(int id, Todo inputTodo, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();
        todo.Name = inputTodo.Name ?? todo.Name;
        todo.Tag = inputTodo.Tag ?? todo.Tag;

        await db.SaveChangesAsync();

        return TypedResults.Ok(todo);
    }

    /// <summary>
    /// Batch update todo items.
    /// </summary>
    /// <param name="todos">The array of todo items to update.</param>
    /// <param name="db">The database context.</param>
    /// <returns>The updated todo items.</returns>
    [McpServerTool(Name = "batch_update_todo_items"), Description("Batch update todo items.")]
    public static async Task<IResult> BatchUpdateTodoItems(Todo[] todos, TodoDb db)
    {
        //var updatedTodos = new List<Todo>();

        foreach (var inputTodo in todos)
        {
            var todo = await db.Todos.FindAsync(inputTodo.Id);
            if (todo != null)
            {
                todo.Name = inputTodo.Name ?? todo.Name;
                todo.Tag = inputTodo.Tag ?? todo.Tag;
                db.Update(todo);
            }
        }

        await db.SaveChangesAsync();
        return TypedResults.Ok(todos);
    }

    /// <summary>
    /// Delete a todo item by its ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to delete.</param>
    /// <param name="db">The database context.</param>
    /// <returns>The deleted todo item if found; otherwise, a not found result.</returns>
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
}

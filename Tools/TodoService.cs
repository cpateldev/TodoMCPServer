using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using CoreWebAPI;

public static class TodoService
{

    public static IEndpointRouteBuilder MapEndPoints(this IEndpointRouteBuilder app)
    {
        // Register additional endpoints here
        /*
        var endpointDefinitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => Activator.CreateInstance(t) as IEndpoint)
            .Cast<IEndpoint>();
        */
        app.MapGroup("/todoitems");

        app.MapGet("/", TodoTools.GetAllTodos);

        app.MapPost("/complete", TodoTools.CompleteTodoItem);

        app.MapGet("/completed", TodoTools.GetCompletedTodos);
        app.MapGet("/{id}", TodoTools.GetTodoById);
        app.MapPost("/ids", TodoTools.GetTodosByIds);
        app.MapGet("/search/{name}", TodoTools.SearchTodosByName);

        app.MapPost("/", TodoTools.AddTodoItem);
        app.MapPatch("/{id}", TodoTools.UpdateTodoItem);
        // POST /batch
        app.MapPost("/batch", TodoTools.BatchUpdateTodoItems);
        app.MapDelete("/{id}", TodoTools.DeleteTodoItem);
        app.MapGet("/exists/{id}", TodoTools.TodoExists);

        return app;
    }
}

[McpServerToolType]
public static class TodoTools
{
    // Get all todo items
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

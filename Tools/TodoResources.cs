using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// MCP Resources exposing todo data as URI-addressable read-only endpoints.
/// </summary>
[McpServerResourceType]
public static class TodoResources
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Returns all todo items as a JSON resource.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON array of all todo items.</returns>
    [McpServerResource(UriTemplate = "todo://all", Name = "all_todos", MimeType = "application/json"),
     Description("All todo items in the system.")]
    public static async Task<string> GetAllTodos(TodoDb db, CancellationToken cancellationToken)
    {
        var todos = await db.Todos.ToListAsync(cancellationToken);
        return JsonSerializer.Serialize(todos.Select(t => new TodoItemDTO(t)), JsonOptions);
    }

    /// <summary>
    /// Returns only pending (incomplete) todo items as a JSON resource.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON array of pending todo items.</returns>
    [McpServerResource(UriTemplate = "todo://pending", Name = "pending_todos", MimeType = "application/json"),
     Description("Pending (incomplete) todo items.")]
    public static async Task<string> GetPendingTodos(TodoDb db, CancellationToken cancellationToken)
    {
        var todos = await db.Todos.Where(t => !t.IsComplete).ToListAsync(cancellationToken);
        return JsonSerializer.Serialize(todos.Select(t => new TodoItemDTO(t)), JsonOptions);
    }

    /// <summary>
    /// Returns summary statistics about the todo list.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plain text summary of todo counts.</returns>
    [McpServerResource(UriTemplate = "todo://stats", Name = "todo_stats", MimeType = "text/plain"),
     Description("Summary statistics: total, completed, and pending counts.")]
    public static async Task<string> GetStats(TodoDb db, CancellationToken cancellationToken)
    {
        var total = await db.Todos.CountAsync(cancellationToken);
        var completed = await db.Todos.CountAsync(t => t.IsComplete, cancellationToken);
        var pending = total - completed;
        return $"Total: {total}, Completed: {completed}, Pending: {pending}";
    }

    /// <summary>
    /// Returns a single todo item by ID as a JSON resource (URI template).
    /// </summary>
    /// <param name="id">The ID from the URI template.</param>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON representation of the todo item, or a not-found message.</returns>
    [McpServerResource(UriTemplate = "todo://item/{id}", Name = "todo_by_id", MimeType = "application/json"),
     Description("A single todo item by its ID.")]
    public static async Task<string> GetTodoById(int id, TodoDb db, CancellationToken cancellationToken)
    {
        var todo = await db.Todos.FindAsync([id], cancellationToken);
        return todo is not null
            ? JsonSerializer.Serialize(new TodoItemDTO(todo), JsonOptions)
            : JsonSerializer.Serialize(new { error = "Todo not found", id }, JsonOptions);
    }
}

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// MCP App service for interactive Todo Grid widget.
/// Provides both a tool to fetch todos data and a resource serving the UI.
/// </summary>
[McpServerToolType]
public static class TodoAppTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// MCP Tool that returns all todos as JSON formatted for the grid widget.
    /// Use this tool to fetch data that will be displayed in the interactive UI.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON object with todos array and statistics.</returns>
    [McpServerTool(Name = "get_todos_grid_data")]
    [Description("Fetch all todos with statistics for display in the interactive grid widget. Returns JSON with todos array, counts, and timestamp.")]
    public static async Task<string> GetTodosGridData(
        TodoDb db,
        CancellationToken cancellationToken)
    {
        var todos = await db.Todos.ToListAsync(cancellationToken);

        var todoData = todos.Select(t => new
        {
            Id = t.Id,
            Name = t.Name ?? "Untitled",
            IsComplete = t.IsComplete,
            Status = t.IsComplete ? "completed" : "pending"
        });

        return JsonSerializer.Serialize(new
        {
            Todos = todoData,
            TotalCount = todos.Count,
            CompletedCount = todos.Count(t => t.IsComplete),
            PendingCount = todos.Count(t => !t.IsComplete),
            Timestamp = DateTime.UtcNow.ToString("u")
        }, JsonOptions);
    }
}

/// <summary>
/// MCP Resources for the Todo Grid MCP App UI.
/// </summary>
[McpServerResourceType]
public static class TodoAppResources
{
    /// <summary>
    /// Serves the interactive HTML UI for the todos grid widget.
    /// This resource provides an MCP App interface using the ui:// scheme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTML content for the todos grid widget.</returns>
    [McpServerResource(UriTemplate = "ui://todos/index.html", Name = "todos_grid_widget", MimeType = "text/html")]
    [Description("Interactive Todos Grid Widget - An MCP App UI for displaying todos in a grid layout with live statistics.")]
    public static async Task<string> GetTodosGridWidget(CancellationToken cancellationToken)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "app", "dist", "index.html");

        if (!File.Exists(file))
        {
            return """
<!DOCTYPE html>
<html><head><title>UI Bundle Not Found</title></head><body>
<h1>UI Bundle Not Found</h1>
<p>The UI bundle hasn't been built yet. Please run:</p>
<pre>cd app
npm install
npm run build</pre>
</body></html>
""";
        }

        return await File.ReadAllTextAsync(file, cancellationToken);
    }
}

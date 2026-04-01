using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// MCP Prompts for generating AI-ready todo management prompt templates.
/// </summary>
[McpServerPromptType]
public static class TodoPrompts
{
    /// <summary>
    /// Generates a daily planning prompt with pending todos and optional focus area.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="focusArea">Optional area to prioritize (e.g., "work", "personal", "health").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prompt messages for the LLM to build a daily plan.</returns>
    [McpServerPrompt(Name = "daily_planner"), Description("Generate a daily plan based on pending todos with an optional focus area.")]
    public static async Task<IEnumerable<PromptMessage>> DailyPlanner(
        IServiceProvider serviceProvider,
        [Description("Optional focus area to prioritize (e.g., work, personal, health).")] string? focusArea,
        CancellationToken cancellationToken)
    {
        var db = serviceProvider.GetRequiredService<TodoDb>();
        var pendingTodos = await db.Todos
            .Where(t => !t.IsComplete)
            .ToListAsync(cancellationToken);

        var todoList = pendingTodos.Count > 0
            ? string.Join("\n", pendingTodos.Select(t => $"- [{t.Id}] {t.Name}"))
            : "No pending items.";

        var focusClause = string.IsNullOrWhiteSpace(focusArea)
            ? ""
            : $" Focus especially on items related to \"{focusArea}\".";

        return
        [
            new()
            {
                Role = Role.User,
                Content = new TextContentBlock
                {
                    Text = $"""
                        You are a productivity assistant. Here are my pending todo items:

                        {todoList}

                        Create a prioritized daily plan for me.{focusClause}
                        Suggest a logical order, estimate rough time blocks, and flag any items 
                        that could be batched together.
                        """
                }
            }
        ];
    }

    /// <summary>
    /// Generates a review prompt covering all todos for the LLM to analyze and recommend next actions.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prompt messages for the LLM to review and prioritize todos.</returns>
    [McpServerPrompt(Name = "todo_review"), Description("Review all todos (completed and pending) and recommend next actions.")]
    public static async Task<IEnumerable<PromptMessage>> TodoReview(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var db = serviceProvider.GetRequiredService<TodoDb>();
        var allTodos = await db.Todos.ToListAsync(cancellationToken);

        var completed = allTodos.Where(t => t.IsComplete).ToList();
        var pending = allTodos.Where(t => !t.IsComplete).ToList();

        var completedList = completed.Count > 0
            ? string.Join("\n", completed.Select(t => $"- [DONE] {t.Name}"))
            : "None yet.";

        var pendingList = pending.Count > 0
            ? string.Join("\n", pending.Select(t => $"- [PENDING] {t.Name}"))
            : "All clear!";

        return
        [
            new()
            {
                Role = Role.User,
                Content = new TextContentBlock
                {
                    Text = $"""
                        You are a task management advisor. Here is my current todo status:

                        ## Completed ({completed.Count})
                        {completedList}

                        ## Pending ({pending.Count})
                        {pendingList}

                        Please:
                        1. Highlight which pending items seem most urgent or important.
                        2. Identify any items that could be broken into smaller steps.
                        3. Suggest if any completed items have natural follow-ups.
                        4. Give an overall productivity assessment.
                        """
                }
            }
        ];
    }

    /// <summary>
    /// Generates a prompt asking the LLM to suggest new todo items based on existing ones and user context.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="context">User-provided context for generating suggestions (e.g., "preparing for a trip").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prompt messages for the LLM to suggest new todos.</returns>
    [McpServerPrompt(Name = "suggest_new_todos"), Description("Suggest 3-5 new todo items based on existing todos and user-provided context.")]
    public static async Task<IEnumerable<PromptMessage>> SuggestNewTodos(
        IServiceProvider serviceProvider,
        [Description("Context for suggestions (e.g., 'preparing for a trip', 'sprint planning').")] string context,
        CancellationToken cancellationToken)
    {
        var db = serviceProvider.GetRequiredService<TodoDb>();
        var existingTodos = await db.Todos.ToListAsync(cancellationToken);

        var existingList = existingTodos.Count > 0
            ? string.Join("\n", existingTodos.Select(t => $"- {t.Name} ({(t.IsComplete ? "done" : "pending")})"))
            : "No existing items.";

        return
        [
            new()
            {
                Role = Role.User,
                Content = new TextContentBlock
                {
                    Text = $"""
                        You are a helpful assistant. Here are my current todo items:

                        {existingList}

                        Context: {context}

                        Based on my existing items and the context above, suggest 3-5 new todo items 
                        I should add. For each suggestion, provide:
                        - A concise task name
                        - A brief reason why it's relevant
                        - A priority level (high, medium, low)
                        """
                }
            }
        ];
    }
}

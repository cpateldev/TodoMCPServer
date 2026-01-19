# Step-by-Step Explanation: `Program.cs`

This guide walks through the structure and logic of the `Program.cs` file for the Todo MCP Server, highlighting key ASP.NET Core and MCP server concepts.

## 1. Namespaces and Usings

- The file uses ASP.NET Core, Entity Framework Core (for data access), and Scalar.AspNetCore (for OpenAPI and Scalar API features).
- The main namespace is `AspNetOpenAPIDemo`.

## 2. Database Seeding

```csharp
internal static void PopulateTodoDB(IServiceProvider services)
```
- This method seeds the in-memory database with sample Todo items.
- It creates a scope, gets the `TodoDb` service, adds several `Todo` entities, and saves changes.
- **Purpose:** Ensures the API has initial data for demonstration and testing.

## 3. Application Startup (`Main` Method)

```csharp
public static async Task Main(string[] args)
```
- Entry point for the ASP.NET Core application.

### a. Web Host Builder

```csharp
var builder = WebApplication.CreateBuilder(args);
```
- Initializes the web application builder.

### b. Register Services

#### - Entity Framework Core In-Memory Database

```csharp
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
```
- Registers the TodoDb context using an in-memory database.
- **Purpose:** Quick setup for development/testing without a real database.

#### - OpenAPI Documentation

```csharp
builder.Services.AddOpenApi(options => { ... });
```
- Configures OpenAPI (Swagger) generation.
- Sets API metadata: title, version, description, contact info.
- **Purpose:** Enables API documentation and client generation.

#### - MCP Server Setup

```csharp
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithToolsFromAssembly();
```
- Adds Model Context Protocol (MCP) server capabilities.
- Supports both stdio and HTTP transports.
- Auto-discovers MCP tools in the assembly.
- **Purpose:** Exposes AI-powered tools and endpoints for LLM integration.

### c. Build Application

```csharp
var app = builder.Build();
```
- Builds the web application.

### d. Development Environment Configuration

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```
- Maps OpenAPI and Scalar API reference endpoints in development.
- **Purpose:** Enables API exploration and documentation during development.

### e. Middleware and Data Seeding

```csharp
app.UseHttpsRedirection();
PopulateTodoDB(app.Services);
```
- Redirects HTTP to HTTPS.
- Seeds the database with initial data.

### f. Endpoint Mapping

```csharp
app.MapEndPoints();
app.MapMcp(pattern: "api/mcp");
```
- Maps API endpoints (likely Minimal APIs or controllers).
- Maps MCP endpoint at `/api/mcp` for tool access.
- Optionally, can expose MCP at `/mcp` by uncommenting a line.

### g. Run Application

```csharp
app.Run();
```
- Starts the web server.

---

## Summary Table

| Step | Purpose |
|------|--------|
| Register DbContext | In-memory Todo data |
| Configure OpenAPI | API docs & metadata |
| Add MCP Server | AI tool endpoints |
| Seed Database | Demo/test data |
| Map Endpoints | REST & MCP APIs |
| Run App | Start server |

---

## Additional Notes

- **Minimal API Approach:** The code uses endpoint mapping (`app.MapEndPoints()`) typical of Minimal APIs.
- **MCP Integration:** Enables advanced AI tool invocation via HTTP or stdio.
- **OpenAPI:** Provides rich documentation for consumers and developers.

---

## Next Steps

- Explore `/openapi/v1.json` for API docs.
- Use `/api/mcp` to interact with MCP tools.
- Extend with new Todo endpoints or MCP tools as needed.

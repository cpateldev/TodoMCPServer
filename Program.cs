//using CoreWebAPI;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace AspNetOpenAPIDemo
{
    public class Program
    {
        internal static void PopulateTodoDB(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
                db.Todos.Add(new Todo { Name = "Go to watch movie" });
                db.Todos.Add(new Todo { Name = "Get the dog for a walk", IsComplete = true });
                db.Todos.Add(new Todo { Name = "Buy 3 gallons of milk" });
                db.Todos.Add(new Todo { Name = "Call mom", IsComplete = true });
                db.Todos.Add(new Todo { Name = "Do the laundry" });
                db.Todos.Add(new Todo { Name = "Finish the book", IsComplete = true });
                db.Todos.Add(new Todo { Name = "Go to the gym" });
                db.Todos.Add(new Todo { Name = "Buy a new phone", IsComplete = true });
                db.Todos.Add(new Todo { Name = "Get the car fixed" });
                db.Todos.Add(new Todo { Name = "Go to the supermarket", IsComplete = true });
                db.SaveChanges();
            }
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));            
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, _) =>
                {
                    document.Info = new()
                    {
                        Title = "Todo API",
                        Version = "v1",
                        Description = """
                                        Modern API for managing product catalogs.
                                        Supports JSON and XML responses.
                                        Rate limited to 1000 requests per hour.
                                        """,
                        Contact = new()
                        {
                            Name = "API Support",
                            Email = "api@example.com",
                            Url = new Uri("https://api.example.com/support")
                        }
                    };
                    return Task.CompletedTask;
                });
            });

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()   // Disable this line to deploy this to Azure App Service.
                .WithHttpTransport()
                .WithToolsFromAssembly();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi(); // Launch the app and navigate to https://localhost:{port}/openapi/v1.json to see the OpenAPI document.
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();
            PopulateTodoDB(app.Services);

            app.MapEndPoints();

            // Expose MCP endpoint
            app.MapMcp(pattern: "api/mcp");

            app.Run();
        }
    }
}

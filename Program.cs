using CoreWebAPI;
using Microsoft.EntityFrameworkCore;

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
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services
                .AddMcpServer()
                //.WithTools<TemperatureConverterTool>()    // Old way of adding tools now replaced with WithToolsFromAssembly()    
                //.WithTools<MultiplicationTool>()
                //.WithTools<WeatherTools>()
                //.WithTools<TodoTools>()
                .WithStdioServerTransport()   // Disable this line to deploy this to Azure App Service.
                .WithHttpTransport()
                .WithToolsFromAssembly();

            builder.Logging.AddConsole();  // If missing (common compilation error)
            builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));

            /*
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:5000")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            //.AllowCredentials() // Required if you use credentials
                            .WithExposedHeaders("Mcp-Session-Id", "X-Custom-Header"); // <-- Expose the custom header
                    });
            });
            */

            var app = builder.Build();

            //app.UseCors(MyAllowSpecificOrigins);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Todo API V1"));
            }

            app.UseHttpsRedirection();
            PopulateTodoDB(app.Services);

            app.MapEndPoints();

            // Expose MCP endpoint
            app.MapMcp(pattern: "api/mcp");

            app.Run();
            //await app.RunAsync();
        }
    }
}

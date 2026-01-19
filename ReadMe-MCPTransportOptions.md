## MCP Transport Options
 
 The MCP server can be configured to use different transport mechanisms to communicate with clients. The two primary transport options are:
 
 - **Standard Input/Output (stdio) Transport**: This transport is suitable for local AI clients that run on the same machine as the MCP server. It uses standard input and output streams for communication.
 
 - **HTTP Transport**: This transport is ideal for web-based clients and allows communication over HTTP protocols. It is especially useful when deploying the MCP server to cloud environments like Azure App Service.

 - **Custom Transports**: Developers can also implement custom transport mechanisms by adhering to the MCP transport interface, allowing for flexibility in communication methods.


 ### Configuring Transports in `Program.cs`
 
 To configure the MCP server with the desired transport options, you can chain the appropriate methods in the service configuration section of your `Program.cs` file.
 
 Here is an example of how to set up both stdio and HTTP transports:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()               // Add MCP server services support
    .WithStdioServerTransport()   // Use stdio transport for local AI clients
    .WithHttpTransport();         // Use HTTP transport for web-based clients
```
 
 > [!NOTE]
 > If you are deploying your MCP server to Azure App Service, you may need to comment out the `.WithStdioServerTransport()` line, as stdio transport is not supported in that environment.
 
 ### Summary
 
 By configuring the MCP server with the appropriate transport options, you can ensure effective communication with your AI clients, whether they are running locally or in a web-based environment. 
 
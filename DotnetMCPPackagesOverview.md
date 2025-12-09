
# Differences between ModelContextProtocol vs ModelContextProtocol.AspNetCore and when to use each of them and together

## 📚 Overview
*ModelContextProtocol* is the **core SDK** that defines the protocol, server, and client abstractions for building MCP-based systems. *ModelContextProtocol.AspNetCore* is an **integration layer** that plugs MCP into ASP.NET Core, adding HTTP transport, middleware, dependency injection, and session management. Use *ModelContextProtocol* when you want protocol-level functionality in any .NET app, and *ModelContextProtocol.AspNetCore* when hosting MCP servers inside ASP.NET Core web applications. Together, they let you build scalable MCP services with web endpoints and modern ASP.NET Core features.

---

## 🔑 Key Differences

| Feature | **ModelContextProtocol** | **ModelContextProtocol.AspNetCore** |
|---------|--------------------------|-------------------------------------|
| **Purpose** | Core SDK for MCP servers/clients | ASP.NET Core integration layer |
| **Scope** | Protocol definitions, server/client APIs, core abstractions | Hosting MCP servers in ASP.NET Core apps |
| **Transport** | Generic (can be extended to sockets, streams, etc.) | HTTP transport (SSE, streaming handlers) |
| **Session Management** | Basic protocol-level session handling | Stateful session manager integrated with ASP.NET Core |
| **Dependency Injection** | Minimal, protocol-focused | Full ASP.NET Core DI support |
| **Middleware/Filters** | Not included | Authorization filters, endpoint routing, background services |
| **Use Case** | Build MCP-compatible libraries, services, or clients | Expose MCP endpoints via ASP.NET Core web APIs |

Sources: 

---

## 🛠 When to Use Each

- **Use *ModelContextProtocol***:
  - If you’re building a **standalone MCP server or client** not tied to ASP.NET Core.
  - For **low-level protocol work** (custom transports, testing, or embedding MCP in non-web apps).
  - When you want **maximum portability** across .NET environments (console apps, services, etc.).

- **Use *ModelContextProtocol.AspNetCore***:
  - If you’re hosting MCP servers **inside ASP.NET Core applications**.
  - When you need **HTTP/SSE transport**, endpoint mapping, or ASP.NET Core middleware integration.
  - For scenarios requiring **session management, DI, and authentication/authorization**.

---

## 🤝 Using Them Together

- **Typical workflow**:
  1. Define your MCP server logic using *ModelContextProtocol* (core abstractions).
  2. Host it in an ASP.NET Core app with *ModelContextProtocol.AspNetCore*.
  3. Leverage ASP.NET Core features (routing, DI, middleware) while still adhering to MCP standards.

- **Example scenario**:  
  You’re building an AI-powered service that needs to expose MCP endpoints over HTTP.  
  - *ModelContextProtocol* → defines the protocol, request/response handling.  
  - *ModelContextProtocol.AspNetCore* → maps those endpoints into ASP.NET Core routes, adds session management, and integrates with authentication.

---

## ⚠️ Considerations & Trade-offs

- **Performance**: Using *AspNetCore* adds middleware overhead compared to raw protocol hosting, but gives scalability and maintainability benefits.
- **Flexibility**: If you don’t need ASP.NET Core features (e.g., DI, routing), sticking with *ModelContextProtocol* alone may be simpler.
- **Extensibility**: Together, they provide both **protocol compliance** and **modern web hosting capabilities**, which is ideal for production systems.

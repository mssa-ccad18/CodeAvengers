# CodeChat - Real-time Chat Application

A real-time chat application built with Blazor and SignalR.

## Project Structure

### Server-Side Project (CodeChat/)
```
CodeChat/
├── Components/         # Shared Blazor components used across the application
├── Hubs/              # SignalR hubs for real-time communication
├── wwwroot/           # Static files (CSS, JavaScript, images)
├── Properties/        # Project properties and launch settings
├── Program.cs         # Application entry point and service configuration
├── appsettings.json   # Application configuration
└── CodeChat.csproj    # Project file with dependencies
```

### Client-Side Project (CodeChat.Client/)
```
CodeChat.Client/
├── Pages/             # Blazor pages and components
├── wwwroot/           # Client-side static files
├── Program.cs         # Client application entry point
├── _Imports.razor     # Global using directives
└── CodeChat.Client.csproj  # Client project file
```

## Key Components

### Server-Side
- **Components/**: Contains reusable UI components that can be shared between client and server
- **Hubs/**: Contains SignalR hub classes that handle real-time communication between clients
- **Program.cs**: Configures services, middleware, and SignalR hub endpoints

### Client-Side
- **Pages/**: Contains the main UI pages of the application
- **wwwroot/**: Stores static files like CSS, JavaScript, and images
- **Program.cs**: Sets up the client-side application and connects to SignalR hub

## Technology Stack
- Blazor (Server and Client)
- SignalR for real-time communication
- C# for backend logic
- HTML/CSS for frontend styling

## Getting Started
[To be added: Setup instructions, prerequisites, and running instructions] 
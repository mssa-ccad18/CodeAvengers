using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using CodeChat.Client.Services;
using CodeChat.Client.Services.Encryption;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register services
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ChatEncryptionService>();

var host = builder.Build().RunAsync();

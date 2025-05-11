using CodeChat.Client.Services;
using CodeChat.Client.Services.Encryption;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<UserSessionService>();


var host = builder.Build().RunAsync();

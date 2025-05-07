using CodeChat.Client.Pages;
using CodeChat.Components;
using Microsoft.AspNetCore.ResponseCompression;
using CodeChat.Hubs;
using CodeChat.Services;
using CodeChat.Services.Encryption;
using CodeChat.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts => {
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

//encrypted room service
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//Register encryption service
builder.Services.AddSingleton<IRoomEncryptionService, RoomEncryptionService>();

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CodeChat.Client._Imports).Assembly);
//enpoints added (razorpages , controllers for encryption service)
app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.Run();

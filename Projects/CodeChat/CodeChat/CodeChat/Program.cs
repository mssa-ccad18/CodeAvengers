using CodeChat.Client.Pages;
using CodeChat.Components;
using Microsoft.AspNetCore.ResponseCompression;
using CodeChat.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;
using CodeChat.Data;
using CodeChat.Client.Components.Models;
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


var connectionString = "Data Source=chat.db";
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(connectionString));

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


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated(); 

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Username = "alice", Password = "password" },
            new User { Username = "bob", Password = "password" }
        );
        db.SaveChanges();
    }

    if(!db.ChatRooms.Any())
    {
        var alice = db.Users.FirstOrDefault
        db.ChatRooms.Add( new Room { RoomOwner = db.Users.})
    }



}

app.MapGet("/api/users", async (ChatDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
});

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

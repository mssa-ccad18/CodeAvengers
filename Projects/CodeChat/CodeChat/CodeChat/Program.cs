using Microsoft.AspNetCore.ResponseCompression;
using CodeChat.Hubs;
using Microsoft.EntityFrameworkCore;
using CodeChat.Data;
using CodeChat.Services.Encryption;
using CodeChat.Services;
using CodeChat.Client.Services.Encryption;
using CodeChat.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CodeChat.Components;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy",
        builder => {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});


builder.Services.AddResponseCompression(opts => {
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});


var connectionString = "Data Source=chat.db";
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//Register encryption service
builder.Services.AddSingleton(new RoomService(new RoomEncryptionService()));
builder.Services.AddScoped<ChatEncryptionService>();
builder.Services.AddScoped<UserSessionService>();

var app = builder.Build();

app.UseResponseCompression();
app.UseCors("CorsPolicy");

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
}

app.MapGet("/api/users", async (ChatDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
});

app.MapGet("/api/chatrooms", async (ChatDbContext db) =>
{
    var chatrooms = await db.ChatRooms.ToListAsync();
    return Results.Ok(chatrooms);
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

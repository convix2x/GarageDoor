using PLGarageFrontend.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<PLGarageFrontend.Services.PLGarageService>();
builder.Services.AddSingleton<PLGarageFrontend.Services.StringMapService>();
builder.Services.AddHttpClient("moderation");
builder.Services.AddScoped<PLGarageFrontend.Services.ModerationService>(sp =>
    new PLGarageFrontend.Services.ModerationService(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("moderation"),
        sp.GetRequiredService<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage>()));
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // shut the fuck up microsoft i dont fucking care you douche
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

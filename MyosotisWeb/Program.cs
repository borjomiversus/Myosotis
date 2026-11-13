using MudBlazor.Services;
using MyosotisWeb.Components;
using MyosotisWeb.ServicesWeb;
using MyosotisWeb.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AnonymousUserDataService>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<MediaSearch>();
builder.Services.AddSingleton<MediaRecommendation>();
builder.Services.AddMudServices();
builder.Services.AddScoped<DemoDataService>();
builder.Services.AddHttpClient("TmdbClient", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    var token = builder.Configuration["Tmdb:ReadAccessToken"];
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<TmdbApiService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<AnonymousUserMiddleware>();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();

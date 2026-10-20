using MudBlazor.Services;
using MyosotisWeb.Components;
using MyosotisWeb.ServicesWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<MediaSearch>();
builder.Services.AddSingleton<MediaRecommendation>();
builder.Services.AddMudServices();
builder.Services.AddSingleton<MyosotisWeb.ServicesWeb.DemoDataService>();
builder.Services.AddMudServices();
builder.Services.AddHttpClient("TmdbClient", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    var token = builder.Configuration["Tmdb:ReadAccessToken"];
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<TmdbApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using Hum.Server.Audio;
using Hum.Server.Data;
using Hum.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<PcmDecoder>();
builder.Services.AddSingleton<SpectrogramBuilder>();
builder.Services.AddSingleton<PeakPicker>();
builder.Services.AddSingleton<FingerprintGenerator>();
builder.Services.AddSingleton<FingerprintService>();

var connString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=hum.db";
var dbInit = new DbInitializer(connString);
dbInit.Initialize();

builder.Services.AddSingleton(new FingerprintStore(connString));
builder.Services.AddSingleton<MatcherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

using Microsoft.EntityFrameworkCore;
using LojaApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --- DO BANCO DE DADOS ---
var connectionString = builder.Configuration.GetConnectionString("BazarConexao"); //O nome "BazarConexao" tem que ser o mesmo do appsettings.json

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddPageRoute("/Produtos/Index", "");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

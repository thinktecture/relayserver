using ExampleArticleApi.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
	o.ForwardedHeaders = ForwardedHeaders.All;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(c =>
	c.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<RequestInfoService>();

// Build application
var app = builder.Build();


// Define http request pipeline
app.UseHttpsRedirection();

app.UseForwardedHeaders();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

// run
app.Run();

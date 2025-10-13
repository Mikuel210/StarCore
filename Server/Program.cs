using Microsoft.AspNetCore.ResponseCompression;
using SDK;
using Server;

var builder = WebApplication.CreateBuilder(args);

// Setup services
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		[ "application/octet-stream" ]);
});

// Create and run app
var app = builder.Build();
app.UseResponseCompression();

app.MapHub<ServerHub>("/hub");
app.MapGet("/", () => "All engines running");

app.Run();
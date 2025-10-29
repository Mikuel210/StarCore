using Microsoft.AspNetCore.ResponseCompression;
using SDK;
using SDK.Communication;
using Server;

var builder = WebApplication.CreateBuilder(args);

// Setup services
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(options =>
{
	options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		[ "application/octet-stream" ]);
});

// Create app
var app = builder.Build();
app.UseResponseCompression();

app.MapHub<ServerHub>("/hub");
app.MapGet("/", () => "All engines running");

// Start Core
var thread = new Thread(Core.Initialize);
thread.Start();

// Start app
app.Run();

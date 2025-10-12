using Microsoft.AspNetCore.ResponseCompression;
using Server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Setup SignalR
builder.Services.AddSignalR();

// Setup compression
builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		[ "application/octet-stream" ]);
});

app.UseResponseCompression();

// Map the hub and run the app
app.MapHub<ServerHub>("/");
app.Run();
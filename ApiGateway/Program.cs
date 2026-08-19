using ApiGateway;
using ApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiGateway();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();

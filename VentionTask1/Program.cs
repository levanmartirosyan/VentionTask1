using VentionTask1;
using VentionTask1.Application;
using VentionTask1.Infrastructure;
using VentionTask1.WebApi.GrpcServices;
using VentionTask1.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<UsersGrpcService>();

app.MapControllers();

app.Run();

using VentionTask1.Application;
using VentionTask1.Infrastructure;
using VentionTask1;

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

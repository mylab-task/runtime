using MyLab.Task.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTaskRuntime(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

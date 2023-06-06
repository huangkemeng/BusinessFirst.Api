using RenameMe.Api.Engines.Bases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
var app = builder.BuildWithEngines();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

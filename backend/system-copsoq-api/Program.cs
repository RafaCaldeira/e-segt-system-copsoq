using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using Microsoft.AspNetCore.Identity;
using system_copsoq_api.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexão com o SQL Server (pode ser local)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IPasswordHasher<Empresa>, PasswordHasher<Empresa>>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
        policy =>
        {
            policy.AllowAnyOrigin()  // Permite qualquer origem (qualquer site)
                  .AllowAnyMethod()  // Permite qualquer método (GET, POST, PUT, DELETE)
                  .AllowAnyHeader(); // Permite qualquer cabeçalho
        });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseAuthorization();
app.MapControllers();
app.Run();
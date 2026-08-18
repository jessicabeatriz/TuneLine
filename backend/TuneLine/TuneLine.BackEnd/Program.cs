using Microsoft.EntityFrameworkCore;
using TuneLine.BackEnd.Data;
using TuneLine.BackEnd.Repositories;
using TuneLine.BackEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TuneLineDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteTuneLine"))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<SpotifyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

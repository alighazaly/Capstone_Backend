using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Backend.Repositories.AuthenticationRepository;
using Capstone_Backend.Repositories.RegistrationRepository;
using Capstone_Backend.Repositories.UserRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                                                                                      // Add repository services

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
    build.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

//enable
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Get the full path to the ProfileImages directory within the project
var profileImagesPath = Path.Combine(app.Environment.ContentRootPath, "ProfileImages");
var appartmentsImagesPath = Path.Combine(app.Environment.ContentRootPath, "AppartmentsImages");
// Configure static file serving with the correct root directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profileImagesPath),
    RequestPath = "/ProfileImages"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(appartmentsImagesPath),
    RequestPath = "/AppartmentsImages"
});

app.UseCors("corspolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

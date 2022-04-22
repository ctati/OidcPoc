using Microsoft.AspNetCore.Authentication.JwtBearer;
using server;

var builder = WebApplication.CreateBuilder(args);

// Configure default CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            // TODO: Read from config
            builder.WithOrigins("http://localhost:4200");
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT authentication.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearerConfiguration(
        builder.Configuration["Jwt:Issuer"],
        builder.Configuration["Jwt:Audience"]
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS
app.UseCors();

// Add JWT authentication.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using server;
using System.Security.Claims;

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
builder.Services.AddSingleton<IAuthorizationHandler, ApplicationOwnerRequirementHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //c.EnableAnnotations();
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT authentication.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearerConfiguration(builder);

builder.Services.AddAuthorization(options =>
{
    // Configure custom aith policy
    options.AddPolicy("ContentsEditor", policy =>
    {
        // custom requirement
        policy.Requirements.Add(new ApplicationOwnerRequirement(ApplicationOwnerRequirement.CurrentUser));

        // complex assertion
        policy.RequireAssertion(ctx =>
           {
               Console.WriteLine("executing policy 'ContentsEditor'.");
               return ctx.User.HasClaim(ClaimTypes.Email, "ctati@pa.gov") ||
                      ctx.User.HasClaim(ClaimTypes.Role, "admin");
           });

        // simple built-in checks
        policy.RequireRole("admin");
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors();

app.UseRouting();

// Add authentication and authorization middleware.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

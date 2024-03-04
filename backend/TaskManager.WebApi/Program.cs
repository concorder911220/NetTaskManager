using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application;
using TaskManager.Common;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Services;
using TaskManager.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.RegisterModules();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection("GoogleOAuthOptions"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}       
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("api");
apiGroup.MapEndpoints();

apiGroup.MapGet("test", ([FromQuery] string token, HttpContext context, JwtService jwtService) =>
{
    var test = jwtService.GetPrincipalFromExpiredToken(token);
});

app.Run();
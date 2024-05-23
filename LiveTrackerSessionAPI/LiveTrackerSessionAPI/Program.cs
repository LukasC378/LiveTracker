using System.Reflection;
using System.Security.Claims;
using idunno.Authentication.Basic;
using LiveTrackerSessionAPI.Handlers;
using LiveTrackerSessionAPI.Hubs;
using LiveTrackerSessionAPI.Repository;
using LiveTrackerSessionAPI.Services;
using LiveTrackerSessionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IRepository, Repository>();
builder.Services.AddSingleton<IBasicAuthenticationHandler, BasicAuthenticationHandler>();
builder.Services.AddSingleton<IJWTService, JWTService>();

builder.Services.AddScoped<ITrackService, TrackService>();

var jwtSettings = builder.Configuration.GetSection("JWTSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"]!,
        ValidAudience = jwtSettings["Audience"]!,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings["SecretKey"]!))
    };
});

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = Assembly.GetExecutingAssembly().GetName().Name;
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                var authenticationHandler = context.HttpContext.RequestServices.GetService<IBasicAuthenticationHandler>();

                if (!(authenticationHandler?.CheckUser(context.Username, context.Password) ?? false))
                    return Task.CompletedTask;

                var claims = Array.Empty<Claim>();

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy(BasicAuthenticationDefaults.AuthenticationScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(BasicAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x.AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseRouting(); // UseRouting should come before any other middleware that depends on endpoint routing

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<RaceHub>("/racehub");
});

app.Run();
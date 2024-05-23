using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;
using BL;
using BL.Extensions;
using BL.Middleware;
using BL.Services;
using BL.Services.Interfaces;
using BL.Workers;
using DB.Database;
using idunno.Authentication.Basic;

var builder = WebApplication.CreateBuilder(args);

var sessionApiSettings = builder.Configuration.GetSection("LiveTrackerSessionApi");
var webSiteSettings = builder.Configuration.GetSection("WebSite");
var jwtSettings = builder.Configuration.GetSection("JWTSettings");

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<RaceTrackerDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("API")));

//Workers
builder.Services.AddHostedService<NotificationWorker>();

//Controllers
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //c.IncludeXmlComments(xmlPath);
});

// HTTP Clients
builder.Services.AddHttpClient(
    Constants.RECAPTCHA_CLIENT,
    client =>
    {
        client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/");
    });

builder.Services.AddHttpClient(Constants.SESSION_API_CLIENT, client =>
{
    client.BaseAddress = new Uri(sessionApiSettings["Url"]!);

    var username = sessionApiSettings["User"];
    var password = sessionApiSettings["Password"];
    var base64AuthString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BasicAuthenticationDefaults.AuthenticationScheme, base64AuthString);
});

// Services
builder.Services.AddSingleton<IJWTService, JWTService>();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILayoutsService, LayoutsService>();
builder.Services.AddScoped<ISubscribeService, SubscribeService>();

builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<ISessionApiService, SessionApiService>();
builder.Services.AddTransient<IRecaptchaService, RecaptchaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", x =>
    {
        x.WithOrigins(webSiteSettings["Url"]!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Allow credentials (cookies) only for specific origins
    });
});

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings["SecretKey"]!))
    };
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigration();
}
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseJwtTokenMiddleware();

app.UseAuthentication();
app.UseAuthorization();

//Middleware
app.UseExceptionHandlingMiddleware();

app.MapControllers();

app.Run();

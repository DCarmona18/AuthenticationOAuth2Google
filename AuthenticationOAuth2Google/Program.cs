using AuthenticationOAuth2Google.Authentication;
using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Services;
using AuthenticationOAuth2Google.Hubs;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using AuthenticationOAuth2Google.Infrastructure.Models;
using AuthenticationOAuth2Google.Infrastructure.Repositories;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using AuthenticationServiceDomain = AuthenticationOAuth2Google.Domain.Services.AuthenticationService;
using IAuthenticationServiceDomain = AuthenticationOAuth2Google.Domain.Interfaces.IAuthenticationService;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, (o) => { });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            // TODO: Add origins from AppSettings
            policy.WithOrigins("http://localhost", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton(typeof(IMongoDBRepository<>), typeof(MongoDBRepository<>));
builder.Services.AddSingleton(FirebaseApp.Create());

// Register Services
builder.Services.AddScoped<IAuthenticationServiceDomain, AuthenticationServiceDomain>();
builder.Services.AddScoped<IChatHubService, ChatHubService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();

builder.Services.AddSignalR();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapHub<ChatHub>("/api/hubs/chat");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

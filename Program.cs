using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Services.Auth;
using Newtonsoft.Json.Converters;
using Microsoft.OpenApi.Models;
using System.Reflection;
using sample_auth_aspnet.Services.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

#region AutoMapper Configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#endregion

builder.Services.AddSwaggerGen();

ConfigureServices(builder.Services, builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    #region Versioning
    services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    });
    #endregion

    #region Swagger Docs
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Authentication API",
            Version = "1.0",
            Description = "A simple sample authentication API using ASP.NET Core Web API",
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.\r\n\r\n 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          \r\n\r\nExample: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
    #endregion

    #region SQL Server 
    services.AddDbContext<DataContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    #endregion

    #region Authentication
    var jwt = configuration.GetSection("JWT");
    var key = jwt["Key"];

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };
    });
    #endregion

    services.AddLogging();

    services.AddTransient<DataContext>();

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IUserService, UserService>();
}
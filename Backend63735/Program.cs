using Backend63735.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Backend63735;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using User.Management.Service.Models;
using User.Management.Service.Services;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
var conn = new MySqlConnection("Server=diploma.mysql.database.azure.com;Database=adhd_api;Uid=w63735;Pwd=K01110011!");

// For Entity Framework
builder.Services.AddDbContext<DemoDbContext>(
dbContextOptions => dbContextOptions
             .UseMySql("Server=diploma.mysql.database.azure.com;Database=adhd_api;Uid=w63735;Pwd=K01110011!", serverVersion)
//.LogTo(Console.WriteLine, LogLevel.Information)
//.EnableSensitiveDataLogging()
//.EnableDetailedErrors()
);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", builder =>
    {
        builder
        .WithOrigins("http://localhost:49430")
            .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// For Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DemoDbContext>()
    .AddDefaultTokenProviders();

//Add Config for Required Email
builder.Services.Configure<IdentityOptions>(
    opts => opts.SignIn.RequireConfirmedEmail = true
    );

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
});


//Add Email Configs
var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserManagement, UserManagement>();

builder.Services.AddControllers().AddNewtonsoftJson(opt =>
{
    opt.SerializerSettings.Converters.Add(new StringEnumConverter());
}); ;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
    option.UseInlineDefinitionsForEnums();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
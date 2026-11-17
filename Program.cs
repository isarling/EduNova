using EduNova.Authorization;
using EduNova.Data;
using EduNova.Extensions;
using EduNova.Mapping;
using EduNova.Middleware;
using EduNova.Models.Settings;
using EduNova.Repositories;
using EduNova.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => options.Filters.Add<EduNova.Filters.OperacionExceptionFilter>());

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(
        (builder.Configuration.GetConnectionString("EduNova")
            ?? builder.Configuration.GetConnectionString("AcademiaAula")
        ?? builder.Configuration.GetConnectionString("IsarlingProyecto"))));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "No está presente la configuración del JWT");

builder.Services.AddAutoMapper(
    cfg => cfg.AddMaps(typeof(UserProfile).Assembly));

builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRoleRepository, RolRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token =
                    context.Request.Cookies["access_token"];

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();

                if (!context.Response.HasStarted)
                    context.Response.Redirect("/Auth/Login");

                return Task.CompletedTask;
            },

            OnForbidden = context =>
            {
                if (!context.Response.HasStarted)
                    context.Response.Redirect("/Auth/AccessDenied");

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin.panel", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.HasPermission("rol.ver") ||
            context.User.HasPermission("permiso.ver") ||
            context.User.HasPermission("usuario.ver"));
    });

    options.AddPolicy("permiso.actualizar", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.HasPermission("permiso.actualizar") ||
            context.User.HasPermission("permiso.crear"));
    });

    options.AddPolicy("permiso.borrar", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.HasPermission("permiso.borrar") ||
            context.User.HasPermission("permiso.crear"));
    });
});

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    JwtPermissionPolicies.PermissionPolicyProvider>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    JwtPermissionPolicies.PermissionAuthorizationHandler>();

builder.Services.AddScoped<ICreditoRepository, CreditoRepository>();
builder.Services.AddScoped<ICreditoService, CreditoService>();
builder.Services.AddScoped<IMatriculaService, MatriculaService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDBContext>();

    await DbSeeder.SeedAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status404NotFound)
        response.Redirect("/Home/NotFoundPage");

    return Task.CompletedTask;
});

app.UseRouting();

app.UseMiddleware<JwtRefreshMiddleware>();

app.UseAuthentication();

app.UseMiddleware<ValidarSesionMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
// Punto de entrada visible para las pruebas de integración.
public partial class Program { }

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SGP_Freelancing.Data;
using SGP_Freelancing.Middleware;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Utilities;
using System.Text;

namespace SGP_Freelancing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // ========== Configure Serilog ==========
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .CreateLogger();

                builder.Host.UseSerilog();

            // ========== Add services to the container ==========
            
            // Database Context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true; // OTP email verification is now active
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            // JWT Authentication for API (optional)
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345678"; // Change in production
            
            builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "SGP_Freelancing",
                    ValidAudience = jwtSettings["Audience"] ?? "SGP_Freelancing_Users",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.Policies.RequireAdminRole, 
                    policy => policy.RequireRole(Constants.Roles.Admin));
                options.AddPolicy(Constants.Policies.RequireClientRole, 
                    policy => policy.RequireRole(Constants.Roles.Client));
                options.AddPolicy(Constants.Policies.RequireFreelancerRole, 
                    policy => policy.RequireRole(Constants.Roles.Freelancer));
                options.AddPolicy(Constants.Policies.RequireClientOrFreelancer, 
                    policy => policy.RequireRole(Constants.Roles.Client, Constants.Roles.Freelancer));
            });

            // Repository Pattern & Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IProjectService, SGP_Freelancing.Services.ProjectService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IBidService, SGP_Freelancing.Services.BidService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IMessageService, SGP_Freelancing.Services.MessageService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IContractService, SGP_Freelancing.Services.ContractService>();

            // Email & OTP Services
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IEmailService, SGP_Freelancing.Services.EmailService>();
            builder.Services.AddSingleton<SGP_Freelancing.Services.OtpService>();

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(Program));

            // SignalR
            builder.Services.AddSignalR();

            // Controllers with Views
            builder.Services.AddControllersWithViews();

            // API Controllers
            builder.Services.AddControllers();

            // Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SGP Freelancing API",
                    Version = "v1",
                    Description = "API for SGP Freelancing Platform"
                });

                // JWT Authentication in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
                        Array.Empty<string>()
                    }
                });
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Session (for notifications, etc.)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Anti-forgery
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // Data Protection (for anti-forgery tokens in containers)
            builder.Services.AddDataProtection()
                .SetApplicationName("SGP_Freelancing")
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

            // Forwarded Headers (for Render reverse proxy)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            // Add application lifetime logging
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() => Log.Information("APPLICATION STARTED - Server is now listening for requests"));
            lifetime.ApplicationStopping.Register(() => Log.Warning("APPLICATION STOPPING - Shutdown initiated"));
            lifetime.ApplicationStopped.Register(() => Log.Warning("APPLICATION STOPPED - Shutdown complete"));

            // ========== Configure the HTTP request pipeline ==========

            // Forwarded Headers (MUST be first - before any other middleware)
            app.UseForwardedHeaders();

            // Global Exception Handling
            app.UseExceptionHandlingMiddleware();

            // Swagger (Development & Staging)
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGP Freelancing API V1");
                    c.RoutePrefix = "api/docs";
                });
            }

            // TEMPORARILY show detailed errors everywhere to debug Render issues
            // TODO: Remove this after debugging
            app.UseDeveloperExceptionPage();

            // Only redirect to HTTPS in development (Render handles SSL termination)
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // Map MVC Controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Map API Controllers
            app.MapControllers();

            // Map SignalR Hub
            app.MapHub<SGP_Freelancing.Hubs.ChatHub>("/chatHub");

            // Initialize Database & Roles
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Ensure database is created and migrations are applied
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    Log.Information("Ensuring database exists and applying migrations...");
                    context.Database.Migrate(); // This will create the database if it doesn't exist
                    Log.Information("Database ready");

                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    InitializeRolesAndAdminAsync(roleManager, userManager).Wait();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while initializing database or roles");
                }
            }

            Log.Information("Starting SGP Freelancing Platform");
            Log.Information("About to call app.Run()...");
            
            try
            {
                app.Run();
                Log.Information("app.Run() completed normally");
            }
            catch (Exception runEx)
            {
                Log.Fatal(runEx, "Exception in app.Run()");
                throw;
            }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task InitializeRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Create roles if they don't exist
            string[] roleNames = { Constants.Roles.Admin, Constants.Roles.Client, Constants.Roles.Freelancer };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Log.Information($"Role '{roleName}' created");
                }
            }

            // Create default admin user
            var adminEmail = "admin@sgpfreelancing.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Constants.Roles.Admin);
                    Log.Information("Default admin user created");
                }
            }
        }
    }
}

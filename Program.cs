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
                var seedDummyEmailData = args.Any(a => string.Equals(a, "--seed-dummy-email-data", StringComparison.OrdinalIgnoreCase));
                var seedDummyMarketData = args.Any(a => string.Equals(a, "--seed-dummy-market-data", StringComparison.OrdinalIgnoreCase));

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
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IReviewService, SGP_Freelancing.Services.ReviewService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IPortfolioService, SGP_Freelancing.Services.PortfolioService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IProfileService, SGP_Freelancing.Services.ProfileService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IFreelancerService, SGP_Freelancing.Services.FreelancerService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IFileUploadService, SGP_Freelancing.Services.FileUploadService>();
            builder.Services.AddScoped<SGP_Freelancing.Services.Interfaces.IBookmarkService, SGP_Freelancing.Services.BookmarkService>();

            // Add HttpClient for calling AI Microservice
            builder.Services.AddHttpClient();

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

            // CORS - Secure configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                        ?? new[] { "http://localhost:3000", "http://localhost:5173", "https://localhost:7042" };
                    
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
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

            app.UseCors("AllowSpecificOrigins");

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

                    // Seed Categories and Skills
                    SeedCategoriesAndSkillsAsync(context).Wait();

                    if (seedDummyEmailData || seedDummyMarketData)
                    {
                        SeedDummyEmailDataAsync(services).Wait();
                    }

                    if (seedDummyMarketData)
                    {
                        SeedDummyMarketDataAsync(services).Wait();
                        Log.Information("Dummy email data seeding completed. Exiting without starting the web host.");
                        return;
                    }
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

        private static async Task SeedCategoriesAndSkillsAsync(ApplicationDbContext context)
        {
            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Web Development", 
                        Description = "Build websites and web applications",
                        IconClass = "fa-globe",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Mobile App Development", 
                        Description = "iOS, Android and cross-platform apps",
                        IconClass = "fa-mobile-alt",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Graphic Design", 
                        Description = "Logo design, branding, illustrations",
                        IconClass = "fa-palette",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "UI/UX Design", 
                        Description = "User interface and experience design",
                        IconClass = "fa-pencil-ruler",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Content Writing", 
                        Description = "Articles, blogs, copywriting",
                        IconClass = "fa-pen",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Digital Marketing", 
                        Description = "SEO, social media, email marketing",
                        IconClass = "fa-bullhorn",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Video & Animation", 
                        Description = "Video editing, motion graphics, 3D animation",
                        IconClass = "fa-video",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Data Science & Analytics", 
                        Description = "Data analysis, machine learning, AI",
                        IconClass = "fa-chart-bar",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Virtual Assistant", 
                        Description = "Administrative support, customer service",
                        IconClass = "fa-headset",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SGP_Freelancing.Models.Entities.Category 
                    { 
                        Name = "Translation", 
                        Description = "Language translation and localization",
                        IconClass = "fa-language",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                Log.Information($"{categories.Length} categories seeded");
            }

            // Seed Skills
            if (!context.Skills.Any())
            {
                var skills = new[]
                {
                    // Web Development
                    new SGP_Freelancing.Models.Entities.Skill { Name = "HTML/CSS", Description = "Frontend markup and styling", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "JavaScript", Description = "Frontend and backend programming", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "React", Description = "JavaScript library for building UIs", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Vue.js", Description = "Progressive JavaScript framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Angular", Description = "TypeScript-based web application framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Node.js", Description = "JavaScript runtime for backend", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "ASP.NET Core", Description = "Cross-platform .NET framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "PHP", Description = "Server-side scripting language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Python", Description = "High-level programming language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Django", Description = "Python web framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Ruby on Rails", Description = "Ruby web framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Mobile Development
                    new SGP_Freelancing.Models.Entities.Skill { Name = "React Native", Description = "Cross-platform mobile development", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Flutter", Description = "Cross-platform mobile UI framework", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "iOS Development", Description = "Native iOS app development", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Android Development", Description = "Native Android app development", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Swift", Description = "iOS programming language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Kotlin", Description = "Android programming language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Design
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Adobe Photoshop", Description = "Image editing and design", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Adobe Illustrator", Description = "Vector graphics design", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Figma", Description = "UI/UX design and prototyping", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Adobe XD", Description = "UI/UX design tool", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Sketch", Description = "Digital design platform", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "UI Design", Description = "User interface design", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "UX Design", Description = "User experience design", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Marketing
                    new SGP_Freelancing.Models.Entities.Skill { Name = "SEO", Description = "Search engine optimization", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Social Media Marketing", Description = "Social platform marketing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Content Marketing", Description = "Content strategy and creation", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Google Ads", Description = "PPC advertising", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Email Marketing", Description = "Email campaign management", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Data & AI
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Machine Learning", Description = "ML model development", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Data Analysis", Description = "Data processing and insights", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "SQL", Description = "Database query language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Power BI", Description = "Business intelligence tool", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Tableau", Description = "Data visualization platform", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Video & Animation
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Video Editing", Description = "Video post-production", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Adobe After Effects", Description = "Motion graphics and VFX", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Adobe Premiere Pro", Description = "Video editing software", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "3D Animation", Description = "3D modeling and animation", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Blender", Description = "3D creation suite", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                    // Other
                    new SGP_Freelancing.Models.Entities.Skill { Name = "WordPress", Description = "CMS platform", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Shopify", Description = "E-commerce platform", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Technical Writing", Description = "Documentation and manuals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new SGP_Freelancing.Models.Entities.Skill { Name = "Copywriting", Description = "Marketing and sales writing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                await context.Skills.AddRangeAsync(skills);
                await context.SaveChangesAsync();
                Log.Information($"{skills.Length} skills seeded");
            }
        }

        private static async Task SeedDummyEmailDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var dummyUsers = new[]
            {
                new
                {
                    Email = "client.demo1@sgpfreelancing.local",
                    Password = "Demo@1234",
                    FirstName = "Aarav",
                    LastName = "Sharma",
                    Role = Constants.Roles.Client,
                    EmailConfirmed = true,
                    Otp = "123456"
                },
                new
                {
                    Email = "freelancer.demo1@sgpfreelancing.local",
                    Password = "Demo@1234",
                    FirstName = "Meera",
                    LastName = "Verma",
                    Role = Constants.Roles.Freelancer,
                    EmailConfirmed = false,
                    Otp = "654321"
                },
                new
                {
                    Email = "freelancer.demo2@sgpfreelancing.local",
                    Password = "Demo@1234",
                    FirstName = "Rohan",
                    LastName = "Khan",
                    Role = Constants.Roles.Freelancer,
                    EmailConfirmed = false,
                    Otp = "778899"
                },
                new
                {
                    Email = "client.demo2@sgpfreelancing.local",
                    Password = "Demo@1234",
                    FirstName = "Kabir",
                    LastName = "Singh",
                    Role = Constants.Roles.Client,
                    EmailConfirmed = false,
                    Otp = "112233"
                }
            };

            foreach (var dummy in dummyUsers)
            {
                var user = await userManager.FindByEmailAsync(dummy.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = dummy.Email,
                        Email = dummy.Email,
                        FirstName = dummy.FirstName,
                        LastName = dummy.LastName,
                        EmailConfirmed = dummy.EmailConfirmed,
                        IsActive = true
                    };

                    var createResult = await userManager.CreateAsync(user, dummy.Password);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create dummy user {dummy.Email}: {errors}");
                    }

                    await userManager.AddToRoleAsync(user, dummy.Role);

                    if (dummy.Role == Constants.Roles.Freelancer)
                    {
                        var profile = new FreelancerProfile
                        {
                            UserId = user.Id,
                            Title = "Full Stack Developer",
                            Bio = "Demo freelancer account for testing email and OTP flows.",
                            HourlyRate = 1500,
                            TotalEarnings = 45000,
                            CompletedProjects = 12,
                            AverageRating = 4.7m,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await context.FreelancerProfiles.AddAsync(profile);
                    }
                    else if (dummy.Role == Constants.Roles.Client)
                    {
                        var profile = new ClientProfile
                        {
                            UserId = user.Id,
                            CompanyName = "Demo Client Pvt Ltd",
                            CompanyDescription = "Demo client account for testing email and OTP flows.",
                            Website = "https://example.com",
                            TotalProjectsPosted = 5,
                            AverageRating = 4.5m,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await context.ClientProfiles.AddAsync(profile);
                    }

                    Log.Information("Created dummy user {Email}", dummy.Email);
                }

                var existingOtp = await context.OtpRecords
                    .Where(r => r.Email == dummy.Email)
                    .ToListAsync();

                context.OtpRecords.RemoveRange(existingOtp);
                context.OtpRecords.Add(new OtpRecord
                {
                    Email = dummy.Email,
                    Otp = dummy.Otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            Log.Information("Seeded dummy email users and OTP records for testing");
        }

        private static async Task SeedDummyMarketDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            var clientUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "client.demo1@sgpfreelancing.local");
            if (clientUser == null)
            {
                Log.Error("Client demo user not found. Run --seed-dummy-email-data first");
                return;
            }

            var freelancerUser1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "freelancer.demo1@sgpfreelancing.local");
            var freelancerUser2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "freelancer.demo2@sgpfreelancing.local");

            if (freelancerUser1 == null || freelancerUser2 == null)
            {
                Log.Error("Freelancer demo users not found. Run --seed-dummy-email-data first");
                return;
            }

            var clientProfile = await context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == clientUser.Id);
            var freelancerProfile1 = await context.FreelancerProfiles.FirstOrDefaultAsync(p => p.UserId == freelancerUser1.Id);
            var freelancerProfile2 = await context.FreelancerProfiles.FirstOrDefaultAsync(p => p.UserId == freelancerUser2.Id);

            if (clientProfile == null || freelancerProfile1 == null || freelancerProfile2 == null)
            {
                Log.Error("Client or freelancer profiles not found. Run --seed-dummy-email-data first");
                return;
            }

            var skillNames = new[] { "Python", "Machine Learning", "SQL", "ASP.NET Core", "React", "JavaScript", "HTML/CSS", "Django" };
            var skills = await context.Skills.Where(s => skillNames.Contains(s.Name)).ToListAsync();

            async Task<Skill> GetSkillAsync(string name)
            {
                var skill = skills.FirstOrDefault(s => s.Name == name);
                if (skill != null)
                {
                    return skill;
                }

                skill = await context.Skills.FirstOrDefaultAsync(s => s.Name == name);
                if (skill != null)
                {
                    skills.Add(skill);
                    return skill;
                }

                Log.Warning("Skill '{SkillName}' not found, creating it dynamically", name);
                skill = new Skill
                {
                    Name = name,
                    Description = $"Auto-created skill: {name}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Skills.AddAsync(skill);
                await context.SaveChangesAsync();
                skills.Add(skill);
                return skill;
            }

            async Task EnsureFreelancerSkillAsync(int freelancerProfileId, string skillName, int years)
            {
                var skill = await GetSkillAsync(skillName);
                var exists = await context.FreelancerSkills.AnyAsync(fs => fs.FreelancerProfileId == freelancerProfileId && fs.SkillId == skill.Id);
                if (!exists)
                {
                    await context.FreelancerSkills.AddAsync(new FreelancerSkill
                    {
                        FreelancerProfileId = freelancerProfileId,
                        SkillId = skill.Id,
                        YearsOfExperience = years
                    });
                }
            }

            async Task EnsureProjectSkillAsync(int projectId, string skillName)
            {
                var skill = await GetSkillAsync(skillName);
                var exists = await context.ProjectSkills.AnyAsync(ps => ps.ProjectId == projectId && ps.SkillId == skill.Id);
                if (!exists)
                {
                    await context.ProjectSkills.AddAsync(new ProjectSkill
                    {
                        ProjectId = projectId,
                        SkillId = skill.Id
                    });
                }
            }

            await EnsureFreelancerSkillAsync(freelancerProfile1.Id, "Python", 4);
            await EnsureFreelancerSkillAsync(freelancerProfile1.Id, "Django", 3);
            await EnsureFreelancerSkillAsync(freelancerProfile1.Id, "SQL", 4);
            await EnsureFreelancerSkillAsync(freelancerProfile2.Id, "React", 4);
            await EnsureFreelancerSkillAsync(freelancerProfile2.Id, "JavaScript", 5);
            await EnsureFreelancerSkillAsync(freelancerProfile2.Id, "HTML/CSS", 5);

            var webCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Web Development");
            if (webCategory == null)
            {
                webCategory = new Category
                {
                    Name = "Web Development",
                    Description = "Build websites and web applications",
                    IconClass = "fa-globe",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Categories.AddAsync(webCategory);
                await context.SaveChangesAsync();
                Log.Warning("Web Development category created dynamically");
            }

            var aiCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Data Science & Analytics");
            if (aiCategory == null)
            {
                aiCategory = new Category
                {
                    Name = "Data Science & Analytics",
                    Description = "Data analysis, machine learning, AI",
                    IconClass = "fa-chart-bar",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Categories.AddAsync(aiCategory);
                await context.SaveChangesAsync();
                Log.Warning("Data Science & Analytics category created dynamically");
            }

            var project1 = await context.Projects.FirstOrDefaultAsync(p => p.Title == "AI Job Matching Dashboard");
            if (project1 == null)
            {
                project1 = new Project
                {
                    Title = "AI Job Matching Dashboard",
                    Description = "Build a dashboard that recommends jobs to freelancers using profile skills and project metadata.",
                    Budget = 45000,
                    Deadline = DateTime.UtcNow.AddDays(21),
                    Status = ProjectStatus.Open,
                    ClientId = clientUser.Id,
                    CategoryId = aiCategory.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Projects.AddAsync(project1);
                await context.SaveChangesAsync();

                await EnsureProjectSkillAsync(project1.Id, "Python");
                await EnsureProjectSkillAsync(project1.Id, "Machine Learning");
                await EnsureProjectSkillAsync(project1.Id, "SQL");
            }

            var project2 = await context.Projects.FirstOrDefaultAsync(p => p.Title == "Freelance Marketplace Frontend Refresh");
            if (project2 == null)
            {
                project2 = new Project
                {
                    Title = "Freelance Marketplace Frontend Refresh",
                    Description = "Redesign the client and freelancer landing pages with a clean responsive UI and better search flows.",
                    Budget = 30000,
                    Deadline = DateTime.UtcNow.AddDays(14),
                    Status = ProjectStatus.Open,
                    ClientId = clientUser.Id,
                    CategoryId = webCategory.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Projects.AddAsync(project2);
                await context.SaveChangesAsync();

                await EnsureProjectSkillAsync(project2.Id, "React");
                await EnsureProjectSkillAsync(project2.Id, "JavaScript");
                await EnsureProjectSkillAsync(project2.Id, "HTML/CSS");
            }

            await context.SaveChangesAsync();

            var bid1 = await context.Bids.FirstOrDefaultAsync(b => b.ProjectId == project1.Id && b.FreelancerId == freelancerUser1.Id);
            if (bid1 == null)
            {
                bid1 = new Bid
                {
                    FreelancerId = freelancerUser1.Id,
                    ProjectId = project1.Id,
                    ProposedAmount = 42000,
                    EstimatedDurationDays = 18,
                    CoverLetter = "I can build the AI matching dashboard with project scoring, recommendation cards, and admin insights.",
                    Status = BidStatus.Accepted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Bids.AddAsync(bid1);
            }

            var bid2 = await context.Bids.FirstOrDefaultAsync(b => b.ProjectId == project2.Id && b.FreelancerId == freelancerUser2.Id);
            if (bid2 == null)
            {
                bid2 = new Bid
                {
                    FreelancerId = freelancerUser2.Id,
                    ProjectId = project2.Id,
                    ProposedAmount = 28000,
                    EstimatedDurationDays = 12,
                    CoverLetter = "I can refresh the marketplace frontend with responsive layouts and a polished browse experience.",
                    Status = BidStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Bids.AddAsync(bid2);
            }

            await context.SaveChangesAsync();

            var contract = await context.Contracts.FirstOrDefaultAsync(c => c.ProjectId == project1.Id);
            if (contract == null)
            {
                contract = new Contract
                {
                    ProjectId = project1.Id,
                    ClientId = clientUser.Id,
                    FreelancerId = freelancerUser1.Id,
                    AgreedAmount = 42000,
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EndDate = DateTime.UtcNow.AddDays(18),
                    Terms = "Demo contract for AI dashboard delivery and testing.",
                    Status = ContractStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Contracts.AddAsync(contract);
                await context.SaveChangesAsync();

                await context.PaymentTransactions.AddAsync(new PaymentTransaction
                {
                    ContractId = contract.Id,
                    Amount = 12600,
                    Type = PaymentType.Deposit,
                    Status = PaymentStatus.Completed,
                    TransactionId = "DEMO-ESCROW-001",
                    Description = "Initial escrow deposit for AI dashboard project",
                    ProcessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await context.Messages.AddAsync(new Message
                {
                    SenderId = clientUser.Id,
                    ReceiverId = freelancerUser1.Id,
                    Subject = "Welcome to the demo project",
                    Content = "Please use this demo project to test messaging, contracts, and AI matching flows.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await context.Reviews.AddAsync(new Review
                {
                    ContractId = contract.Id,
                    ReviewerId = clientUser.Id,
                    RevieweeId = freelancerUser1.Id,
                    Rating = 5,
                    Comment = "Excellent demo work and quick turnaround.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                freelancerProfile1.CompletedProjects += 1;
                freelancerProfile1.AverageRating = 4.8m;
                clientProfile.TotalProjectsPosted += 2;
                clientProfile.AverageRating = 4.7m;

                await context.SaveChangesAsync();
            }

            Log.Information("Seeded dummy marketplace data for AI matching, bids, contracts, messaging, and reviews");
        }
    }
}

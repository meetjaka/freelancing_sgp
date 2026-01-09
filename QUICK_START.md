# 🚀 Quick Start Guide - SGP Freelancing Platform

## Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB included with Visual Studio)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone/Open Project
```bash
cd F:\Freelance\freelancing_sgp
```

### 2. Restore Packages
```bash
dotnet restore
```

### 3. Database is Already Migrated ✅
The database has been created and seeded with:
- 6 Categories (Web Dev, Mobile, Design, etc.)
- 20 Skills (C#, React, Python, etc.)
- Admin user account
- All tables with relationships

### 4. Run the Application
```bash
dotnet run
```

### 5. Access the Platform
- **Web App**: https://localhost:7284
- **API Docs**: https://localhost:7284/api/docs
- **Admin Login**: 
  - Email: `admin@sgpfreelancing.com`
  - Password: `Admin@123`

---

## 🏗️ Project Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  Controllers (MVC) │ API Controllers │ Razor Views │ Models │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Service Layer (TODO)                    │
│    Business Logic │ Validation │ DTOs │ AutoMapper          │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Repository Layer ✅                      │
│  Unit of Work │ Repositories │ Specifications                │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Access Layer ✅                    │
│    EF Core │ DbContext │ Entities │ Migrations              │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     SQL Server Database ✅                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 📋 Next Development Tasks

### Priority 1: Authentication Controllers & Views
**Create AccountController:**
```csharp
// Controllers/AccountController.cs
[Route("Account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    // Implement:
    // - Register (GET/POST)
    // - Login (GET/POST)
    // - Logout
    // - Profile (GET/POST)
}
```

**Create Views:**
- Views/Account/Register.cshtml
- Views/Account/Login.cshtml
- Views/Account/Profile.cshtml

### Priority 2: Project Management
**Create ProjectController:**
```csharp
// Controllers/ProjectController.cs
[Authorize]
public class ProjectController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    // Implement:
    // - Index (List all projects)
    // - Details (Project details with bids)
    // - Create (POST project)
    // - Edit (Update project)
    // - Delete (Soft delete)
}
```

**Create Views:**
- Views/Project/Index.cshtml
- Views/Project/Details.cshtml
- Views/Project/Create.cshtml
- Views/Project/Edit.cshtml

### Priority 3: Bidding System
**Create BidController:**
```csharp
// Controllers/BidController.cs
[Authorize(Roles = "Freelancer")]
public class BidController : Controller
{
    // Implement:
    // - Create (Submit bid)
    // - MyBids (Freelancer's bids)
    // - Accept (Client accepts bid)
    // - Reject (Client rejects bid)
}
```

---

## 🔑 Key Classes & Usage

### Using Unit of Work
```csharp
public class ProjectController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<IActionResult> Index()
    {
        var projects = await _unitOfWork.Projects.GetOpenProjectsAsync();
        return View(projects);
    }
    
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var project = _mapper.Map<Project>(dto);
        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
```

### Using AutoMapper
```csharp
// Entity to DTO
var projectDto = _mapper.Map<ProjectDto>(project);

// DTO to Entity
var project = _mapper.Map<Project>(createProjectDto);

// Collection mapping
var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
```

### Repository Pattern
```csharp
// Generic operations
var project = await _unitOfWork.Projects.GetByIdAsync(id);
var allProjects = await _unitOfWork.Projects.GetAllAsync();
var filtered = await _unitOfWork.Projects.FindAsync(p => p.Status == ProjectStatus.Open);

// Specific operations
var openProjects = await _unitOfWork.Projects.GetOpenProjectsAsync();
var projectWithBids = await _unitOfWork.Projects.GetProjectWithBidsAsync(id);
```

### Transaction Management
```csharp
try
{
    await _unitOfWork.BeginTransactionAsync();
    
    // Multiple operations
    await _unitOfWork.Projects.AddAsync(project);
    await _unitOfWork.Bids.AddAsync(bid);
    
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## 🎨 Sample Controller (Full Example)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProjectController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _unitOfWork.Projects.GetOpenProjectsAsync();
                var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
                var categories = await _unitOfWork.Categories.GetAllAsync();
                
                var viewModel = new ProjectListViewModel
                {
                    Projects = projectDtos,
                    Categories = _mapper.Map<List<CategoryDto>>(categories)
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects");
                return View("Error");
            }
        }

        [AllowAnonymous]
        [Route("Project/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _unitOfWork.Projects.GetProjectWithBidsAsync(id);
            
            if (project == null)
                return NotFound();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var viewModel = new ProjectDetailsViewModel
            {
                Project = _mapper.Map<ProjectDto>(project),
                Bids = _mapper.Map<List<BidDto>>(project.Bids),
                IsOwner = project.ClientId == userId,
                CanBid = User.IsInRole("Freelancer") && project.ClientId != userId
            };
            
            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var skills = await _unitOfWork.Skills.GetAllAsync();
            
            var viewModel = new CreateProjectViewModel
            {
                Categories = _mapper.Map<List<CategoryDto>>(categories),
                Skills = _mapper.Map<List<SkillDto>>(skills)
            };
            
            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var project = _mapper.Map<Project>(dto);
                project.ClientId = userId;
                
                await _unitOfWork.Projects.AddAsync(project);
                await _unitOfWork.SaveChangesAsync();
                
                TempData["Success"] = "Project created successfully!";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                ModelState.AddModelError("", "Error creating project");
                return View(dto);
            }
        }
    }
}
```

---

## 🗂️ Sample View (Razor)

```cshtml
@model SGP_Freelancing.Models.ViewModels.ProjectListViewModel
@{
    ViewData["Title"] = "Browse Projects";
}

<div class="container mt-4">
    <div class="row">
        <div class="col-md-12">
            <h2>Available Projects</h2>
            <hr />
        </div>
    </div>

    <div class="row">
        <!-- Filters -->
        <div class="col-md-3">
            <h5>Categories</h5>
            <ul class="list-group">
                @foreach (var category in Model.Categories)
                {
                    <li class="list-group-item">
                        <a href="?categoryId=@category.Id">@category.Name</a>
                    </li>
                }
            </ul>
        </div>

        <!-- Projects -->
        <div class="col-md-9">
            <div class="row">
                @foreach (var project in Model.Projects)
                {
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body">
                                <h5 class="card-title">@project.Title</h5>
                                <p class="card-text">@project.Description.Substring(0, Math.Min(150, project.Description.Length))...</p>
                                <p class="text-muted">
                                    <i class="fa fa-money"></i> Budget: $@project.Budget
                                    <br />
                                    <i class="fa fa-tag"></i> @project.CategoryName
                                    <br />
                                    <i class="fa fa-comments"></i> @project.BidsCount Bids
                                </p>
                                <a asp-action="Details" asp-route-id="@project.Id" class="btn btn-primary">View Details</a>
                            </div>
                        </div>
                    </div>
                }
            </div>
        </div>
    </div>
</div>
```

---

## 🔐 Authentication Examples

### Check if User is Authenticated
```csharp
if (User.Identity?.IsAuthenticated ?? false)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = User.FindFirstValue(ClaimTypes.Email);
}
```

### Check User Role
```csharp
if (User.IsInRole("Admin"))
{
    // Admin-specific logic
}
```

### Authorize Attribute
```csharp
[Authorize] // Any authenticated user
[Authorize(Roles = "Admin")] // Only admins
[Authorize(Roles = "Client,Freelancer")] // Clients or Freelancers
[Authorize(Policy = "RequireAdminRole")] // Using policy
```

---

## 📚 Useful Commands

### Entity Framework
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

### Build & Run
```bash
# Clean
dotnet clean

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Watch (auto-rebuild)
dotnet watch run
```

---

## 🐛 Troubleshooting

### Database Issues
```bash
# Drop database and recreate
dotnet ef database drop --force
dotnet ef database update
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Port Already in Use
Edit `Properties/launchSettings.json` to change ports

---

## 📖 Additional Resources

- **Project Documentation**: See MODERNIZATION_REPORT.md
- **ASP.NET Core Docs**: https://docs.microsoft.com/aspnet/core
- **EF Core Docs**: https://docs.microsoft.com/ef/core
- **Bootstrap 5**: https://getbootstrap.com/docs/5.0

---

## 💡 Tips

1. **Use AutoMapper** for all DTO conversions
2. **Always use async/await** for database operations
3. **Log important actions** using ILogger
4. **Validate input** with Data Annotations and FluentValidation
5. **Use TempData** for success/error messages between redirects
6. **Implement pagination** for large lists
7. **Add search functionality** for better UX
8. **Use ViewModels** instead of DTOs in views
9. **Implement proper error handling**
10. **Test with different user roles**

---

## ✅ Checklist for New Features

- [ ] Create Controller
- [ ] Add Authorization attributes
- [ ] Inject dependencies (IUnitOfWork, IMapper, ILogger)
- [ ] Implement actions (GET/POST)
- [ ] Add model validation
- [ ] Create/Update Views
- [ ] Add navigation links
- [ ] Test with different roles
- [ ] Add logging
- [ ] Handle errors gracefully
- [ ] Update documentation

---

## 🚀 Start Building!

The foundation is ready. Pick a task from Priority 1 and start coding! 🎉
```

Good luck with your freelancing platform development! 💪

# SGP Freelancing Platform — Project Reference

> **Last Updated:** March 4, 2026 | **Framework:** ASP.NET Core 8.0 MVC | **CSS:** Tailwind 3.4.1

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8.0, C# 12, .NET 8 |
| Database | SQL Server (LocalDB dev) via EF Core 8.0 |
| Frontend | Tailwind CSS 3.4.1 (CDN), Font Awesome 6.4.0, jQuery, Google Fonts (Inter) |
| Real-time | SignalR 1.1.0 |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Logging | Serilog (Console + daily rolling files) |
| Mapping | AutoMapper 12.0.1 |
| Validation | FluentValidation 11.3.0 |
| API Docs | Swagger/Swashbuckle 6.5.0 at `/api/docs` |
| Patterns | Repository, Unit of Work, Service Layer, DI, DTOs, Middleware Pipeline |

---

## Architecture

```
Controllers/ → Services/ → Repositories/ → EF Core → SQL Server
     ↕              ↕
  Views/         DTOs/ViewModels
```

- **Generic Repository:** `IRepository<T>` / `Repository<T>` — async CRUD, LINQ, expression filtering
- **9 Specific Repos:** Project, Bid, Contract, FreelancerProfile, ClientProfile, Category, Skill, Review, Message
- **Unit of Work:** `IUnitOfWork` — transaction management, single SaveChanges

---

## Entities (20)

| Entity | Key Fields |
|--------|-----------|
| **ApplicationUser** (IdentityUser) | FirstName, LastName, ProfilePictureUrl, IsActive |
| **FreelancerProfile** | UserId, Title, Bio, HourlyRate, Availability, ExperienceYears |
| **ClientProfile** | UserId, CompanyName, BusinessType, Website |
| **Category** | Name, Description, IconClass (6 seeded) |
| **Skill** | Name, Description (20 seeded) |
| **FreelancerSkill** | FreelancerProfileId, SkillId (junction) |
| **ProjectSkill** | ProjectId, SkillId (junction) |
| **Project** | Title, Description, Budget, Deadline, Status, ClientId, CategoryId |
| **Bid** | ProjectId, FreelancerId, ProposedAmount, EstimatedDurationDays, Status |
| **Contract** | ProjectId, BidId, ClientId, FreelancerId, TotalAmount, Status |
| **PaymentTransaction** | ContractId, Amount, TransactionType, Status |
| **Review** | ContractId, ReviewerId, RevieweeId, Rating (1-5), Comment |
| **Message** | SenderId, ReceiverId, Content, IsRead, SentAt |
| **Portfolio** | FreelancerProfileId, Title, Description, IsPublic |
| **PortfolioCase** | PortfolioId, Title, Description |
| **PortfolioImage** | PortfolioCaseId, ImageUrl |
| **ProjectTestimonial** | PortfolioId, ClientName, Content, Rating |
| **FileAttachment** | FileName, FilePath, FileType, FileSize, ProjectId?, ContractId?, MessageId? |
| **OtpRecord** | Email, Otp, ExpiresAt |
| **BaseEntity** | Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted (soft delete) |

**Global query filter:** `IsDeleted == false` on all BaseEntity descendants.

---

## Services (12)

| Service | Key Methods |
|---------|------------|
| **ProjectService** | GetAllProjectsAsync (paginated+filter+search), CRUD, GetRecommended, GetByClient |
| **BidService** | Create (duplicate prevention), Accept/Reject, Withdraw, GetByProject/Freelancer |
| **ContractService** | CreateFromBid, Complete/Cancel, GetByClient/Freelancer (paginated), GetActive |
| **MessageService** | Send, GetUserMessages, GetConversation (paginated), MarkAsRead, GetUnreadCount |
| **ReviewService** | Create, GetByUser, GetByContract |
| **PortfolioService** | CRUD portfolio, cases, images, testimonials, avg rating |
| **ProfileService** | Get/Update FreelancerProfile, Get/Update ClientProfile |
| **FreelancerService** | SearchFreelancers (paginated), GetDetail, GetTopRated |
| **FileUploadService** | Upload single/multiple, Delete, GetByProject/Contract/Message |
| **EmailService** | SendOtpEmail (SMTP) |
| **OtpService** | GenerateOtp, VerifyOtp |

---

## Controllers & Actions

| Controller | Actions |
|-----------|---------|
| **HomeController** | Index, Privacy, TailwindTest, Error |
| **AccountController** | Login, Register, VerifyOtp, ResendOtp, Logout, Profile, AccessDenied |
| **DashboardController** | Index (stats: projects, messages, activity) |
| **ProjectController** | Index (paginated+search+filter), Details, Create, Edit, Delete, MyProjects, SubmitBid, AcceptBid |
| **ContractController** | Index (paginated), Details, Complete |
| **MessageController** | Index (inbox+unread), Conversation (paginated), Send (AJAX) |
| **PortfolioController** | Index, Featured, Details, MyPortfolio, Create, Edit, Delete, AddCase/EditCase/DeleteCase, AddImage/DeleteImage, AddTestimonial/DeleteTestimonial |
| **FreelancerController** | Index (search), Details |
| **ProfileController** | EditFreelancer, EditClient |
| **ReviewController** | SubmitReview |
| **AnalyticsController** | Index |
| **EarningsController** | Index |
| **SettingsController** | Index, ChangePassword |

---

## Views Structure

```
Views/
├── Shared/
│   ├── _Layout.cshtml              → Public pages (Home, Login, Register)
│   ├── _LayoutDashboard.cshtml     → Authenticated pages (sidebar nav, bento cards)
│   └── Error.cshtml
├── Home/Index.cshtml               → Landing: hero, how-it-works, categories, stats, CTA
├── Account/                        → Login, Register, VerifyOtp, Profile, AccessDenied
├── Dashboard/Index.cshtml          → Stat cards, recent projects, activity feed
├── Project/                        → Index (browse+filter), Details (bids), Create, Edit, MyProjects
├── Contract/                       → Index (table+pagination), Details (parties+review)
├── Message/                        → Index (conversation list), Conversation (SignalR chat)
├── Portfolio/                      → Index, Details, MyPortfolio, Create, Edit, AddCase, EditCase
├── Freelancer/                     → Index (search), Details
├── Profile/                        → EditFreelancer, EditClient
├── Analytics/Index.cshtml          → Metrics, charts, skills, activity table
├── Earnings/Index.cshtml           → Earnings overview
└── Settings/Index.cshtml           → Password change, preferences
```

---

## Theme System (Light/Dark Mode)

- **Tailwind Config:** `darkMode: 'class'` in CDN script (both layouts)
- **Toggle:** JavaScript localStorage-based, toggles `dark` class on `<html>`
- **Custom CSS Classes:**
  - `.bento-card` — light: white bg, subtle border | dark: `#12141a` bg, white/5 border
  - `.nav-link` — light: `#64748b` | dark: `#94a3b8`
- **Pattern:** All views use `text-slate-900 dark:text-white`, `bg-white dark:bg-[#12141a]`, etc.
- **Color Scheme:** Indigo-Violet (`#4F46E5` primary, `#7C3AED` secondary)

---

## Auth & Authorization

- **Roles:** Admin, Client, Freelancer
- **Policies:** RequireAdminRole, RequireClientRole, RequireFreelancerRole, RequireClientOrFreelancer
- **Cookie Auth** for MVC views, **JWT Bearer** for API
- **Default Admin:** `admin@sgpfreelancing.com` / `Admin@123` (auto-seeded)
- **Anti-forgery:** Header `X-CSRF-TOKEN`
- **HTTPS:** Enforced

---

## Real-time & API

**SignalR Hub:** `/chatHub` — SendMessage, NotifyTyping, NotifyOnline

**REST API:** Swagger at `/api/docs` — Projects, Bids, Messages, Auth endpoints

**CORS:** AllowSpecificOrigins policy

---

## DI Registration (Program.cs)

| Lifetime | Registrations |
|----------|--------------|
| Scoped | DbContext, IUnitOfWork, IProjectService, IBidService, IMessageService, IContractService, IReviewService, IPortfolioService, IProfileService, IFreelancerService, IFileUploadService, IEmailService |
| Singleton | OtpService |
| Framework | Identity, JWT, AutoMapper, SignalR, MVC, Swagger, CORS, Session, DataProtection |

---

## Key DTOs

| DTO | Purpose |
|-----|---------|
| `PagedResult<T>` | Pagination (Items, TotalCount, PageNumber, PageSize, TotalPages) |
| `ApiResponse<T>` | API response wrapper (Success, Message, Data, Errors) |
| `ProjectDto / CreateProjectDto / UpdateProjectDto` | Project CRUD |
| `BidDto / CreateBidDto` | Bid operations |
| `ContractDto / CreateContractDto` | Contract management |
| `MessageDto / SendMessageDto` | Messaging |
| `ReviewDto / CreateReviewDto` | Reviews |
| `FreelancerProfileDto / UpdateFreelancerProfileDto` | Freelancer profiles |
| `ClientProfileDto / UpdateClientProfileDto` | Client profiles |
| `PortfolioDto / PortfolioCaseDto / PortfolioImageDto` | Portfolio system |
| `FileAttachmentDto` | File uploads |
| `ChangePasswordDto` | Settings |

---

## Configuration

```
Connection: Server=(localdb)\mssqllocaldb;Database=SGP_FreelancingDB
JWT: Issuer=SGP_Freelancing, Audience=SGP_Freelancing_Users, Expiry=60min
Serilog: Info level → Console + File (logs/log-*.txt daily)
Session: 30 min timeout
```

**Run:** `dotnet restore && dotnet run`

---

## Migrations

| Migration | Content |
|-----------|---------|
| InitialCreate | Core entities |
| CompleteArchitectureSetup | Full schema with relationships |
| AddOtpRecordsTable | OTP verification |
| AddPortfolioFeature | Portfolio, Cases, Images, Testimonials |

---

## Seeded Data

- **6 Categories:** Web Dev, Mobile Dev, Design & Creative, Writing & Translation, Data Science & AI, Digital Marketing
- **20 Skills:** C#, ASP.NET Core, React, Angular, Python, JavaScript, TypeScript, Node.js, SQL, MongoDB, Docker, Azure, AWS, Figma, Adobe XD, Swift, Kotlin, Flutter, ML, Data Analysis
- **3 Roles:** Admin, Client, Freelancer
- **1 Admin:** admin@sgpfreelancing.com

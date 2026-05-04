# SGP Freelancing Platform

A full-stack freelancing marketplace built with ASP.NET Core 8.0 MVC, connecting clients with skilled freelancers. Features real-time messaging, portfolio showcasing, contract management, and a modern dark/light theme UI.

---

## Features

- **User Registration & Authentication** — Role-based (Client / Freelancer / Admin) with OTP email verification and a secure **Forgot Password flow**.
- **Project Marketplace** — Browse, search, filter by category, and paginate projects with a modern, high-performance UI.
- **Gig Marketplace** — Service catalog where freelancers list predefined packages with a clean, bento-style design.
- **Bidding System** — Submit proposals with duplicate prevention, accept/reject, withdraw
- **Contract Management** — Auto-created from accepted bids, complete/cancel workflows
- **Real-time Messaging** — SignalR-powered chat with typing indicators and online status
- **Portfolio System** — Freelancers showcase work with cases, images, and testimonials
- **Freelancer Directory** — Search and browse freelancer profiles with ratings
- **Dashboard** — Stats overview with active projects, messages, and recent activity
- **Analytics** — Performance metrics, success rates, top skills tracking
- **Profile Management** — Edit freelancer/client profiles with skills and experience
- **Reviews & Ratings** — Post-contract review system (1-5 stars)
- **File Uploads** — Attach files to projects, contracts, and messages
- **Dark/Light Theme** — Toggle with localStorage persistence, Tailwind `darkMode: 'class'`
- **Settings** — Password change and account preferences

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8.0, C# 12, .NET 8 |
| **Database** | SQL Server via Entity Framework Core 8.0 |
| **Frontend** | Tailwind CSS 3.4.1 (CDN), Font Awesome 6.4.0, jQuery |
| **Real-time** | SignalR 1.1.0 |
| **Auth** | ASP.NET Core Identity + JWT Bearer Tokens |
| **Logging** | Serilog (Console + Rolling File) |
| **Mapping** | AutoMapper 12.0.1 |
| **Validation** | FluentValidation 11.3.0 |
| **API Docs** | Swagger at `/api/docs` |

**Architecture:** Repository Pattern → Unit of Work → Service Layer → Controllers → Razor Views

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB works for development)

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/meetjaka/freelancing_sgp.git
cd freelancing_sgp

# 2. Restore dependencies
dotnet restore

# 3. Apply database migrations
dotnet ef database update

# 4. Run the application
dotnet run
```

The app will start on the port configured in `Properties/launchSettings.json`.

**Default Admin Account:** `admin@sgpfreelancing.com` / `Admin@123`

---

## Project Structure

```
├── Controllers/          # 13 MVC controllers
├── Services/             # 12 business logic services + interfaces
├── Repositories/         # Generic + specific repositories, Unit of Work
├── Models/
│   ├── Entities/         # 20 EF Core entities (BaseEntity with soft delete)
│   ├── DTOs/             # Data transfer objects (PagedResult, ApiResponse, etc.)
│   └── ViewModels/       # View-specific models
├── Views/
│   ├── Shared/           # _Layout.cshtml (public), _LayoutDashboard.cshtml (auth)
│   ├── Home/             # Landing page
│   ├── Account/          # Login, Register, OTP verification
│   ├── Dashboard/        # User dashboard with stats
│   ├── Project/          # Browse, create, edit, details, my projects
│   ├── Contract/         # List with pagination, details with review
│   ├── Message/          # Inbox + SignalR conversation
│   ├── Portfolio/        # CRUD portfolios, cases, images
│   ├── Freelancer/       # Search & browse freelancers
│   ├── Profile/          # Edit freelancer/client profiles
│   ├── Analytics/        # Performance metrics
│   ├── Earnings/         # Earnings overview
│   └── Settings/         # Account settings
├── Hubs/                 # ChatHub (SignalR)
├── Mapping/              # AutoMapper profiles
├── Middleware/            # Global exception handling
├── Migrations/           # EF Core migrations
├── Data/                 # DbContext with seed data
└── wwwroot/              # Static assets
```

---

## Configuration

Update `appsettings.json` for your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SGP_FreelancingDB;Trusted_Connection=true"
  },
  "JwtSettings": {
    "Issuer": "SGP_Freelancing",
    "Audience": "SGP_Freelancing_Users",
    "ExpiryInMinutes": 60
  }
}
```

---

## Seeded Data

- **6 Categories:** Web Dev, Mobile Dev, Design & Creative, Writing & Translation, Data Science & AI, Digital Marketing
- **20 Skills:** C#, ASP.NET Core, React, Angular, Python, JavaScript, TypeScript, Node.js, SQL, MongoDB, Docker, Azure, AWS, Figma, Adobe XD, Swift, Kotlin, Flutter, ML, Data Analysis
- **3 Roles:** Admin, Client, Freelancer

---

## API Endpoints

Swagger UI available at `/api/docs` with JWT authentication support.

Key endpoints: Projects, Bids, Messages, Auth (register/login/logout).

**SignalR Hub:** `/chatHub` — Real-time messaging with typing indicators and online status.

---

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Commit changes: `git commit -m "Add your feature"`
3. Push: `git push origin feature/your-feature`
4. Open a Pull Request

---

## License

This project is licensed under the MIT License.

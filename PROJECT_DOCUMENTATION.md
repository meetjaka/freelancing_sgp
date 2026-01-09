# SGP Freelancing Platform - Complete Documentation

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Tech Stack](#tech-stack)
3. [Architecture & Design Patterns](#architecture--design-patterns)
4. [Project Structure](#project-structure)
5. [Database Schema](#database-schema)
6. [Features & Pages](#features--pages)
7. [API Endpoints](#api-endpoints)
8. [UI/UX Design](#uiux-design)
9. [Configuration](#configuration)

---

## 🎯 Project Overview

**SGP Freelancing Platform** is a modern, full-stack freelancing marketplace built with ASP.NET Core 8.0 MVC. It connects clients with freelancers, enabling project posting, bidding, contract management, real-time messaging, and payment processing.

### Key Functionalities
- **User Management**: Registration, Login, Role-based access (Admin, Client, Freelancer)
- **Project Management**: Create, browse, search, and filter projects
- **Bidding System**: Freelancers bid on projects, clients accept bids
- **Contract Management**: Track project contracts from start to completion
- **Real-time Messaging**: SignalR-powered chat between users
- **Payment Processing**: Transaction tracking and payment management
- **Review System**: Rate and review completed work

---

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C# (.NET 8)
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB for development)
- **Authentication**: ASP.NET Core Identity + JWT Bearer Tokens
- **Real-time Communication**: SignalR 1.1.0
- **Logging**: Serilog 8.0.0
- **Object Mapping**: AutoMapper 12.0.1
- **Validation**: FluentValidation 11.3.0
- **API Documentation**: Swagger/Swashbuckle 6.5.0

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Icons**: Font Awesome 6.4.0
- **CSS**: Custom CSS with modern gradients (purple/blue theme)
- **JavaScript**: jQuery, Bootstrap JS, SignalR Client
- **Fonts**: Google Fonts (Inter)

### Development Tools
- **IDE**: Visual Studio Code / Visual Studio 2022
- **Version Control**: Git
- **Containerization**: Docker support

---

## 🏗️ Architecture & Design Patterns

### 1. **Repository Pattern**
- Generic Repository (`Repository<T>`)
- Specific Repositories for complex queries
- Abstracts data access layer

### 2. **Unit of Work Pattern**
- Manages transactions across multiple repositories
- Ensures data consistency
- Single `SaveChanges()` operation

### 3. **Service Layer Pattern**
- Business logic separated from controllers
- Services: ProjectService, BidService, MessageService, ContractService
- Promotes reusability and testability

### 4. **Dependency Injection**
- All services registered in `Program.cs`
- Constructor injection throughout the application

### 5. **DTO (Data Transfer Objects)**
- Separate DTOs for Create, Update, and View operations
- AutoMapper for Entity ↔ DTO conversion

### 6. **Middleware Pipeline**
- Global exception handling
- Request/response logging
- Authentication & Authorization

---

## 📁 Project Structure

```
SGP_Freelancing/
├── Controllers/                    # MVC Controllers
│   ├── HomeController.cs          # Landing page, public pages
│   ├── AccountController.cs       # Login, Register, Logout
│   ├── DashboardController.cs     # User dashboard with stats
│   ├── ProjectController.cs       # CRUD operations for projects
│   └── MessageController.cs       # Messaging & chat
│
├── Models/                         # Data models
│   ├── Entities/                  # Database entities
│   │   ├── ApplicationUser.cs     # Custom Identity user
│   │   ├── Project.cs             # Project entity
│   │   ├── Bid.cs                 # Bid entity
│   │   ├── Contract.cs            # Contract entity
│   │   ├── Message.cs             # Message entity
│   │   ├── Review.cs              # Review entity
│   │   ├── Category.cs            # Project categories
│   │   ├── Skill.cs               # Skills
│   │   ├── FreelancerProfile.cs   # Freelancer details
│   │   ├── ClientProfile.cs       # Client details
│   │   ├── PaymentTransaction.cs  # Payment tracking
│   │   └── BaseEntity.cs          # Base class with audit fields
│   │
│   ├── DTOs/                      # Data Transfer Objects
│   │   └── ApplicationDTOs.cs     # All DTOs (Project, Bid, Message, etc.)
│   │
│   └── ViewModels/                # View-specific models
│
├── Views/                          # Razor views
│   ├── Shared/
│   │   ├── _Layout.cshtml         # Public layout (guests)
│   │   ├── _LayoutDashboard.cshtml # Dashboard layout (logged-in users)
│   │   └── Error.cshtml           # Error page
│   │
│   ├── Home/
│   │   ├── Index.cshtml           # Landing page
│   │   └── Privacy.cshtml         # Privacy policy
│   │
│   ├── Account/
│   │   ├── Login.cshtml           # Login page
│   │   └── Register.cshtml        # Registration page
│   │
│   ├── Dashboard/
│   │   └── Index.cshtml           # User dashboard
│   │
│   ├── Project/
│   │   ├── Index.cshtml           # Browse projects
│   │   ├── Details.cshtml         # Project details & bidding
│   │   └── Create.cshtml          # Create new project
│   │
│   └── Message/
│       ├── Index.cshtml           # Message inbox
│       └── Conversation.cshtml    # Chat interface
│
├── Services/                       # Business logic layer
│   ├── Interfaces/
│   │   ├── IProjectService.cs
│   │   ├── IBidService.cs
│   │   ├── IMessageService.cs
│   │   └── IContractService.cs
│   │
│   ├── ProjectService.cs          # Project business logic
│   ├── BidService.cs              # Bidding business logic
│   ├── MessageService.cs          # Messaging business logic
│   └── ContractService.cs         # Contract management logic
│
├── Repositories/                   # Data access layer
│   ├── Interfaces/
│   │   ├── IRepository.cs         # Generic repository interface
│   │   ├── IProjectRepository.cs
│   │   ├── IBidRepository.cs
│   │   └── ...
│   │
│   ├── Repository.cs              # Generic repository implementation
│   ├── SpecificRepositories.cs    # All specific repositories
│   ├── UnitOfWork.cs              # Unit of Work implementation
│   └── IUnitOfWork.cs             # Unit of Work interface
│
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
│
├── Hubs/
│   └── ChatHub.cs                 # SignalR hub for real-time chat
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Global exception handler
│
├── Mapping/
│   └── MappingProfile.cs          # AutoMapper configuration
│
├── Utilities/
│   └── Constants.cs               # Application constants
│
├── Migrations/                     # EF Core migrations
│
├── wwwroot/                        # Static files
│   ├── css/
│   │   └── site.css               # Custom CSS (modern purple/blue theme)
│   ├── js/
│   │   └── site.js                # Custom JavaScript
│   └── lib/                       # Third-party libraries
│       ├── bootstrap/
│       ├── jquery/
│       └── jquery-validation/
│
├── Program.cs                      # Application entry point
├── appsettings.json               # Configuration
└── SGP_Freelancing.csproj         # Project file
```

---

## 💾 Database Schema

### **14 Entity Tables:**

#### 1. **AspNetUsers** (Identity - ApplicationUser)
```csharp
- Id (PK)
- UserName
- Email
- PasswordHash
- FirstName
- LastName
- ProfilePictureUrl
- CreatedAt
- UpdatedAt
- IsActive
```

#### 2. **Categories**
```csharp
- Id (PK)
- Name
- Description
- IconClass
- CreatedAt, UpdatedAt, IsDeleted
```

#### 3. **Skills**
```csharp
- Id (PK)
- Name
- Description
- CreatedAt, UpdatedAt, IsDeleted
```

#### 4. **ClientProfiles**
```csharp
- Id (PK)
- UserId (FK -> ApplicationUser)
- CompanyName
- BusinessType
- Website
- Description
- CreatedAt, UpdatedAt, IsDeleted
```

#### 5. **FreelancerProfiles**
```csharp
- Id (PK)
- UserId (FK -> ApplicationUser)
- Title
- Bio
- HourlyRate
- Availability
- ExperienceYears
- PortfolioUrl
- CreatedAt, UpdatedAt, IsDeleted
```

#### 6. **Projects**
```csharp
- Id (PK)
- Title
- Description
- Budget
- Deadline
- Status (Open/InProgress/Completed/Cancelled)
- ClientId (FK -> ApplicationUser)
- CategoryId (FK -> Category)
- CreatedAt, UpdatedAt, IsDeleted
```

#### 7. **Bids**
```csharp
- Id (PK)
- ProjectId (FK -> Project)
- FreelancerId (FK -> ApplicationUser)
- ProposedAmount
- EstimatedDurationDays
- CoverLetter
- Status (Pending/Accepted/Rejected)
- CreatedAt, UpdatedAt, IsDeleted
```

#### 8. **Contracts**
```csharp
- Id (PK)
- ProjectId (FK -> Project)
- BidId (FK -> Bid)
- ClientId (FK -> ApplicationUser)
- FreelancerId (FK -> ApplicationUser)
- StartDate
- EndDate
- Status (Active/Completed/Cancelled)
- TotalAmount
- CreatedAt, UpdatedAt, IsDeleted
```

#### 9. **Messages**
```csharp
- Id (PK)
- SenderId (FK -> ApplicationUser)
- ReceiverId (FK -> ApplicationUser)
- Content
- IsRead
- SentAt
- ReadAt
- CreatedAt, UpdatedAt, IsDeleted
```

#### 10. **Reviews**
```csharp
- Id (PK)
- ContractId (FK -> Contract)
- ReviewerId (FK -> ApplicationUser)
- RevieweeId (FK -> ApplicationUser)
- Rating (1-5)
- Comment
- CreatedAt, UpdatedAt, IsDeleted
```

#### 11. **PaymentTransactions**
```csharp
- Id (PK)
- ContractId (FK -> Contract)
- Amount
- TransactionType (Payment/Refund)
- Status (Pending/Completed/Failed)
- TransactionDate
- PaymentMethod
- CreatedAt, UpdatedAt, IsDeleted
```

#### 12. **ProjectSkills** (Many-to-Many)
```csharp
- ProjectId (FK -> Project)
- SkillId (FK -> Skill)
```

#### 13. **FreelancerSkills** (Many-to-Many)
```csharp
- FreelancerProfileId (FK -> FreelancerProfile)
- SkillId (FK -> Skill)
```

### **Relationships:**
- User ↔ ClientProfile (1:1)
- User ↔ FreelancerProfile (1:1)
- User ↔ Projects (1:Many) [Client creates projects]
- User ↔ Bids (1:Many) [Freelancer creates bids]
- Project ↔ Bids (1:Many)
- Project ↔ Category (Many:1)
- Bid ↔ Contract (1:1)
- Contract ↔ Reviews (1:2) [Client & Freelancer can review each other]
- Contract ↔ PaymentTransactions (1:Many)
- Project ↔ Skills (Many:Many via ProjectSkills)
- FreelancerProfile ↔ Skills (Many:Many via FreelancerSkills)
- User ↔ Messages (1:Many as Sender, 1:Many as Receiver)

---

## 🎨 Features & Pages

### **1. Public Pages (Guest Users)**

#### **Home Page** (`/` or `/Home/Index`)
**Design:**
- Full-width purple-to-blue gradient hero section
- Floating briefcase icon animation
- Main heading: "Find the Perfect Freelancer for Your Project"
- Two CTA buttons: "Browse Projects" and "Post a Project"
- "How It Works" section with 3 cards
- Statistics showcase (1000+ projects, 5000+ freelancers)
- Popular categories grid (6 categories with icons)
- Footer with social links

**Features:**
- Responsive design
- Smooth animations
- Clear value proposition

#### **Login Page** (`/Account/Login`)
**Design:**
- Centered card with purple gradient icon
- Email & password fields with FontAwesome icons
- "Remember me" checkbox
- "Forgot password?" link
- Social login buttons (Google, Facebook, GitHub)
- Link to registration page

**Features:**
- Form validation
- Secure authentication via ASP.NET Identity
- Remember me functionality
- Redirects to Dashboard after successful login

#### **Register Page** (`/Account/Register`)
**Design:**
- Centered card with user-plus gradient icon
- Fields: First Name, Last Name, Email, Password, Confirm Password
- Role selection: Freelancer or Client (radio buttons with descriptions)
- Terms of Service checkbox
- Sign Up button

**Features:**
- Client-side & server-side validation
- Password strength requirements
- Role assignment during registration
- Auto-login after registration

---

### **2. Authenticated Pages (Logged-in Users)**

#### **Dashboard** (`/Dashboard/Index`)
**Design:**
- **Sidebar Navigation** (left side, fixed):
  - Dashboard
  - Projects
  - My Projects
  - Messages (with unread badge)
  - Analytics
  - Earnings
  - Profile
  - Settings
  - Logout

- **Top Bar**:
  - Search bar
  - Notification icon with badge
  - User avatar dropdown

- **Main Content**:
  - Welcome message with user name
  - 4 Stat Cards:
    - Total Projects (purple icon)
    - Active Projects (blue icon)
    - Completed Projects (green icon)
    - Messages (orange icon)
  - Project Status Overview (bar chart style)
  - Recent Projects table
  - Recent Activity feed

**Features:**
- Real-time statistics
- Quick access to all features
- Modern card-based layout
- Gradient stat cards with hover effects

#### **Browse Projects** (`/Project/Index`)
**Design:**
- Search bar and filters (category, budget range, skills)
- Grid of project cards showing:
  - Title
  - Description excerpt
  - Budget
  - Status badge
  - Number of bids
  - Posted date
  - Client name
  - Required skills (tags)
- Pagination controls

**Features:**
- Search by keywords
- Filter by category
- Sort by date, budget, bids
- Pagination
- Responsive grid layout

#### **Project Details** (`/Project/Details/{id}`)
**Design:**
- Project title & status badge
- Full description
- Budget & deadline
- Required skills (tags)
- Client information
- Bid submission form (for freelancers)
- List of all bids with:
  - Freelancer name
  - Proposed amount
  - Duration
  - Cover letter excerpt
  - Accept button (for project owner)

**Features:**
- Freelancers can place bids
- Clients can accept bids
- View all bids on project
- Bid status tracking

#### **Create Project** (`/Project/Create`)
**Design:**
- Form with fields:
  - Title
  - Description (textarea)
  - Category (dropdown)
  - Budget
  - Deadline (date picker)
  - Required Skills (multi-select)
- Submit button

**Features:**
- Form validation
- Category selection
- Skill tagging
- Date picker for deadline

#### **Messages/Inbox** (`/Message/Index`)
**Design:**
- List of conversations with:
  - User avatar
  - User name
  - Last message preview
  - Timestamp
  - Unread indicator
- Search conversations

**Features:**
- View all conversations
- Search users
- Unread message indicators

#### **Conversation/Chat** (`/Message/Conversation/{userId}`)
**Design:**
- Chat interface with:
  - User header (avatar, name, online status)
  - Message list (scrollable)
  - Message input with send button
  - Typing indicator

**Features:**
- Real-time messaging via SignalR
- Message history
- Read receipts
- Typing indicators
- Online/offline status

---

### **3. Admin Pages** (Future Implementation)
- User management
- Project monitoring
- Category/Skill management
- Payment oversight
- Platform analytics

---

## 🔌 API Endpoints (Planned/Partial)

### Projects
- `GET /api/projects` - Get all projects (with pagination)
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project (soft delete)

### Bids
- `GET /api/bids/project/{projectId}` - Get bids for project
- `POST /api/bids` - Submit bid
- `PUT /api/bids/{id}/accept` - Accept bid

### Messages
- `GET /api/messages/conversations` - Get user conversations
- `GET /api/messages/{userId}` - Get messages with specific user
- `POST /api/messages` - Send message

### Authentication
- `POST /api/account/register` - Register user
- `POST /api/account/login` - Login user
- `POST /api/account/logout` - Logout user

---

## 🎨 UI/UX Design

### **Color Scheme**
```css
/* Modern Purple-Blue Gradient Theme */
Primary Purple: #8B7BE6
Primary Blue: #4F7CFF
Dark Blue: #667eea, #2563eb
Success Green: #10B981
Warning Orange: #F97316
Danger Red: #EF4444
Background Light: #F8F9FC
Card Background: #FFFFFF
Text Primary: #1F2937
Text Secondary: #6B7280
Border Color: #E5E7EB
```

### **Typography**
- **Font Family**: Inter (Google Fonts)
- **Headings**: Bold, 700-800 weight
- **Body**: Regular, 400-500 weight
- **Small Text**: 300 weight

### **Components**

#### **Buttons**
- Rounded corners (10px border-radius)
- Gradient backgrounds for primary buttons
- Hover effects with lift animation
- Icon + text combinations

#### **Cards**
- White background
- Subtle shadow (box-shadow: 0 1px 3px rgba(0,0,0,0.05))
- 16px border-radius
- Hover effect: lift + stronger shadow

#### **Forms**
- Light gray input backgrounds
- Border-radius: 10px
- Icon prefixes for inputs
- Focus state with blue glow

#### **Navigation**
- Sidebar: Fixed, white background, 260px width
- Top navbar: Gradient background, full-width
- Active state: gradient background
- Hover effects: subtle background change

#### **Animations**
- Fade-in on page load
- Float animation for hero illustration
- Smooth transitions (0.2s ease)
- Card hover lift effect

---

## ⚙️ Configuration

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SGP_FreelancingDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration12345678",
    "Issuer": "SGP_Freelancing",
    "Audience": "SGP_Freelancing_Users",
    "ExpiryInMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

### **Database Connection**
- Uses SQL Server LocalDB for development
- Connection string in `appsettings.json`
- Migrations applied automatically on startup

### **Seeding**
- Default roles: Admin, Client, Freelancer
- Sample categories and skills
- Admin user (email: admin@sgp.com, password: Admin@123)

---

## 🚀 Running the Application

### **Prerequisites**
- .NET 8 SDK
- SQL Server LocalDB or SQL Server
- Visual Studio 2022 / VS Code

### **Steps**
1. Clone the repository
2. Open project in IDE
3. Update connection string in `appsettings.json` if needed
4. Run migrations: `dotnet ef database update`
5. Start application: `dotnet run --no-launch-profile --urls "http://localhost:5000"`
6. Navigate to `http://localhost:5000`

### **Default Credentials**
- Admin: admin@sgp.com / Admin@123
- (Create your own Client/Freelancer accounts via Register page)

---

## 📊 Current Status

### ✅ **Completed**
- All entities and database schema
- Repository + Unit of Work pattern
- Service layer with business logic
- Authentication & Authorization
- Project CRUD operations
- Bidding system
- Real-time messaging with SignalR
- Modern UI with purple/blue gradient theme
- Dashboard with statistics
- Login/Register pages
- Landing page
- Project browsing and details pages

### 🔄 **In Progress**
- File upload functionality
- Payment integration
- Email notifications

### 📋 **Todo**
- Admin dashboard
- Advanced search and filters
- User profile management
- Review system implementation
- Analytics and reporting
- Mobile responsive optimization
- Unit tests

---

## 📞 Support & Documentation

For any questions or issues:
1. Check this documentation
2. Review code comments in source files
3. Check IMPLEMENTATION_COMPLETE.md for detailed implementation notes
4. Check MODERNIZATION_REPORT.md for architecture decisions

---

**Last Updated**: January 9, 2026
**Version**: 1.0.0
**Developer**: SGP Team

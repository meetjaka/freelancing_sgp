# SGP Freelancing Platform

A modern, full-stack freelancing marketplace connecting clients with skilled professionals.

## Features

- **Futuristic Dashboard**: A glassmorphic, bento-box-inspired dashboard without sidebars, optimized for user experience.
- **Portfolios**: Freelancers can build and showcase their best work through customized portfolio cases.
- **Contract Management**: Secure creation, and management of work contracts between clients and freelancers.
- **Messaging**: Integrated messaging interface for seamless communication.
- **Analytics**: Visualize key performance metrics and success rates.

## Technologies Used

- **Framework**: ASP.NET Core MVC (C#)
- **Database**: Entity Framework / SQL Server
- **Frontend**: Tailwind CSS, Vanilla JS
- **Design Aesthetic**: Dark Bento Box styling, custom glassmorphism, responsive design

## Getting Started

### Prerequisites

- .NET 8.0 SDK or higher
- Node.js (for Tailwind CSS continuous building)
- SQL Server (LocalDB or configured instance)

### Installation

1. Clone the repository.
2. Ensure `appsettings.json` and `appsettings.Development.json` contain the correct SQL Server connection strings.
3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
4. Install npm dependencies and run Tailwind build (if modifying styles):
   ```bash
   npm install
   npm run build:css
   ```
5. Build and run the project:
   ```bash
   dotnet run
   ```

## Contributing

1. Create a feature branch (`git checkout -b feature/AmazingFeature`)
2. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
3. Push to the branch (`git push origin feature/AmazingFeature`)
4. Open a Pull Request.

## License

This project is licensed under the MIT License.

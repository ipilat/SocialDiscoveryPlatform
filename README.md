# Social Discovery Platform

A full-stack web application for social discovery, built with ASP.NET Core and Angular.

## Tech Stack

| Layer    | Technology                                    |
|----------|-----------------------------------------------|
| Backend  | ASP.NET Core 9, Entity Framework Core, SQLite |
| Frontend | Angular 20.3, Tailwind CSS                      |
| Auth     | JWT (JSON Web Tokens)                         |

## Features
- User registration and login with JWT-based authorization
- Member profiles with photos and details
- Member list, detailed view, and profile management
- Angular interceptors and route guards for secure API access
- Data seeding with Entity Framework Core migrations
- Responsive UI with Tailwind CSS

## Project Structure
```
SocialDiscoveryPlatform.sln
API/                # ASP.NET Core Web API backend
  Controllers/      # API endpoints
  Data/             # DbContext, repositories, migrations, seed data
  DTOs/             # Data transfer objects
  Entities/         # Domain models
  Interfaces/       # Repository interfaces
  Middleware/       # Custom middleware (exception handling)
  Services/         # Business logic services
client/             # Angular frontend application
  src/
    app/            # Root app module and config
    core/           # Guards, interceptors, services
    features/       # Feature modules (account, members, etc.)
    layout/         # Layout components (nav)
    shared/         # Shared components
    types/          # TypeScript interfaces
```

## API Endpoints

### Account
| Method | Endpoint              | Description           |
|--------|-----------------------|-----------------------|
| POST   | `/api/account/register` | Register a new user |
| POST   | `/api/account/login`    | Login               |

### Members
| Method | Endpoint                    | Description              |
|--------|-----------------------------|--------------------------|
| GET    | `/api/members`              | Get all members          |
| GET    | `/api/members/{id}`         | Get member by ID         |
| GET    | `/api/members/{id}/photos`  | Get photos for a member  |

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- Node.js (v18+) and npm
- Angular CLI (`npm install -g @angular/cli`)

### Backend Setup (API)
1. Navigate to the API folder:
   ```sh
   cd API
   ```
2. Restore dependencies and apply migrations:
   ```sh
   dotnet restore
   dotnet ef database update
   ```
3. Run the API:
   ```sh
   dotnet run
   ```
   The API will be available at `https://localhost:5001`.

### Frontend Setup (client)
1. Navigate to the client folder:
   ```sh
   cd client
   ```
2. Install dependencies:
   ```sh
   npm install
   ```
3. Run the Angular app:
   ```sh
   ng serve
   ```
   The app will be available at `http://localhost:4200`.

## Environment Configuration
- **API:** `appsettings.json`, `appsettings.Development.json`
- **Client:** `src/environments/environment.ts`, `src/environments/environment.development.ts`

## Development Notes
- VS Code settings are ignored via `.gitignore`
- Create new migrations: `dotnet ef migrations add <Name>`
- Database is automatically seeded on first run

## License
MIT License


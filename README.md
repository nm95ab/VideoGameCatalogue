# Video Game Catalogue

An enterprise-grade, clean-code application demonstrating **Clean Architecture**, **Onion Architecture**, **Ports & Adapters (Hexagonal Architecture)**, **Domain-Driven Design (DDD)**, and **Test-Driven Development (TDD)** using **ASP.NET Core (.NET 10)**, **EF Core Code First**, and **Angular + ng-bootstrap**.

---

## Architecture Overview

```text
VideoGameCatalogue/
├── src/
│   ├── VideoGameCatalogue.Domain/           # Onion Ring 1: Pure business logic (Entities, Value Objects, Ports)
│   ├── VideoGameCatalogue.Application/      # Onion Ring 3: Use cases, DTOs, and Business Orchestration
│   ├── VideoGameCatalogue.Infrastructure/   # Onion Ring 4: EF Core Code First (SQL Server / InMemory), Repositories
│   ├── VideoGameCatalogue.Api/              # Onion Ring 4: Driving Adapters (ASP.NET Core REST Endpoints)
│   └── VideoGameCatalogue.Client/           # Frontend (Angular, Angular Router, Bootstrap, ng-bootstrap)
│
└── tests/
    └── VideoGameCatalogue.UnitTests/        # Fast xUnit test suite (> 95% line coverage, 100% domain rules)
```

---

## Features

### Two-Page Architecture
1. **Browsing Page (`/games`)**:
   - Live search by title or description
   - Dropdown filtering by gaming platform and genre
   - Badges for Platform, Genre, and ESRB Ratings
   - Delete confirmation modal powered by `ng-bootstrap`
   - Navigation to Add and Edit entries
2. **Editing / Creation Page (`/games/:id/edit` & `/games/new`)**:
   - Reactive forms with real-time validation feedback (Bootstrap `is-invalid` / `invalid-feedback`)
   - Pre-populated platform, genre, and rating choices from metadata API
   - Numeric release year validation (1950 to current year)
   - Save (with loading state) and Cancel actions returning to the browse page

---

## 🚀 Quick Start (One Command)

Clone the repository and run the automated bootstrap script. It will verify your toolchain, start SQL Server in Docker (or fall back to In-Memory if Docker is offline), install dependencies, compile the solution, start both the backend API and Angular frontend, and open your browser:

### On macOS / Linux:
```bash
git clone https://github.com/<org>/VideoGameCatalogue.git
cd VideoGameCatalogue
./run.sh
```

### On Windows (PowerShell):
```powershell
git clone https://github.com/<org>/VideoGameCatalogue.git
cd VideoGameCatalogue
.\run.ps1
```

---

## Getting Started (Manual Steps)

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20+ or v22)
- [Docker Desktop](https://www.docker.com/) (Optional: for local SQL Server container)

### 1. Run Backend Unit Tests (TDD Verification)
```bash
dotnet test
```

### 2. Run Frontend Unit Tests (Vitest)
```bash
cd src/VideoGameCatalogue.Client
npm test -- --watch=false
```

### 3. Run the Backend API
```bash
dotnet run --project src/VideoGameCatalogue.Api
```
The API starts at `http://localhost:5111` and automatically initializes and seeds 8 classic video games.

### 4. Run the Frontend (Angular)
```bash
cd src/VideoGameCatalogue.Client
npm start
```
Open your browser at `http://localhost:4200` to browse and manage the catalogue.

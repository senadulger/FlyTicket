# System Structure and Overview

## Project Overview
This is an **ASP.NET Core 8.0 MVC** web application for an airline reservation system. It uses **SQLite** as the database and raw SQL queries for data access.

## Technology Stack
-   **Framework**: .NET 8.0 (ASP.NET Core MVC)
-   **Database**: SQLite (`Microsoft.Data.Sqlite` v9.0.10)
-   **Frontend**: Razor Views (`.cshtml`), HTML, CSS, JavaScript (in `wwwroot`)
-   **ORM/Data Access**: Raw SQL via a custom `SqliteDbHelper` class.

## Directory Structure

### Root Directory
-   `Program.cs`: Application entry point. Configures services (ControllersWithViews), database initialization (`SqliteDbHelper.Initialize()`), middleware (StaticFiles, Routing, Authorization), and default routes.
-   `appsettings.json`: Configuration file (Logging, AllowedHosts).
-   `prgmlab3.csproj`: Project file defining dependencies and target framework.
-   `prgmlab3.db`: SQLite database file.

### Controllers (`/Controllers`)
Contains the application logic.
-   `AdminController.cs`: Manages administrative tasks such as:
    -   **Users**: List users.
    -   **Planes**: List, Add, Delete planes.
    -   **Airports**: List, Add, Delete airports.
    -   **Flights**: List, Add, Delete flights.
    -   **Seats**: List, Add, Delete seats for a plane.
    -   **Reservations**: List, Cancel reservations.
-   `HomeController.cs`: Handles public-facing pages (likely Home, Index, etc.).

### Models (`/Models`)
Represents the domain entities and view models.
-   `AdminModel.cs`
-   `AirportModel.cs`: Represents an airport (Code, City, Name, Country).
-   `BaseModel.cs`: Likely a base class for other models.
-   `CustomerModel.cs`
-   `FlightModel.cs`: Represents a flight (Plane, Departure/Arrival Time & Location, Price).
-   `PlaneModel.cs`: Represents a plane (Name, Seat Count).
-   `RezervationModel.cs`: Represents a reservation.
-   `SeatModel.cs`: Represents a seat on a plane.
-   `UserModel.cs`: Represents a user.
-   `ErrorViewModel.cs`: For error display.

### Views (`/Views`)
Contains the UI (Razor pages).
-   `Admin/`: Views corresponding to `AdminController` actions.
-   `Home/`: Views corresponding to `HomeController` actions.
-   `Shared/`: Shared layouts (`_Layout.cshtml`) and partial views.
-   `_ViewImports.cshtml`: Global Razor imports.
-   `_ViewStart.cshtml`: Sets the default layout.

### Data (`/data`)
-   `SqLiteDbHelper.cs`: A static helper class for database operations.
    -   `Initialize()`: Creates tables (`users`, `planes`, `airports`, `flights`, `seats`, `reservations`) if they don't exist.
    -   `ExecuteQuery()`: Executes SELECT queries.
    -   `ExecuteNonQuery()`: Executes INSERT/UPDATE/DELETE queries.
    -   `ExecuteScalar()`: Executes queries returning a single value.
    -   `GetConnection()`: Returns a new `SqliteConnection`.

### Web Root (`/wwwroot`)
Contains static assets.
-   `css/`: Stylesheets.
-   `js/`: JavaScript files.
-   `lib/`: Client-side libraries (e.g., Bootstrap, jQuery).

## Database Schema
The database is initialized in `SqLiteDbHelper.Initialize()` with the following tables:

-   **users**: `id`, `username`, `password`, `role`, `mail`
-   **planes**: `id`, `name`, `seat_count`
-   **airports**: `id`, `code`, `city`, `name`, `country`
-   **flights**: `id`, `plane_id` (FK), `departure_time`, `arrival_time`, `price`, `departure_location` (FK), `arrival_location` (FK)
-   **seats**: `id`, `plane_id` (FK), `seat_number`, `class`
-   **reservations**: `id`, `user_id` (FK), `flight_id` (FK), `price`, `seat_id` (FK), `status`

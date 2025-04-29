# 🖖 Subspace - A Star Trek Episode API

**Subspace** is an open Star Trek episode database API and administration dashboard designed for developers, fans, and archivists alike. It includes all canonical series, detailed episode metadata, and powerful filtering via a clean RESTful API and an ASP.NET Core MVC admin frontend.

## 🚀 Features

### ✅ API (Subspace.API)
- Access to every canonical Star Trek episode
- RESTful endpoints for:
  - Episodes
  - Series
  - Tags
  - Search suggestions
  - Timeline viewing
- Filter by series, season, tags, and air date
- Self-documenting API with Swagger (Redoc enabled)
- Hosted under `/swagger` and `/redoc`
- Supports pagination and cross-series timelines
- Optimised for frontend consumption (e.g., mobile apps, SSGs, web clients)

### ✅ Web Interface (Subspace.Web)
- ASP.NET Core MVC frontend for admin access
- Admin dashboard under `/admin` with login-protected access
- Manage:
  - Episodes
  - Series
- Tailwind CSS styling
- Live search + filter support by series, season, and episode
- Authentication via ASP.NET Identity
- Role-based access control for admin-only routes

### ✅ Shared Class Library (Subspace.Shared)
- Common models and utilities shared between Subspace.API and Subspace.Web
- Provides consistent data definitions for:
  - Episodes
  - Series
  - Tags
- Centralised logic to reduce duplication across projects
- Simplifies future maintenance and extension of Subspace features

## 📁 Project Structure

```
Subspace/
├── Subspace.API/         # RESTful Star Trek episode API
├── Subspace.Web/         # Admin dashboard for managing content
├── Subspace.Shared/      # Class Library to share common models and data
└── README.md             # This file
```

## 🔧 Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core (MariaDB)
- Tailwind CSS
- Identity (with roles)
- Swagger / Redoc
- C#

## 📦 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/JeyEeSea/Subspace.git
cd Subspace
```

### 2. Database Configuration
Update your `appsettings.json` in both projects with your MariaDB connection string:

```json
"ConnectionStrings": {
  "SubspaceApiDb": "server=localhost;database=subspaceapi;user=root;password=yourpassword;"
}
```

Ensure the same database is used by both API and Web projects (schema is shared).

### 3. Run Migrations
```bash
cd Subspace.Web
dotnet ef database update
```

### 4. Seed Roles and Admin User (optional)

To automatically seed a default role and user to the database (e.g. Admin), use the following section in `appsettings.Development.json`:

```json
"SeedAdmin": {
  "Email": "youradmin@yourdomain.com",
  "Role": "Admin"
}
```

> ⚠️ Only applies in development. This logic is ignored in production by default.
> This avoids committing secrets like email addresses to version control.

### 5. Run the API and Web Projects
In Visual Studio:
- Set both **Subspace.API** and **Subspace.Web** as startup projects
- Run the solution

Or via CLI:
```bash
dotnet run --project Subspace.API
dotnet run --project Subspace.Web
```

## 🌐 Access

| Project       | URL                         |
|---------------|------------------------------|
| API Docs      | `https://localhost:xxxx/docs` or `/swagger` |
| Admin Panel   | `https://localhost:yyyy/admin`             |

## ✍️ Contributing

PRs and ideas welcome! If you’re a Star Trek fan or full stack dev looking to contribute canonical data or improve the backend/frontend — warp in.

## 📜 License

This project is open-source under the [MIT License](LICENSE).

---

🖖 *Live long and parse JSON.*

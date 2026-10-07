# Antigravity Agent Configuration for Civil Engineering App

## Developer Context & Persona
* **Primary Language:** C# (.NET 8.0 or later).
* **Familiarity:** Understands simple Python. Has NO background in TypeScript, Node.js, or advanced JavaScript frameworks.
* **Domain:** Civil Engineering (structural algorithms, calculations, material indexing, and estimation).
* **Core Task:** Developing a fast, web-based business quoting and engineering analysis application with an Excel-like interface.

## Target Architecture & Tech Stack
You must strictly restrict all code generation and refactoring to the following C# ecosystem:
* **Frontend UI:** Blazor WebAssembly (WASM) for fast client-side execution of engineering algorithms.
* **Excel Component:** Use RadzenSpreadsheet or BlazorDatasheet for rich, high-performance in-browser grid editing.
* **Database Layer:** SQLite paired with Entity Framework Core (EF Core) for structured, lightweight local/cloud storage.
* **UI Design:** Tailwind CSS or Bootstrap 5 integrated natively via Blazor styling.

## Mandatory Project Directory Structure
You must respect and strictly adhere to the following project layout when generating files:

├── .agents/                 # Antigravity rule engine configs
├── src/
│   ├── EngineeringApp.Client/   # Blazor WASM Frontend project
│   │   ├── Pages/              # Web views (e.g., QuotationGrid.razor, Dashboard.razor)
│   │   ├── Shared/             # Reusable UI components
│   │   └── wwwroot/            # Static assets and CSS
│   ├── EngineeringApp.Shared/   # Shared domain models and calculation algorithms
│   │   ├── Algorithms/         # Core Civil Engineering calculations (Shoring, Steel, etc.)
│   │   └── Models/             # Quotation Data structures & Entities
│   └── EngineeringApp.Server/   # Minimal API backend for persistence (SQLite context)
│       ├── Controllers/
│       └── Data/               # EF Core AppDbContext and migrations
└── AGENTS.md                # Global developer constraints (this file)

## Code Generation & AI Interaction Rules
1. **NO JAVASCRIPT/TYPESCRIPT:** Never generate React, Angular, Vue, Node.js, or complex vanilla JS snippets unless absolutely requested for direct interop. All UI state mutations and arithmetic must be written in pure C# using Razor syntax (`@code { ... }`).
2. **Algorithm Extraction:** Keep engineering computation blocks decoupled from the UI. Place logic inside `Shared/Algorithms/` as pure C# classes to facilitate high performance and isolated testing.
3. **Simplified Databases:** When writing database interactions, stick to standard async Linq-to-Entities (EF Core) queries. Avoid raw SQL queries or complex repository patterns that complicate maintenance.
4. **Excel Grid Pattern:** Always structure grid data-binding around observable collections or reactive `Sheet` objects compatible with Blazor data-grid components. Keep cell-recalculation functions optimized for performance.

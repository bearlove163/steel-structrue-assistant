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

## Git Commit & Collaboration Rules
1. **Commit Message Specification:** 严格遵循 Conventional Commits 格式规范：`<type>(<scope>): <中文描述>` 或 `<type>: <中文描述>`。
   * **规范类型 (`<type>`)：** `feat`（新功能）、`fix`（修复缺陷）、`refactor`（代码重构）、`docs`（文档更新）、`style`（样式调整）、`test`（单元测试）、`chore`（构建/配置维护）。
   * **常用作用域 (`<scope>`)：** `pricing`（材料价格域）、`coating`（防腐与防火涂装）、`sections`（截面几何力学）、`bom`（构件装配与定额）、`cad`（CAD矢量视窗）、`portal`（统一门户）、`db`（数据库与实体）。
2. **Mandatory Chinese Description (强制中文描述):** 冒号后的提交描述正文**必须强制使用简明扼要的中文书写**，严禁使用纯英文叙述。准确传达本次变更的工程业务实质与价值。
   * 正确示例：`feat(pricing): 新增钢板五维加价规则矩阵与钢厂溢价计算`
   * 正确示例：`fix(coating): 修复超薄型防火涂料升温极限计算偏差`
   * 正确示例：`refactor(bom): 解耦截面物理型式与构件工程角色正交维度`
   * 正确示例：`docs: 在 AGENTS.md 中补充 Git 提交信息全中文规范`


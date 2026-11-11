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
├── doc/                     # 规划方案与工程设计文档归档目录 (强制：所有 /plan 文件必须存放于此)
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
5. **Plan & Specification File Archival (方案归档规则):**
   * **强制统一存放于 `doc/` 目录：** 每次通过 `/plan` 或其他交互制定的所有规划方案、架构设计、工程计算规范与实施计划 Markdown 文件（例如 `doc/01_xxx.md`），**必须直接写入项目根目录下的 `doc/` 文件夹中**，严禁存放至缓存目录、临时目录或项目外的其他路径。
   * **时间序列顺序数字前缀编号 (`01_文件名.md`)：** 存放在 `doc/` 目录下的所有方案文件，必须严格按照生成的时间先后顺序，在文件名前增加两位数字编号前缀（如 `01_fabrication_quotation_plan.md`、`02_steel_structure_assistant_refactor_plan.md` 等），以便直观按时间轴追溯工程方案的演进与迭代过程。
   * **清晰的工程业务语义：** 紧随数字前缀之后的文件名应具备明确的工程业务含义（如 `doc/05_section_calculator_plan.md`、`doc/06_gui_layout_and_temporal_architecture_plan.md`），确保团队及用户随时可在版本库中检索、追溯和复核。

## Git Commit & Collaboration Rules
1. **Commit Message Structure (提交信息双层结构):** 必须严格遵循工业级 Conventional Commits 标准结构，包含「首行简要标题」与「详细变更要点列表」：
   ```
   <type>(<scope>): <简明中文摘要 (不超过50字)>

   * <详细中文要点1: 具体的界面列/卡片/交互变更，明确工程计算公式或逻辑>
   * <详细中文要点2: 底层数据模型/算法/定额矩阵调整>
   * <详细中文要点3: 导出/审计/报表功能更新>
   * <详细中文要点4: 新增的单元测试与验证覆盖>
   ```
2. **规范类型与常用作用域：**
   * **规范类型 (`<type>`)：** `feat`（新功能）、`fix`（修复缺陷）、`refactor`（代码重构）、`docs`（文档更新）、`style`（样式调整）、`test`（单元测试）、`chore`（构建/配置维护）。
   * **常用作用域 (`<scope>`)：** `pricing`（材料价格域）、`coating`（防腐与防火涂装）、`sections`（截面几何力学）、`bom`（构件装配与定额）、`cad`（CAD矢量视窗）、`portal`（统一门户）、`db`（数据库与实体）。
3. **Mandatory Detailed Chinese Body (强制中文详细正文，严禁仅一行概括):**
   * 每次提交**严禁仅仅写一行标题**，必须在空行后使用中文分条要点（`*`）极其详细地列出本次提交具体增加或改动了哪些功能、算法公式、界面控件及测试用例。
   * **标题与正文必须 100% 使用中文书写**，严禁使用纯英文叙述。准确传达本次变更的工程业务实质与价值。
   * **标准范例：**
     ```text
     feat(pricing): 增加施工单位采办经营创效(剪刀差利润)看板与明细列

     * 在溯源工作台网格中新增“★ 采办经营创效 (剪刀差利润)”专属列：经营创效 = 追溯索赔单价 - 实际执行单价 (例如 +130 索赔 - +50 超支 = +80元/吨 净利润)
     * 顶部大盘新增第6张核心商务指标卡片，实时展示项目总承包商净创效额与吨钢平均利润
     * 在底部审计结论中集成施工单位采办套利创效综合汇总与定案说明
     * 新增单元测试精准验算 +80元/吨 剪刀差利润与全项目级商业净利润指标
     ```



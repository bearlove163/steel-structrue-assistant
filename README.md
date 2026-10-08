# 钢结构截面特性计算与涂装防火工程算量工作台 (Steel Structure Assistant)

基于 **C# (.NET 9.0) Blazor WebAssembly** 构建的高性能纯客户端钢结构截面几何/力学特性计算、应力分析与防腐涂装/防火涂料商务工程算量工作台。

---

## 🌟 核心功能特性

### 1. 结构截面几何与力学特性计算 (Section Properties Engine)
- **参数化截面覆盖**：支持 H型钢/工字钢、箱型/矩形空心管 (RHS)、圆形钢管 (CHS)、普通槽钢 (C)、等边/不等边角钢 (L)、剖分T型钢等多种截面。
- **高阶力学指标**：
  - 截面面积 $A$、理论米重 $M$、形心位置 $(Y_c, Z_c)$。
  - 主惯性矩 $I_x, I_y, I_{xy}$ 及主轴转角 $\alpha$。
  - 惯性半径 $i_x, i_y$、弹性截面模量 $W_{x}, W_{y}$、塑性截面抵抗矩 $W_{px}, W_{py}$。
  - 圣维南扭转常数 $J$、扇形惯性矩/翘曲常数 $C_w$。
- **构件长细比与受压稳定预估**：
  - 支持面内/面外计算长度系数与实际长度输入，自动解算构件长细比 $\lambda_x, \lambda_y$ 及轴心受压稳定折减系数 $\varphi$。

### 2. 交互式应力分析与双模云图渲染 (Stress Analysis & CAD Visualizer)
- **多工况内力求解**：支持轴力 $N$、双向弯矩 $M_y, M_z$、双向剪力 $V_y, V_z$ 与自由扭矩 $T$ 组合输入。
- **中和轴与应力极值**：根据平截面假定自动求解中和轴 (Neutral Axis) 解析方程，精确定位全截面最大拉应力与最大压应力点。
- **CAD 级 SVG 矢量视窗**：
  - 支持 Light / Dark 双主题自由切换。
  - 自动避让与防穿透保护衬底胶囊框（Anti-collision Badges），确保尺寸标注与形心轴线清晰互不遮挡。

### 3. 钢结构涂装防腐与防火涂料工程算量工作台
- **外表面展开面积精算**：依据截面周长与实际构件长度，精确计算展开涂装面积 ($m^2$) 与比表面积 ($m^2/t$)。
- **完整涂装体系多层配套**：
  - 底漆 (Primer)、中间漆 (Intermediate)、封闭漆 (Sealer)、面漆 (Topcoat) 及防火涂料 (Fireproof)。
  - 支持常用工业配套方案快捷预设：底漆+中间漆+面漆、底漆+面漆、底漆+中间漆+防火涂料等。
- **工业级定额与用量计算模型**：
  - 依据体积固体分 ($VS\%$)、漆浆密度 ($\rho$)、设计干膜厚度 (DFT) 与施工工艺损耗率自动解算理论与实际涂料用量 ($kg$)。
  - 联动材料采购成本与施工工费，一键生成商务报价与工程概算报表。

### 4. 工业级型钢与防腐涂料基础数据库
- **国标 (GB/T)**：GB/T 11263 热轧H型钢 (HW/HM/HN 全系列)、GB/T 706 普通工字钢 (10#~63c# 全套 34 款)、普通槽钢 (5#~40c# 全套 29 款)、等边/不等边角钢 (全套 42 款)、方矩管与圆管。
- **美标 (AISC 15th)**：W 宽翼缘梁柱 (W4~W44 全系列)、HP 桩型钢、C/MC 槽钢、L 等边角钢、HSS 方矩管。
- **欧标 (EN 10025 / DIN)**：IPE (IPE 80~750)、HEA (HE 100 A~1000 A)、HEB (HE 100 B~1000 B)、HEM (HE 100 M~1000 M)、UPN (UPN 50~400)、IPN (IPN 80~500)。
- **持久化后端**：内置 SQLite + EF Core 持久化支撑，前端支持离线极速单例引擎与双向 JSON 数据导入/导出。

---

## 🛠️ 技术栈与架构

* **语言平台**：C# 13 / .NET 9.0
* **前端框架**：Blazor WebAssembly (WASM) 纯 C# 客户端计算，无需依赖 Node.js 或第三方前端框架
* **样式设计**：Tailwind CSS / Bootstrap 5 响应式栅格布局（支持三栏式拖拽调节 20%-60%-20%）
* **持久化**：SQLite + Entity Framework Core (`EngineeringApp.Server`)
* **单元测试**：xUnit 测试套件，覆盖截面几何、应力分析与涂装算量算法

---

## 🚀 本地快速启动

### 依赖环境
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 运行方式
```powershell
# 1. 克隆仓库
git clone https://github.com/bearlove163/steel-structrue-assistant.git
cd steel-structrue-assistant

# 2. 编译并启动前端客户端 (或直接运行 .\run.ps1)
dotnet run --project src/EngineeringApp.Client/EngineeringApp.Client.csproj
```

启动后在浏览器中打开：`http://localhost:5206` 即可开始使用。

---

## 📄 许可协议
本项目采用 [MIT License](LICENSE) 开源许可。

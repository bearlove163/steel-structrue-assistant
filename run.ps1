Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  截面特性计算与力学分析工作站 (Section Property Workstation)" -ForegroundColor Green
Write-Host "  启动本地服务: http://localhost:5206" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

dotnet run --project src/EngineeringApp.Client/EngineeringApp.Client.csproj

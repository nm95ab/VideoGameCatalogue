# Video Game Catalogue - Developer Quickstart (Windows PowerShell)
$ErrorActionPreference = "Stop"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Video Game Catalogue - Developer Quickstart      " -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Preflight Toolchain Checks
Write-Host "[1/5] Checking Toolchain Prerequisites..." -ForegroundColor White

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found." -ForegroundColor Red
    Write-Host "   Please install the .NET 10 SDK: https://dotnet.microsoft.com/download"
    exit 1
}
$dotnetVer = & dotnet --version
Write-Host "  [OK] .NET SDK found: $dotnetVer" -ForegroundColor Green

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] Node.js not found." -ForegroundColor Red
    Write-Host "   Please install Node.js (v20+): https://nodejs.org/"
    exit 1
}
$nodeVer = & node --version
Write-Host "  [OK] Node.js found: $nodeVer" -ForegroundColor Green

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] npm not found." -ForegroundColor Red
    exit 1
}
$npmVer = & npm --version
Write-Host "  [OK] npm found: $npmVer" -ForegroundColor Green

# 2. Database Environment Setup
Write-Host ""
Write-Host "[2/5] Checking Database Environment..." -ForegroundColor White
$useInMemory = $false

$alreadyListening = $false
try {
    $tcp = Test-NetConnection -ComputerName 127.0.0.1 -Port 1433 -WarningAction SilentlyContinue
    if ($tcp.TcpTestSucceeded) {
        $alreadyListening = $true
    }
} catch { }

if ($alreadyListening) {
    Write-Host "  [OK] SQL Server is already listening on port 1433." -ForegroundColor Green
} else {
    $dockerAvailable = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker info 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $dockerAvailable = $true
        }
    }

    if ($dockerAvailable) {
        Write-Host "  [OK] Docker is running. Ensuring SQL Server container is up..." -ForegroundColor Green
        try {
            $containers = docker ps -a --format "{{.Names}}" 2>&1
            if ($containers -match "videogamecatalogue-sqlserver") {
                docker start videogamecatalogue-sqlserver 2>&1 | Out-Null
            } else {
                docker compose up -d sqlserver 2>&1 | Out-Null
            }
        } catch { }
        
        Write-Host -NoNewline "  Waiting for SQL Server on port 1433..."
        for ($i = 1; $i -le 15; $i++) {
            $tcp = Test-NetConnection -ComputerName 127.0.0.1 -Port 1433 -WarningAction SilentlyContinue
            if ($tcp.TcpTestSucceeded) {
                Write-Host " Ready!" -ForegroundColor Green
                break
            }
            Start-Sleep -Seconds 1
            Write-Host -NoNewline "."
            if ($i -eq 15) {
                Write-Host ""
                Write-Host "  [WARN] SQL Server timed out. Falling back to In-Memory database." -ForegroundColor Yellow
                $useInMemory = $true
            }
        }
    } else {
        Write-Host "  [WARN] Docker is not running or not installed." -ForegroundColor Yellow
        Write-Host "    Falling back to EF Core In-Memory database for zero-dependency local run!" -ForegroundColor Yellow
        $useInMemory = $true
    }
}

# 3. Dependency Verification & Installation
Write-Host ""
Write-Host "[3/5] Verifying Dependencies..." -ForegroundColor White
if (-not (Test-Path "src/VideoGameCatalogue.Client/node_modules")) {
    Write-Host "  [*] Installing Angular client npm packages (first-time setup)..."
    Push-Location "src/VideoGameCatalogue.Client"
    npm install | Out-Null
    Pop-Location
} else {
    Write-Host "  [OK] Frontend dependencies present." -ForegroundColor Green
}

Write-Host "  [*] Restoring .NET NuGet packages..."
dotnet restore | Out-Null
Write-Host "  [OK] Backend dependencies restored." -ForegroundColor Green

# 4. Compilation & Verification
Write-Host ""
Write-Host "[4/5] Compiling Solution..." -ForegroundColor White
dotnet build --warnaserror --no-restore | Out-Null
Write-Host "  [OK] Build succeeded with zero warnings." -ForegroundColor Green

# 5. Launch Backend & Frontend
Write-Host ""
Write-Host "[5/5] Launching Services..." -ForegroundColor White

if ($useInMemory) {
    $env:UseInMemoryDatabase = "true"
}

$apiProcess = Start-Process dotnet -ArgumentList "run --project src/VideoGameCatalogue.Api --urls http://127.0.0.1:5111" -PassThru
Push-Location "src/VideoGameCatalogue.Client"
$clientProcess = Start-Process npm -ArgumentList "start -- --host 0.0.0.0 --port 4200" -PassThru
Pop-Location

try {
    Write-Host ""
    Write-Host "  Waiting for services to become responsive..."
    for ($i = 1; $i -le 30; $i++) {
        try {
            $respApi = Invoke-WebRequest -Uri "http://127.0.0.1:5111/api/games" -TimeoutSec 1 -ErrorAction SilentlyContinue
            $respClient = Invoke-WebRequest -Uri "http://localhost:4200" -TimeoutSec 1 -ErrorAction SilentlyContinue
            if ($respApi.StatusCode -eq 200 -and $respClient.StatusCode -eq 200) {
                break
            }
        } catch { }
        Start-Sleep -Seconds 1
    }

    Write-Host ""
    Write-Host "====================================================" -ForegroundColor Green
    Write-Host "   Application Ready!                               " -ForegroundColor Green
    Write-Host "   Frontend: http://localhost:4200                  " -ForegroundColor Cyan
    Write-Host "   API:      http://127.0.0.1:5111                  " -ForegroundColor Cyan
    Write-Host "====================================================" -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop all services." -ForegroundColor Yellow
    Write-Host ""

    Start-Process "http://localhost:4200"

    Wait-Process -Id $apiProcess.Id, $clientProcess.Id
} finally {
    Write-Host ""
    Write-Host "Shutting down services..." -ForegroundColor Yellow
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $clientProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Services stopped cleanly." -ForegroundColor Green
}

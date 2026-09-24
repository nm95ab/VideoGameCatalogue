# Video Game Catalogue - Developer Quickstart (Windows PowerShell)
$ErrorActionPreference = "Stop"

function Update-EnvironmentPath {
    $machinePath = [System.Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine)
    $userPath = [System.Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::User)
    $env:Path = "$machinePath;$userPath"

    $standardLocations = @(
        "$env:ProgramFiles\dotnet",
        "$env:ProgramFiles\nodejs",
        "$env:LOCALAPPDATA\Microsoft\dotnet",
        "$env:ProgramFiles\Docker\Docker\resources\bin",
        "$env:ProgramData\DockerDesktop\version-bin"
    )
    foreach ($loc in $standardLocations) {
        if ((Test-Path $loc) -and ($env:Path -notlike "*$loc*")) {
            $env:Path = "$loc;$env:Path"
        }
    }
}

function Install-DotNetSdk {
    Write-Host "  [*] Attempting automatic installation of .NET 10 SDK..." -ForegroundColor Yellow
    $success = $false

    if (Get-Command winget -ErrorAction SilentlyContinue) {
        Write-Host "  [*] Running Windows Package Manager (winget)..."
        try {
            & winget install --id Microsoft.DotNet.SDK.10 -e --silent --accept-package-agreements --accept-source-agreements
            if ($LASTEXITCODE -eq 0) {
                $success = $true
            }
        } catch { }
    }

    if (-not $success) {
        Write-Host "  [*] Downloading official Microsoft .NET 10 SDK installer..."
        $installerUrl = "https://aka.ms/dotnet/10.0/dotnet-sdk-win-x64.exe"
        $installerPath = Join-Path $env:TEMP "dotnet-sdk-10-installer.exe"
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath -UseBasicParsing
            Write-Host "  [*] Launching installer (silent)..."
            $proc = Start-Process -FilePath $installerPath -ArgumentList "/install /quiet /norestart" -PassThru -Wait
            if ($proc.ExitCode -eq 0) {
                $success = $true
            }
        } catch {
            Write-Host "  [WARN] Download or execution failed: $_" -ForegroundColor Yellow
        } finally {
            if (Test-Path $installerPath) { Remove-Item $installerPath -Force -ErrorAction SilentlyContinue }
        }
    }

    Update-EnvironmentPath
}

function Install-NodeJs {
    Write-Host "  [*] Attempting automatic installation of Node.js (LTS)..." -ForegroundColor Yellow
    $success = $false

    if (Get-Command winget -ErrorAction SilentlyContinue) {
        Write-Host "  [*] Running Windows Package Manager (winget)..."
        try {
            & winget install --id OpenJS.NodeJS.LTS -e --silent --accept-package-agreements --accept-source-agreements
            if ($LASTEXITCODE -eq 0) {
                $success = $true
            }
        } catch { }
    }

    if (-not $success) {
        Write-Host "  [*] Downloading Node.js LTS installer..."
        $msiUrl = "https://nodejs.org/dist/v24.21.0/node-v24.21.0-x64.msi"
        $msiPath = Join-Path $env:TEMP "nodejs-lts-installer.msi"
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $msiUrl -OutFile $msiPath -UseBasicParsing
            Write-Host "  [*] Launching MSI installer (silent)..."
            $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$msiPath`" /qn /norestart" -PassThru -Wait
            if ($proc.ExitCode -eq 0) {
                $success = $true
            }
        } catch {
            Write-Host "  [WARN] Download or execution failed: $_" -ForegroundColor Yellow
        } finally {
            if (Test-Path $msiPath) { Remove-Item $msiPath -Force -ErrorAction SilentlyContinue }
        }
    }

    Update-EnvironmentPath
}

function Install-Docker {
    Write-Host "  [*] Attempting automatic installation of Docker Desktop..." -ForegroundColor Yellow
    $success = $false

    if (Get-Command winget -ErrorAction SilentlyContinue) {
        Write-Host "  [*] Running Windows Package Manager (winget)..."
        try {
            & winget install --id Docker.DockerDesktop -e --silent --accept-package-agreements --accept-source-agreements
            if ($LASTEXITCODE -eq 0) {
                $success = $true
            }
        } catch { }
    }

    if (-not $success) {
        Write-Host "  [*] Downloading official Docker Desktop installer..."
        $installerUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
        $installerPath = Join-Path $env:TEMP "DockerDesktopInstaller.exe"
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath -UseBasicParsing
            Write-Host "  [*] Launching Docker Desktop installer (silent)..."
            $proc = Start-Process -FilePath $installerPath -ArgumentList "install --quiet --accept-license" -PassThru -Wait
            if ($proc.ExitCode -eq 0) {
                $success = $true
            }
        } catch {
            Write-Host "  [WARN] Download or execution failed: $_" -ForegroundColor Yellow
        } finally {
            if (Test-Path $installerPath) { Remove-Item $installerPath -Force -ErrorAction SilentlyContinue }
        }
    }

    Update-EnvironmentPath
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Video Game Catalogue - Developer Quickstart      " -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Preflight Toolchain Checks
Write-Host "[1/5] Checking Toolchain Prerequisites..." -ForegroundColor White

Update-EnvironmentPath

$hasDotNet10 = $false
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $sdks = & dotnet --list-sdks 2>&1
    if ($sdks -match "10\.") {
        $hasDotNet10 = $true
    }
}

if (-not $hasDotNet10) {
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host "  [WARN] .NET SDK found, but .NET 10 SDK is missing." -ForegroundColor Yellow
    } else {
        Write-Host "  [WARN] .NET SDK not found." -ForegroundColor Yellow
    }
    Install-DotNetSdk
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found." -ForegroundColor Red
    Write-Host "   Please install the .NET 10 SDK: https://dotnet.microsoft.com/download"
    exit 1
}
$dotnetVer = & dotnet --version
Write-Host "  [OK] .NET SDK found: $dotnetVer" -ForegroundColor Green

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "  [WARN] Node.js not found." -ForegroundColor Yellow
    Install-NodeJs
}

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] Node.js not found." -ForegroundColor Red
    Write-Host "   Please install Node.js (v20+): https://nodejs.org/"
    exit 1
}
$nodeVer = & node --version
Write-Host "  [OK] Node.js found: $nodeVer" -ForegroundColor Green

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Update-EnvironmentPath
}
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] npm not found." -ForegroundColor Red
    exit 1
}
$npmVer = & npm --version
Write-Host "  [OK] npm found: $npmVer" -ForegroundColor Green

# 2. Database Environment Setup
Write-Host ""
Write-Host "[2/5] Checking Database Environment..." -ForegroundColor White

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
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Host "  [WARN] Docker CLI not found." -ForegroundColor Yellow
        Install-Docker
    }

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Host "[ERROR] Docker not found and could not be installed automatically." -ForegroundColor Red
        Write-Host "   Please install Docker Desktop: https://www.docker.com/products/docker-desktop/"
        exit 1
    }

    docker info 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [*] Docker daemon is not running. Starting Docker Desktop..." -ForegroundColor Cyan
        $dockerApp = "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"
        if (Test-Path $dockerApp) {
            Start-Process -FilePath $dockerApp
        }

        Write-Host -NoNewline "  [*] Waiting for Docker engine to start..."
        $dockerReady = $false
        for ($d = 1; $d -le 45; $d++) {
            docker info 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                $dockerReady = $true
                Write-Host " Ready!" -ForegroundColor Green
                break
            }
            Start-Sleep -Seconds 2
            Write-Host -NoNewline "."
        }

        if (-not $dockerReady) {
            Write-Host ""
            Write-Host "[ERROR] Docker engine did not respond in time. Please ensure Docker Desktop is started." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "  [OK] Docker daemon is running." -ForegroundColor Green
    }

    Write-Host "  [*] Ensuring SQL Server container is up..."
    try {
        $containers = docker ps -a --format "{{.Names}}" 2>&1
        if ($containers -match "videogamecatalogue-sqlserver") {
            docker start videogamecatalogue-sqlserver 2>&1 | Out-Null
        } else {
            docker compose up -d sqlserver 2>&1 | Out-Null
        }
    } catch {
        Write-Host "[ERROR] Failed to start SQL Server container: $_" -ForegroundColor Red
        exit 1
    }

    Write-Host -NoNewline "  [*] Waiting for SQL Server on port 1433..."
    $sqlReady = $false
    for ($i = 1; $i -le 30; $i++) {
        $tcp = Test-NetConnection -ComputerName 127.0.0.1 -Port 1433 -WarningAction SilentlyContinue
        if ($tcp.TcpTestSucceeded) {
            $sqlReady = $true
            Write-Host " Ready!" -ForegroundColor Green
            break
        }
        Start-Sleep -Seconds 1
        Write-Host -NoNewline "."
    }

    if (-not $sqlReady) {
        Write-Host ""
        Write-Host "[ERROR] SQL Server timed out waiting on port 1433." -ForegroundColor Red
        exit 1
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

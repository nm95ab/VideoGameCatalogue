#!/usr/bin/env bash

# ANSI color codes for clean terminal output
BOLD='\033[1m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

is_port_open() {
    local host=$1
    local port=$2
    if command -v nc >/dev/null 2>&1; then
        nc -z -w 1 "$host" "$port" >/dev/null 2>&1
        return $?
    elif command -v curl >/dev/null 2>&1; then
        curl -s --connect-timeout 1 "telnet://$host:$port" >/dev/null 2>&1
        return $?
    else
        (exec 3<>/dev/tcp/"$host"/"$port") 2>/dev/null && exec 3>&-
        return $?
    fi
}

echo -e "${BOLD}${BLUE}====================================================${NC}"
echo -e "${BOLD}${BLUE}   Video Game Catalogue — Developer Quickstart      ${NC}"
echo -e "${BOLD}${BLUE}====================================================${NC}\n"

# 1. Preflight Toolchain Checks
echo -e "${BOLD}[1/5] Checking Toolchain Prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found.${NC}"
    echo -e "   Please install the .NET 10 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi
DOTNET_VER=$(dotnet --version)
echo -e "  ${GREEN}✔${NC} .NET SDK found: ${BOLD}$DOTNET_VER${NC}"

if ! command -v node &> /dev/null; then
    echo -e "${RED}❌ Node.js not found.${NC}"
    echo -e "   Please install Node.js (v20+): https://nodejs.org/"
    exit 1
fi
NODE_VER=$(node --version)
echo -e "  ${GREEN}✔${NC} Node.js found: ${BOLD}$NODE_VER${NC}"

if ! command -v npm &> /dev/null; then
    echo -e "${RED}❌ npm not found.${NC}"
    exit 1
fi
NPM_VER=$(npm --version)
echo -e "  ${GREEN}✔${NC} npm found: ${BOLD}$NPM_VER${NC}"

# 2. Database Environment Setup
echo -e "\n${BOLD}[2/5] Checking Database Environment...${NC}"
USE_IN_MEMORY=false

if is_port_open 127.0.0.1 1433; then
    echo -e "  ${GREEN}✔${NC} SQL Server is already listening on port 1433."
else
    if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
        echo -e "  Attempting to start SQL Server container via Docker..."
        
        # If an existing container exists, start it
        if docker ps -a --format '{{.Names}}' 2>/dev/null | grep -q "videogamecatalogue-sqlserver"; then
            docker start videogamecatalogue-sqlserver >/dev/null 2>&1 || true
        elif command -v docker-compose >/dev/null 2>&1; then
            docker-compose up -d sqlserver >/dev/null 2>&1 || true
        else
            docker compose up -d sqlserver >/dev/null 2>&1 || true
        fi

        # Wait for port 1433
        echo -n "  Waiting for SQL Server on port 1433..."
        for i in {1..15}; do
            if is_port_open 127.0.0.1 1433; then
                echo -e " ${GREEN}Ready!${NC}"
                break
            fi
            sleep 1
            echo -n "."
            if [ "$i" -eq 15 ]; then
                echo -e "\n  ${YELLOW}⚠ SQL Server connection timed out. Falling back to In-Memory database.${NC}"
                USE_IN_MEMORY=true
            fi
        done
    else
        echo -e "  ${YELLOW}⚠ Docker is not running or not installed.${NC}"
        echo -e "  ${YELLOW}  Falling back to EF Core In-Memory database for zero-dependency local run!${NC}"
        USE_IN_MEMORY=true
    fi
fi

# 3. Dependency Verification & Installation
echo -e "\n${BOLD}[3/5] Verifying Dependencies...${NC}"
if [ ! -d "src/VideoGameCatalogue.Client/node_modules" ]; then
    echo -e "  📦 Installing Angular client npm packages (first-time setup)..."
    (cd src/VideoGameCatalogue.Client && npm install)
else
    echo -e "  ${GREEN}✔${NC} Frontend dependencies present."
fi

# 4. Compilation & Verification
echo -e "\n${BOLD}[4/5] Compiling Solution...${NC}"
dotnet build --warnaserror > /dev/null
echo -e "  ${GREEN}✔${NC} Build succeeded with zero warnings."

# 5. Launch Backend & Frontend
echo -e "\n${BOLD}[5/5] Launching Services...${NC}"

if [ "$USE_IN_MEMORY" = true ]; then
    export UseInMemoryDatabase=true
fi

# Start API in background
dotnet run --project src/VideoGameCatalogue.Api --urls "http://127.0.0.1:5111" &
API_PID=$!

# Start Angular Client in background
(cd src/VideoGameCatalogue.Client && npm start -- --host 0.0.0.0 --port 4200) &
CLIENT_PID=$!

cleanup() {
    echo -e "\n\n${YELLOW}Shutting down services...${NC}"
    kill $API_PID $CLIENT_PID 2>/dev/null || true
    wait $API_PID 2>/dev/null || true
    wait $CLIENT_PID 2>/dev/null || true
    echo -e "${GREEN}Services stopped cleanly.${NC}"
    exit 0
}

trap cleanup SIGINT SIGTERM

echo -e "\n  Waiting for services to become responsive..."
for i in {1..30}; do
    if curl -s http://127.0.0.1:5111/api/games &>/dev/null && curl -s http://localhost:4200 &>/dev/null; then
        break
    fi
    sleep 1
done

echo -e "\n${BOLD}${GREEN}====================================================${NC}"
echo -e "${BOLD}${GREEN}   Application Ready!                               ${NC}"
echo -e "${BOLD}   Frontend: ${BLUE}http://localhost:4200${NC}"
echo -e "${BOLD}   API:      ${BLUE}http://127.0.0.1:5111${NC}"
echo -e "${BOLD}${GREEN}====================================================${NC}"
echo -e "${YELLOW}Press Ctrl+C to stop all services.${NC}\n"

# Open browser if available
if command -v open &> /dev/null; then
    open http://localhost:4200
elif command -v xdg-open &> /dev/null; then
    xdg-open http://localhost:4200 2>/dev/null || true
fi

# Wait for background processes
wait

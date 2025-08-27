#!/bin/bash

# StudyBridge Development Setup Script

echo "🚀 Setting up StudyBridge development environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

# Check .NET 8
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET 8 SDK not found. Please install .NET 8 SDK.${NC}"
    exit 1
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    echo -e "${RED}❌ Node.js not found. Please install Node.js LTS.${NC}"
    exit 1
fi

# Check Angular CLI
if ! command -v ng &> /dev/null; then
    echo -e "${YELLOW}Installing Angular CLI...${NC}"
    npm install -g @angular/cli
fi

echo -e "${GREEN}✅ Prerequisites check complete.${NC}"

# Build .NET solution
echo -e "${YELLOW}Building .NET solution...${NC}"
cd StudyBridge
dotnet build
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ .NET solution built successfully.${NC}"
else
    echo -e "${RED}❌ .NET build failed.${NC}"
    exit 1
fi

# Install Angular dependencies
echo -e "${YELLOW}Installing Angular dependencies...${NC}"
cd ../Client/Web/studybridge-web
npm install
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Angular dependencies installed.${NC}"
else
    echo -e "${RED}❌ Angular dependency installation failed.${NC}"
    exit 1
fi

# Build Angular app
echo -e "${YELLOW}Building Angular application...${NC}"
npm run build
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Angular application built successfully.${NC}"
else
    echo -e "${RED}❌ Angular build failed.${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}🎉 StudyBridge setup complete!${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Set up PostgreSQL database"
echo "2. Update connection string in StudyBridge/src/StudyBridge.Api/appsettings.json"
echo "3. Configure Google OAuth client ID"
echo "4. Run the API: cd StudyBridge/src/StudyBridge.Api && dotnet run"
echo "5. Run Angular: cd Client/Web/studybridge-web && ng serve"
echo ""
echo -e "${GREEN}Happy coding! 🚀${NC}"

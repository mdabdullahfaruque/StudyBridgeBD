#!/bin/bash

# StudyBridge Project Structure Validator
# This script validates the current project structure against the documented structure

echo "🏗️  StudyBridge Project Structure Validator"
echo "============================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Base directory
BASE_DIR="src/app"

# Function to check if directory exists
check_dir() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1 ${RED}(MISSING)${NC}"
        return 1
    fi
}

# Function to check if file exists
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1 ${RED}(MISSING)${NC}"
        return 1
    fi
}

# Function to list unexpected files in directory
check_unexpected() {
    local dir="$1"
    shift
    local expected=("$@")
    
    if [ -d "$dir" ]; then
        for item in "$dir"/*; do
            if [ -e "$item" ]; then
                basename_item=$(basename "$item")
                if [[ ! " ${expected[@]} " =~ " ${basename_item} " ]]; then
                    echo -e "${YELLOW}⚠${NC}  Unexpected: $item"
                fi
            fi
        done
    fi
}

echo ""
echo -e "${BLUE}📁 Checking Core Module Structure...${NC}"
check_dir "$BASE_DIR/core"
check_dir "$BASE_DIR/core/guards"
check_dir "$BASE_DIR/core/interceptors"
check_dir "$BASE_DIR/core/services"
check_dir "$BASE_DIR/core/models"

echo ""
echo -e "${BLUE}📁 Checking Shared Module Structure...${NC}"
check_dir "$BASE_DIR/shared"
check_dir "$BASE_DIR/shared/components"
check_dir "$BASE_DIR/shared/layouts"
check_dir "$BASE_DIR/shared/models"
check_dir "$BASE_DIR/shared/services"
check_dir "$BASE_DIR/shared/pipes"
check_dir "$BASE_DIR/shared/directives"
check_dir "$BASE_DIR/shared/validators"
check_dir "$BASE_DIR/shared/constants"
check_file "$BASE_DIR/shared/shared.module.ts"

echo ""
echo -e "${BLUE}📁 Checking Auth Module Structure...${NC}"
check_dir "$BASE_DIR/features/auth"
check_dir "$BASE_DIR/features/auth/components"
check_dir "$BASE_DIR/features/auth/models"
check_dir "$BASE_DIR/features/auth/services"
check_dir "$BASE_DIR/features/auth/guards"
check_file "$BASE_DIR/features/auth/auth.module.ts"
check_file "$BASE_DIR/features/auth/auth-routing.module.ts"

echo ""
echo -e "${BLUE}📁 Checking Admin Module Structure...${NC}"
check_dir "$BASE_DIR/features/admin"
check_dir "$BASE_DIR/features/admin/components"
check_dir "$BASE_DIR/features/admin/models"
check_dir "$BASE_DIR/features/admin/services"
check_dir "$BASE_DIR/features/admin/guards"
check_dir "$BASE_DIR/features/admin/pipes"
check_file "$BASE_DIR/features/admin/admin.module.ts"
check_file "$BASE_DIR/features/admin/admin-routing.module.ts"

echo ""
echo -e "${BLUE}📁 Checking Public Module Structure...${NC}"
check_dir "$BASE_DIR/features/public"
check_dir "$BASE_DIR/features/public/components"
check_dir "$BASE_DIR/features/public/models"
check_dir "$BASE_DIR/features/public/services"
check_dir "$BASE_DIR/features/public/pipes"
check_dir "$BASE_DIR/features/public/directives"
check_file "$BASE_DIR/features/public/public.module.ts"
check_file "$BASE_DIR/features/public/public-routing.module.ts"

echo ""
echo -e "${BLUE}📁 Checking Component Structure Convention...${NC}"

# Function to validate component structure
validate_component() {
    local component_path="$1"
    local component_name=$(basename "$component_path")
    
    if [ -d "$component_path" ]; then
        echo -e "${BLUE}  Checking $component_name...${NC}"
        
        # Check for required files
        local ts_file="$component_path/$component_name.component.ts"
        local html_file="$component_path/$component_name.component.html"
        local scss_file="$component_path/$component_name.component.scss"
        
        check_file "$ts_file"
        check_file "$html_file"
        check_file "$scss_file"
        
        # Check for unexpected files
        check_unexpected "$component_path" "$component_name.component.ts" "$component_name.component.html" "$component_name.component.scss" "$component_name.component.spec.ts"
    fi
}

# Check some key components
if [ -d "$BASE_DIR/features/auth/components/login" ]; then
    validate_component "$BASE_DIR/features/auth/components/login"
fi

if [ -d "$BASE_DIR/features/auth/components/register" ]; then
    validate_component "$BASE_DIR/features/auth/components/register"
fi

echo ""
echo -e "${BLUE}🔍 Checking for Legacy Structure...${NC}"

# Check for old structure that should be moved/removed
legacy_paths=(
    "$BASE_DIR/guards"
    "$BASE_DIR/interceptors"
    "$BASE_DIR/services"
    "$BASE_DIR/models"
    "$BASE_DIR/layout"
)

for path in "${legacy_paths[@]}"; do
    if [ -d "$path" ] || [ -f "$path" ]; then
        echo -e "${YELLOW}⚠${NC}  Legacy structure found: $path ${YELLOW}(should be moved/removed)${NC}"
    fi
done

echo ""
echo -e "${BLUE}📊 Structure Validation Complete${NC}"
echo ""
echo -e "${YELLOW}💡 Tips:${NC}"
echo "- Update PROJECT_STRUCTURE.md when making structural changes"
echo "- Follow component naming convention: component-name.component.{ts,html,scss}"
echo "- Keep feature modules self-contained"
echo "- Use shared module for reusable components"
echo "- Sync services with backend API structure"

echo ""
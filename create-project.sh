#!/bin/bash

# Weda Clean Architecture Template - Interactive Project Creator
# Step-by-step guided project creation

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m'
BOLD='\033[1m'

# Default values
PROJECT_NAME=""
DATABASE="sqlite"
NATS_SERVICE=""
INCLUDE_TEST="true"
INCLUDE_WIKI="true"
INCLUDE_SAMPLE="true"

clear
printf "${CYAN}"
printf "╔═══════════════════════════════════════════════════════════════╗\n"
printf "║                                                               ║\n"
printf "║   ${WHITE}Weda Clean Architecture Template${CYAN}                            ║\n"
printf "║   ${WHITE}Interactive Project Creator${CYAN}                                 ║\n"
printf "║                                                               ║\n"
printf "╚═══════════════════════════════════════════════════════════════╝\n"
printf "${NC}\n"

# Step 1: Project Name
printf "${BLUE}Step 1/6: Project Name${NC}\n"
printf "Enter the name for your project (e.g., MyCompany.MyProject)\n"
printf "This will be used as the namespace and file prefix.\n\n"
while [ -z "$PROJECT_NAME" ]; do
    printf "${WHITE}Project name: ${NC}"
    read PROJECT_NAME
    if [ -z "$PROJECT_NAME" ]; then
        printf "${RED}Project name is required!${NC}\n"
    fi
done
NATS_SERVICE=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr '.' '-')
printf "${GREEN}✓ Project name: ${PROJECT_NAME}${NC}\n\n"

# Step 2: Database
printf "${BLUE}Step 2/6: Database Provider${NC}\n"
printf "Choose your database:\n"
printf "  ${WHITE}1${NC}) sqlite   - SQLite (lightweight, file-based) ${YELLOW}[default]${NC}\n"
printf "  ${WHITE}2${NC}) postgres - PostgreSQL (production-ready)\n"
printf "  ${WHITE}3${NC}) mongo    - MongoDB (document database)\n"
printf "  ${WHITE}4${NC}) none     - InMemory (for testing)\n\n"
printf "${WHITE}Select [1-4]: ${NC}"
read db_choice
case "$db_choice" in
    2|postgres) DATABASE="postgres" ;;
    3|mongo) DATABASE="mongo" ;;
    4|none) DATABASE="none" ;;
    *) DATABASE="sqlite" ;;
esac
printf "${GREEN}✓ Database: ${DATABASE}${NC}\n\n"

# Step 3: NATS Service Name
printf "${BLUE}Step 3/6: NATS Service Name${NC}\n"
printf "Used for JetStream streams, KV buckets, and consumer groups.\n\n"
printf "${WHITE}NATS service name [${NATS_SERVICE}]: ${NC}"
read nats_input
if [ -n "$nats_input" ]; then
    NATS_SERVICE="$nats_input"
fi
printf "${GREEN}✓ NATS service: ${NATS_SERVICE}${NC}\n\n"

# Step 4: Include Tests
printf "${BLUE}Step 4/6: Test Projects${NC}\n"
printf "Include unit and integration test projects?\n\n"
printf "${WHITE}Include tests? [Y/n]: ${NC}"
read test_input
test_input=$(echo "$test_input" | tr '[:upper:]' '[:lower:]')
if [ "$test_input" = "n" ] || [ "$test_input" = "no" ]; then
    INCLUDE_TEST="false"
fi
printf "${GREEN}✓ Include tests: ${INCLUDE_TEST}${NC}\n\n"

# Step 5: Include Wiki
printf "${BLUE}Step 5/6: Wiki Documentation${NC}\n"
printf "Include wiki documentation and generator tool?\n\n"
printf "${WHITE}Include wiki? [Y/n]: ${NC}"
read wiki_input
wiki_input=$(echo "$wiki_input" | tr '[:upper:]' '[:lower:]')
if [ "$wiki_input" = "n" ] || [ "$wiki_input" = "no" ]; then
    INCLUDE_WIKI="false"
fi
printf "${GREEN}✓ Include wiki: ${INCLUDE_WIKI}${NC}\n\n"

# Step 6: Include Sample
printf "${BLUE}Step 6/6: Sample Module${NC}\n"
printf "Include the Employee sample module as reference?\n\n"
printf "${WHITE}Include sample? [Y/n]: ${NC}"
read sample_input
sample_input=$(echo "$sample_input" | tr '[:upper:]' '[:lower:]')
if [ "$sample_input" = "n" ] || [ "$sample_input" = "no" ]; then
    INCLUDE_SAMPLE="false"
fi
printf "${GREEN}✓ Include sample: ${INCLUDE_SAMPLE}${NC}\n\n"

# Summary
printf "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
printf "${WHITE}Summary:${NC}\n"
printf "  Project Name:  ${CYAN}${PROJECT_NAME}${NC}\n"
printf "  Database:      ${CYAN}${DATABASE}${NC}\n"
printf "  NATS Service:  ${CYAN}${NATS_SERVICE}${NC}\n"
printf "  Include Tests: ${CYAN}${INCLUDE_TEST}${NC}\n"
printf "  Include Wiki:  ${CYAN}${INCLUDE_WIKI}${NC}\n"
printf "  Include Sample:${CYAN}${INCLUDE_SAMPLE}${NC}\n"
printf "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n\n"

printf "${WHITE}Create project? [Y/n]: ${NC}"
read confirm
confirm=$(echo "$confirm" | tr '[:upper:]' '[:lower:]')
if [ "$confirm" = "n" ] || [ "$confirm" = "no" ]; then
    printf "${YELLOW}Cancelled.${NC}\n"
    exit 0
fi

# Execute
printf "\n${YELLOW}Creating project...${NC}\n\n"

CMD="dotnet new weda -n \"$PROJECT_NAME\" -db $DATABASE --Nats \"$NATS_SERVICE\" --test $INCLUDE_TEST --wiki $INCLUDE_WIKI --sample $INCLUDE_SAMPLE"
printf "${CYAN}$ ${CMD}${NC}\n\n"
eval $CMD

printf "\n${GREEN}╔═══════════════════════════════════════════════════════════════╗${NC}\n"
printf "${GREEN}║              Project created successfully!                    ║${NC}\n"
printf "${GREEN}╚═══════════════════════════════════════════════════════════════╝${NC}\n\n"

printf "${WHITE}Next steps:${NC}\n"
printf "  1. cd ${PROJECT_NAME}\n"
printf "  2. dotnet run --project src/${PROJECT_NAME}.Api\n"
printf "  3. Open ${CYAN}http://localhost:5001/swagger${NC}\n\n"
printf "${WHITE}Or use Docker:${NC}\n"
printf "  docker compose up --build\n\n"
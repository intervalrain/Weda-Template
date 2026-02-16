# Weda Clean Architecture Template - Interactive Project Creator
# Step-by-step guided project creation (PowerShell)

$ErrorActionPreference = "Stop"

# Default values
$ProjectName = ""
$Database = "sqlite"
$NatsService = ""
$IncludeTest = $true
$IncludeWiki = $true
$IncludeSample = $true

Clear-Host
Write-Host ""
Write-Host "  ╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║                                                               ║" -ForegroundColor Cyan
Write-Host "  ║   " -ForegroundColor Cyan -NoNewline
Write-Host "Weda Clean Architecture Template" -ForegroundColor White -NoNewline
Write-Host "                          ║" -ForegroundColor Cyan
Write-Host "  ║   " -ForegroundColor Cyan -NoNewline
Write-Host "Interactive Project Creator" -ForegroundColor White -NoNewline
Write-Host "                               ║" -ForegroundColor Cyan
Write-Host "  ║                                                               ║" -ForegroundColor Cyan
Write-Host "  ╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Project Name
Write-Host "Step 1/6: Project Name" -ForegroundColor Blue
Write-Host "Enter the name for your project (e.g., MyCompany.MyProject)"
Write-Host "This will be used as the namespace and file prefix."
Write-Host ""
while ([string]::IsNullOrEmpty($ProjectName)) {
    $ProjectName = Read-Host "Project name"
    if ([string]::IsNullOrEmpty($ProjectName)) {
        Write-Host "Project name is required!" -ForegroundColor Red
    }
}
$NatsService = $ProjectName.ToLower().Replace(".", "-")
Write-Host "✓ Project name: $ProjectName" -ForegroundColor Green
Write-Host ""

# Step 2: Database
Write-Host "Step 2/6: Database Provider" -ForegroundColor Blue
Write-Host "Choose your database:"
Write-Host "  1) sqlite   - SQLite (lightweight, file-based) " -NoNewline
Write-Host "[default]" -ForegroundColor Yellow
Write-Host "  2) postgres - PostgreSQL (production-ready)"
Write-Host "  3) mongo    - MongoDB (document database)"
Write-Host "  4) none     - InMemory (for testing)"
Write-Host ""
$dbChoice = Read-Host "Select [1-4]"
switch ($dbChoice) {
    "2" { $Database = "postgres" }
    "postgres" { $Database = "postgres" }
    "3" { $Database = "mongo" }
    "mongo" { $Database = "mongo" }
    "4" { $Database = "none" }
    "none" { $Database = "none" }
    default { $Database = "sqlite" }
}
Write-Host "✓ Database: $Database" -ForegroundColor Green
Write-Host ""

# Step 3: NATS Service Name
Write-Host "Step 3/6: NATS Service Name" -ForegroundColor Blue
Write-Host "Used for JetStream streams, KV buckets, and consumer groups."
Write-Host ""
$natsInput = Read-Host "NATS service name [$NatsService]"
if (-not [string]::IsNullOrEmpty($natsInput)) {
    $NatsService = $natsInput
}
Write-Host "✓ NATS service: $NatsService" -ForegroundColor Green
Write-Host ""

# Step 4: Include Tests
Write-Host "Step 4/6: Test Projects" -ForegroundColor Blue
Write-Host "Include unit and integration test projects?"
Write-Host ""
$testInput = Read-Host "Include tests? [Y/n]"
if ($testInput -match "^[Nn]") {
    $IncludeTest = $false
}
Write-Host "✓ Include tests: $IncludeTest" -ForegroundColor Green
Write-Host ""

# Step 5: Include Wiki
Write-Host "Step 5/6: Wiki Documentation" -ForegroundColor Blue
Write-Host "Include wiki documentation and generator tool?"
Write-Host ""
$wikiInput = Read-Host "Include wiki? [Y/n]"
if ($wikiInput -match "^[Nn]") {
    $IncludeWiki = $false
}
Write-Host "✓ Include wiki: $IncludeWiki" -ForegroundColor Green
Write-Host ""

# Step 6: Include Sample
Write-Host "Step 6/6: Sample Module" -ForegroundColor Blue
Write-Host "Include the Employee sample module as reference?"
Write-Host ""
$sampleInput = Read-Host "Include sample? [Y/n]"
if ($sampleInput -match "^[Nn]") {
    $IncludeSample = $false
}
Write-Host "✓ Include sample: $IncludeSample" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor White
Write-Host "  Project Name:  " -NoNewline
Write-Host $ProjectName -ForegroundColor Cyan
Write-Host "  Database:      " -NoNewline
Write-Host $Database -ForegroundColor Cyan
Write-Host "  NATS Service:  " -NoNewline
Write-Host $NatsService -ForegroundColor Cyan
Write-Host "  Include Tests: " -NoNewline
Write-Host $IncludeTest -ForegroundColor Cyan
Write-Host "  Include Wiki:  " -NoNewline
Write-Host $IncludeWiki -ForegroundColor Cyan
Write-Host "  Include Sample:" -NoNewline
Write-Host $IncludeSample -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

$confirm = Read-Host "Create project? [Y/n]"
if ($confirm -match "^[Nn]") {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

# Execute
Write-Host ""
Write-Host "Creating project..." -ForegroundColor Yellow
Write-Host ""

$testStr = if ($IncludeTest) { "true" } else { "false" }
$wikiStr = if ($IncludeWiki) { "true" } else { "false" }
$sampleStr = if ($IncludeSample) { "true" } else { "false" }

$cmd = "dotnet new weda -n `"$ProjectName`" -db $Database --Nats `"$NatsService`" --test $testStr --wiki $wikiStr --sample $sampleStr"
Write-Host "$ $cmd" -ForegroundColor Cyan
Write-Host ""

Invoke-Expression $cmd

Write-Host ""
Write-Host "  ╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║              Project created successfully!                    ║" -ForegroundColor Green
Write-Host "  ╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. cd $ProjectName"
Write-Host "  2. dotnet run --project src/$ProjectName.Api"
Write-Host "  3. Open " -NoNewline
Write-Host "http://localhost:8080/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Or use Docker:" -ForegroundColor White
Write-Host "  docker compose up --build"
Write-Host ""
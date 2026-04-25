$ErrorActionPreference = "Stop"

$PLUGIN_NAME = "Jellyfin2HQPlayer"
$TARBALL = "Jellyfin2HQPlayerPlugin-1.0.0.tar.gz"
$INSTALL_DIR = "C:\ProgramData\Jellyfin\Server\plugins\$PLUGIN_NAME"
$TMP_DIR = Join-Path $env:TEMP "${PLUGIN_NAME}_deploy"

Write-Host "=========================================="
Write-Host " Jellyfin Plugin Deploy Script (Windows)"
Write-Host "=========================================="

# 1. Check package
if (-not (Test-Path $TARBALL)) {
    Write-Host "[ERROR] Package not found: $TARBALL"
    exit 1
}

Write-Host "[OK] Found package: $TARBALL"

# 2. Clean temp directory
if (Test-Path $TMP_DIR) {
    Remove-Item $TMP_DIR -Recurse -Force
}
New-Item -ItemType Directory -Path $TMP_DIR | Out-Null

# 3. Extract
Write-Host "[STEP] Extracting..."
tar -xzf $TARBALL -C $TMP_DIR

# 4. Locate plugin directory
$PLUGIN_SRC = Get-ChildItem -Path $TMP_DIR -Recurse -Directory |
    Where-Object { $_.Name -eq "Jellyfin2HQPlayerPlugin" } |
    Select-Object -First 1

if (-not $PLUGIN_SRC) {
    Write-Host "[ERROR] Cannot find Jellyfin2HQPlayerPlugin directory inside tar.gz"
    exit 1
}

# 5. Create plugin directory
Write-Host "[STEP] Creating plugin directory..."
New-Item -ItemType Directory -Force -Path $INSTALL_DIR | Out-Null

# 6. Check if real Jellyfin server process is running
$runningProcesses = Get-Process -ErrorAction SilentlyContinue | Where-Object {
    $_.ProcessName -in @("jellyfin", "Jellyfin Server")
}

if ($runningProcesses) {
    Write-Host ""
    Write-Host "##############################################"
    Write-Host "##############################################"
    Write-Host "###                                        ###"
    Write-Host "###   [WARN] JELLYFIN IS CURRENTLY RUNNING ###"
    Write-Host "###                                        ###"
    Write-Host "##############################################"
    Write-Host "##############################################"
    Write-Host ""
    Write-Host "[INFO] Please STOP Jellyfin first"
    Write-Host "[INFO] Then run this script again"
    Write-Host ""
    exit 1
}

# 7. Clean old files
Write-Host "[STEP] Cleaning old files..."
Get-ChildItem -Path $INSTALL_DIR -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

# 8. Copy files
Write-Host "[STEP] Copying plugin files..."
Copy-Item (Join-Path $PLUGIN_SRC.FullName "*") $INSTALL_DIR -Recurse -Force

# 9. Manual restart reminder
Write-Host ""
Write-Host "##############################################"
Write-Host "##############################################"
Write-Host "###                                        ###"
Write-Host "###   [WARN] RESTART JELLYFIN REQUIRED !!! ###"
Write-Host "###                                        ###"
Write-Host "##############################################"
Write-Host "##############################################"
Write-Host ""

$service = Get-Service -Name "Jellyfin" -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "[INFO] Jellyfin service exists"
    Write-Host "[INFO] Please restart Jellyfin service manually"
} else {
    Write-Host "[INFO] Jellyfin is NOT installed as a Windows service"
    Write-Host "[INFO] Please START Jellyfin again manually"
}

Write-Host ""
Write-Host "[WARN] Plugin changes will NOT take effect until restart!"
Write-Host ""
Write-Host "[INFO] Plugin path:"
Write-Host $INSTALL_DIR
Write-Host ""

Write-Host "=========================================="
Write-Host "[OK] Deploy completed"
Write-Host "=========================================="
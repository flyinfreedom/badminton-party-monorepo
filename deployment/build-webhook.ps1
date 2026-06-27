param (
    [Parameter(Position=0)]
    [string]$Tag = ""
)

$ErrorActionPreference = "Stop"

# 1. Get Tag
if ([string]::IsNullOrWhiteSpace($Tag)) {
    $Tag = Read-Host "Please enter image tag (default: latest)"
    if ([string]::IsNullOrWhiteSpace($Tag)) {
        $Tag = "latest"
    }
}

$Registry = "asia-east1-docker.pkg.dev/badminton-party-488004/badminton-party-webhook"
$ImageName = "badminton-party-webhook"
$FullImageTag = "${Registry}/${ImageName}:${Tag}"

# 2. Define paths
$DockerfilePath = Join-Path $PSScriptRoot "Dockerfile.webhook"
$ContextPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

Write-Host "=== Preparing to build Webhook image ===" -ForegroundColor Cyan
Write-Host "Dockerfile: $DockerfilePath" -ForegroundColor Gray
Write-Host "Build Context: $ContextPath" -ForegroundColor Gray
Write-Host "Target Image Tag: $FullImageTag" -ForegroundColor Yellow

# 3. Check Docker
if (-not (Get-Command "docker" -ErrorAction SilentlyContinue)) {
    Write-Error "docker command not found. Please make sure Docker is installed and added to PATH."
}

# 4. Build
Write-Host "=== Building Docker image ===" -ForegroundColor Cyan
docker build -t $FullImageTag -f $DockerfilePath $ContextPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed!"
}

# 5. Auth GCP
Write-Host "=== Configuring GCP Docker credentials ===" -ForegroundColor Cyan
if (-not (Get-Command "gcloud" -ErrorAction SilentlyContinue)) {
    Write-Warning "gcloud command not found. Trying to push directly..."
} else {
    gcloud auth configure-docker asia-east1-docker.pkg.dev
}

# 6. Push
Write-Host "=== Pushing image to GCP Artifact Registry ===" -ForegroundColor Cyan
docker push $FullImageTag
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker push failed! Make sure you are logged in and have write permissions."
}

Write-Host "=== Webhook image build & push complete ===" -ForegroundColor Green
Write-Host "Image Tag: $FullImageTag" -ForegroundColor Green

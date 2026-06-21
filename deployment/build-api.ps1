param (
    [Parameter(Position=0)]
    [string]$Tag = ""
)

# 確保在發生錯誤時停止執行
$ErrorActionPreference = "Stop"

# 1. 取得版號 Tag
if ([string]::IsNullOrWhiteSpace($Tag)) {
    $Tag = Read-Host "請輸入映像檔版號 Tag (預設為 latest)"
    if ([string]::IsNullOrWhiteSpace($Tag)) {
        $Tag = "latest"
    }
}

$Registry = "asia-east1-docker.pkg.dev/badminton-party-488004/badminton-party-api"
$ImageName = "badminton-party-api"
$FullImageTag = "${Registry}/${ImageName}:${Tag}"

# 2. 定義路徑 (基於腳本所在目錄，確保在任何目錄下執行皆正常)
$DockerfilePath = Join-Path $PSScriptRoot "Dockerfile"
$ContextPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

Write-Host "=== 準備建置 API 映像檔 ===" -ForegroundColor Cyan
Write-Host "Dockerfile: $DockerfilePath" -ForegroundColor Gray
Write-Host "Build Context: $ContextPath" -ForegroundColor Gray
Write-Host "目標 Image Tag: $FullImageTag" -ForegroundColor Yellow

# 3. 檢查 Docker 服務是否正常
if (-not (Get-Command "docker" -ErrorAction SilentlyContinue)) {
    Write-Error "找不到 docker 指令，請確認已安裝 Docker 並且已加入 PATH。"
}

# 4. 開始建置
Write-Host "=== 開始建置 Docker 映像檔 ===" -ForegroundColor Cyan
docker build -t $FullImageTag -f $DockerfilePath $ContextPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker 建置失敗！"
}

# 5. 設定 GCP Docker 認證
Write-Host "=== 設定 GCP Docker 憑證認證 ===" -ForegroundColor Cyan
if (-not (Get-Command "gcloud" -ErrorAction SilentlyContinue)) {
    Write-Warning "找不到 gcloud 指令，無法自動設定憑證。將直接嘗試推送到 Artifact Registry..."
} else {
    gcloud auth configure-docker asia-east1-docker.pkg.dev
}

# 6. 推送映像檔
Write-Host "=== 開始推送映像檔至 GCP Artifact Registry ===" -ForegroundColor Cyan
docker push $FullImageTag
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker 推送失敗！請確認您已登入 GCP 並且擁有該 Artifact Registry 的寫入權限。"
}

Write-Host "=== 部署 API 映像檔完成 ===" -ForegroundColor Green
Write-Host "映像檔位址: $FullImageTag" -ForegroundColor Green

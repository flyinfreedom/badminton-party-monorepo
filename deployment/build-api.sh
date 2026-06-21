#!/bin/bash

# 確保在發生錯誤時停止執行
set -e

# 1. 取得版號 Tag
TAG=$1
if [ -z "$TAG" ]; then
    read -p "請輸入映像檔版號 Tag [預設: latest]: " TAG
    if [ -z "$TAG" ]; then
        TAG="latest"
    fi
fi

REGISTRY="asia-east1-docker.pkg.dev/badminton-party-488004/badminton-party-api"
IMAGE_NAME="badminton-party-api"
FULL_IMAGE_TAG="${REGISTRY}/${IMAGE_NAME}:${TAG}"

# 2. 定義路徑 (基於腳本所在目錄，確保在任何目錄下執行皆正常)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$( cd "$SCRIPT_DIR/.." && pwd )"
DOCKERFILE_PATH="$SCRIPT_DIR/Dockerfile"

echo -e "\033[36m=== 準備建置 API 映像檔 ===\033[0m"
echo -e "Dockerfile: \033[37m${DOCKERFILE_PATH}\033[0m"
echo -e "Build Context: \033[37m${ROOT_DIR}\033[0m"
echo -e "目標 Image Tag: \033[33m${FULL_IMAGE_TAG}\033[0m"

# 3. 檢查 Docker 指令是否存在
if ! command -v docker &> /dev/null; then
    echo -e "\033[31m錯誤: 找不到 docker 指令，請確認已安裝 Docker 並且已加入 PATH。\033[0m"
    exit 1
fi

# 4. 開始建置
echo -e "\033[36m=== 開始建置 Docker 映像檔 ===\033[0m"
docker build -t "$FULL_IMAGE_TAG" -f "$DOCKERFILE_PATH" "$ROOT_DIR"

# 5. 設定 GCP Docker 認證
echo -e "\033[36m=== 設定 GCP Docker 憑證認證 ===\033[0m"
if ! command -v gcloud &> /dev/null; then
    echo -e "\033[33m警告: 找不到 gcloud 指令，無法自動設定憑證。將直接嘗試推送到 Artifact Registry...\033[0m"
else
    gcloud auth configure-docker asia-east1-docker.pkg.dev
fi

# 6. 推送映像檔
echo -e "\033[36m=== 開始推送映像檔至 GCP Artifact Registry ===\033[0m"
docker push "$FULL_IMAGE_TAG"

echo -e "\033[32m=== 部署 API 映像檔完成 ===\033[0m"
echo -e "映像檔位址: \033[32m${FULL_IMAGE_TAG}\033[0m"

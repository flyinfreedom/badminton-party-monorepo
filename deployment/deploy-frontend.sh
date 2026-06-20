#!/bin/bash

# GCP 設定變數 - 請修改為您實際的設定
PROJECT_ID="your-gcp-project-id"
BUCKET_NAME="your-gcs-bucket-name"

# 取得腳本所在的目錄
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# 1. 建置前端專案
echo "=== 開始建置前端 Angular 專案 ==="
cd "$SCRIPT_DIR/../src/WebApps/badminton-party"
npm install
npm run build -- --configuration=production

# 2. 上傳至 GCS
echo "=== 上傳建置產物至 GCP GCS ==="
# 設定 GCP 專案
gcloud config set project $PROJECT_ID

# 同步檔案至 GCS Bucket
gcloud storage rsync dist/badminton-party-liff-web/browser gs://$BUCKET_NAME --recursive

# 設定 GCS 儲存桶為靜態網站
gsutil web set -m index.html -e index.html gs://$BUCKET_NAME

# 設定所有上傳的物件為公開可讀 (若要直接提供外界存取)
gsutil iam ch allUsers:objectViewer gs://$BUCKET_NAME

echo "=== 前端部署完成 ==="

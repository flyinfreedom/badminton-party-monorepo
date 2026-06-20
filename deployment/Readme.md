# 🚀 GCP 部署指南 (Google Cloud Platform Deployment Guide)

本文件說明如何將 **BadmintonParty** 專案部署至 GCP：
- **前端 (Angular)**：部署至 **Google Cloud Storage (GCS)** 進行靜態網站代管。
- **後端 API (.NET 10)**：部署至 **Cloud Run**。

---

## 📋 前置作業與準備工作

在開始部署之前，請確保您已完成以下準備：

1. **安裝 Google Cloud CLI (gcloud)**
   - 請參考 [GCP 官方文件](https://cloud.google.com/sdk/docs/install) 安裝並初始化您的 `gcloud` 工具。
   - 執行登入並設定專案：
     ```bash
     gcloud auth login
     gcloud config set project <YOUR_GCP_PROJECT_ID>
     ```

2. **啟用 GCP 必要服務 API**
   - 執行以下指令啟用 Artifact Registry、Cloud Run 與 Cloud Build 等服務：
     ```bash
     gcloud services enable \
       run.googleapis.com \
       artifactregistry.googleapis.com \
       storage.googleapis.com \
       cloudbuild.googleapis.com
     ```

3. **建立 GCP 資源**
   - **建立 Artifact Registry 存放後端 Image**：
     ```bash
     gcloud artifacts repositories create badminton-party-repo \
       --repository-format=docker \
       --location=asia-east1 \
       --description="BadmintonParty Docker Images"
     ```
   - **建立 GCS 儲存桶 (Bucket) 存放前端靜態檔案**：
     ```bash
     # Bucket 名稱通常需要是全域唯一的 (例如：my-badminton-party-bucket)
     gcloud storage buckets create gs://<YOUR_BUCKET_NAME> \
       --location=asia-east1 \
       --uniform-bucket-level-access
     ```

---

## 🖥️ 後端 API 部署 (Cloud Run)

後端服務基於 .NET 10.0，我們提供了一個位於 [deployment/Dockerfile](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/deployment/Dockerfile) 的多階段建置 Dockerfile。

> [!IMPORTANT]
> 進行 Docker Image 建置時，**請務必在專案的「根目錄」下執行指令**，以便建置上下文 (Context) 能包含同方案下的其他相依專案 (例如：`BadmintonParty.ServiceDefaults`)。

### 步驟 1：建置並推送 Docker 映像檔

您可以選擇在本地建置後推送，或是直接使用 GCP Cloud Build 在雲端建置：

#### 方法 A：使用 Cloud Build 在雲端建置（推薦，免去本地 Docker 安裝）
在專案根目錄下執行：
```bash
gcloud builds submit --tag asia-east1-docker.pkg.dev/<YOUR_GCP_PROJECT_ID>/badminton-party-repo/api:latest -f deployment/Dockerfile .
```

#### 方法 B：在本地使用 Docker 建置並推送
1. 本地建置：
   ```bash
   docker build -t asia-east1-docker.pkg.dev/<YOUR_GCP_PROJECT_ID>/badminton-party-repo/api:latest -f deployment/Dockerfile .
   ```
2. 設定 Docker 憑證並推送：
   ```bash
   gcloud auth configure-docker asia-east1-docker.pkg.dev
   docker push asia-east1-docker.pkg.dev/<YOUR_GCP_PROJECT_ID>/badminton-party-repo/api:latest
   ```

### 步驟 2：將映像檔部署至 Cloud Run

執行以下指令，將映像檔部署至 Cloud Run：
```bash
gcloud run deploy badminton-party-api \
  --image=asia-east1-docker.pkg.dev/<YOUR_GCP_PROJECT_ID>/badminton-party-repo/api:latest \
  --platform=managed \
  --region=asia-east1 \
  --allow-unauthenticated \
  --port=8080
```
> [!TIP]
> 部署完成後，命令列會輸出 Cloud Run 服務的 URL（例如：`https://badminton-party-api-xxxxx-de.a.run.app`）。**請記錄此 URL**，前端專案在建置時需要將 API 終端節點指向此 URL。

### 步驟 3：設定後端環境變數 (資料庫連線等)

若您的後端 API 需要連接資料庫，請於 Cloud Run 中設定環境變數：
- `ConnectionStrings__DefaultConnection` (例如：`Host=/cloudsql/<YOUR_CONNECTION_NAME>;Database=postgres;Username=postgres;Password=your_password`)
- 若使用的是 GCP Cloud SQL (PostgreSQL)，請在部署時加上 `--add-cloudsql-instances=<YOUR_CLOUD_SQL_CONNECTION_NAME>` 以建立 Cloud SQL 連線通道。

---

## 🌐 前端網頁部署 (GCS)

前端是一個 Angular 應用程式。我們需要先在前端程式碼中配置正確的後端 API URL，再進行建置並發布到 GCS。

### 步驟 1：配置生產環境後端 API 位址

在前端專案中，修改生產環境配置檔，將 API 基礎路徑指向剛才部署的 Cloud Run 服務 URL。例如：
- 檢查 [src/WebApps/badminton-party/projects/badminton-party-liff-web/src/environments/environment.prod.ts](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/src/WebApps/badminton-party/projects/badminton-party-liff-web/src/environments/environment.prod.ts) 或對應的環境設定檔。
- 將 `apiUrl` 設定為 Cloud Run URL：
  ```typescript
  export const environment = {
    production: true,
    apiUrl: 'https://badminton-party-api-xxxxx-de.a.run.app' // 替換為您的 Cloud Run 網址
  };
  ```

### 步驟 2：執行自動化部署腳本

我們在 `deployment` 資料夾下提供了自動化建置與部署腳本，會自動完成 `npm install`、`npm run build`，並利用 `rsync` 指令將網頁上傳到 GCS 儲存桶，同時配置儲存桶的靜態託管權限。

請依據您的作業系統，修改腳本頂部的 `$PROJECT_ID` / `PROJECT_ID` 及 `$BUCKET_NAME` / `BUCKET_NAME` 變數後執行：

#### 🔹 在 Windows PowerShell 下執行：
請開啟 PowerShell 並執行 [deploy-frontend.ps1](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/deployment/deploy-frontend.ps1)：
```powershell
./deployment/deploy-frontend.ps1
```

#### 🔹 在 Linux / macOS / WSL 環境下執行：
請開啟 Terminal 並執行 [deploy-frontend.sh](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/deployment/deploy-frontend.sh)：
```bash
chmod +x ./deployment/deploy-frontend.sh
./deployment/deploy-frontend.sh
```

---

## 🔒 跨網域設定 (CORS) 與自訂網域 (HTTPS)

### 1. CORS 設定
由於前端託管在 GCS，後端託管在 Cloud Run，兩者網域名稱不同，請確保後端 API 的 `Program.cs` 中，有配置 CORS 政策，允許來自您 GCS 託管網域（如 `https://storage.googleapis.com` 或您的自訂網域）的跨來源請求。

### 2. GCS 靜態網站的 HTTPS 與自訂網域
- **預設 GCS 靜態網址** 不支援直接綁定自訂網域的 HTTPS。若您要使用自訂網域並啟用 HTTPS，強烈建議在 GCS 前方架設 **GCP HTTPS Load Balancer** 並搭配 **Cloud CDN**。
- 或者，您可以使用 Cloudflare 作為 DNS 解析，並開啟 Proxy 模式 (橘色雲朵) 來為您的 GCS 靜態網站提供免費 of SSL 憑證。

---

## 🛠️ 常見維護指令速查

* **查看 Cloud Run 日誌**：
  ```bash
  gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=badminton-party-api" --limit 50
  ```
* **重新發布後端 API**：
  更新程式碼後，重新執行 **後端 API 部署** 的步驟 1 即可。Cloud Run 會自動執行無中斷滾動更新。

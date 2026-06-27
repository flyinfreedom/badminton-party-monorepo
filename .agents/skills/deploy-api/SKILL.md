---
name: deploy-api
description: 協助部署後端 API 到 GCP Cloud Run。此 skill 可以記住目前的 API tag 號碼，自動遞增版號或使用指定版號，執行映像檔打包、推送至 Artifact Registry，並部署到 Cloud Run。
---

# Deploy API Skill

此 Skill 協助使用者管理 API 版本與部署流程。當使用者提出部署 API 或是使用此 skill 時，請使用本 Skill 定義的流程執行。

## 狀態儲存
此 Skill 將最新的 Tag 資訊儲存在專案的 `.agents/skills/deploy-api/state.json` 檔案中。
檔案格式：
```json
{
  "latest_tag": "1.0.0"
}
```

## 執行流程與指引
當使用者提出部署 API 或是想要使用 deploy-api 相關功能時，你必須執行以下步驟：

1. **讀取目前版本**：
   - 讀取 [state.json](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/.agents/skills/deploy-api/state.json)，取得目前的 `latest_tag`。如果檔案不存在或沒有 `latest_tag`，預設為 `1.0.0`。

2. **確認目標版本 (Tag)**：
   - 如果使用者在請求中已指定 Tag (例如「我要部署 1.0.5 版」)，則使用該指定 Tag。
   - 如果使用者沒有指定（例如只說「我要部署 API」或「自動遞增」），則自動將 `latest_tag` 的最後一個數字加 1。
     - 例如：`1.0.0` -> `1.0.1`；`1.2.14` -> `1.2.15`。
     - 若版本號格式非標準 SemVer，請試著解析最後一段數字並加 1。
   - 詢問使用者是否要使用該版本進行部署。例如：「目前最新版本是 {latest_tag}，即將為您部署新版本 {target_tag}。是否確定開始部署？」
   - 若使用者同意，則繼續。

3. **更新版本狀態**：
   - 將最新的 `target_tag` 寫回 [state.json](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/.agents/skills/deploy-api/state.json)。

4. **執行打包與推送**：
   - 執行專案下的 [build-api.ps1](file:///D:/BadmintonParty/SourceCode/badminton-party-monorepo/deployment/build-api.ps1) 並傳入 `target_tag`。使用 PowerShell 執行：
     ```powershell
     ./deployment/build-api.ps1 {target_tag}
     ```

5. **執行 Cloud Run 部署**：
   - 使用 PowerShell 執行以下 `gcloud` 指令：
     ```powershell
     gcloud run deploy badminton-party-api `
       --image=asia-east1-docker.pkg.dev/badminton-party-488004/badminton-party-api/badminton-party-api:{target_tag} `
       --platform=managed `
       --region=asia-east1 `
       --allow-unauthenticated `
       --port=8080
     ```

6. **完成通知**：
   - 部署完成後，向使用者回報部署成功，並附上 Cloud Run 服務的網址。

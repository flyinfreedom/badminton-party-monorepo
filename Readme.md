# BadmintonParty Monorepo

本專案使用 .NET Aspire 進行本地微服務與基礎設施（如 PostgreSQL）的開發管理。

---

## 🛠️ 開發前置設定：設定 PostgreSQL 本地密碼

為了保障開發環境的安全性，本專案的 PostgreSQL 密碼使用 **.NET User Secrets** 進行本機儲存與管理，不應將機敏密碼直接提交至 Git。

在使用本專案之前，請務必先至 `BadmintonParty.AppHost` 專案目錄下，為 PostgreSQL 設定本地開發用的密碼。

### 📋 操作指南

請打開您的終端機（Terminal）並依循以下步驟操作：

#### 1. 移動至 AppHost 專案目錄
```bash
cd src/Aspire/BadmintonParty.AppHost
```

#### 2. 設定 PostgreSQL 密碼 (User Secrets)
請使用 `dotnet user-secrets set` 指令，將 `"Parameters:postgres-password"` 設定為您偏好的密碼：
```bash
dotnet user-secrets set "Parameters:postgres-password" "您的密碼"
```
*例如，設定為 `postgres`：*
```bash
dotnet user-secrets set "Parameters:postgres-password" "postgres"
```

#### 3. 檢視目前已設定的 Secret 內容
若要確認是否設定成功，可以列出目前專案內所有的機敏變數：
```bash
dotnet user-secrets list
```

#### 4. 移除已設定的密碼
若未來需要清除此密碼，可執行：
```bash
dotnet user-secrets remove "Parameters:postgres-password"
```

---

## 🚀 啟動專案
設定完畢後，您就可以在根目錄下執行以下指令啟動 .NET Aspire AppHost：

```bash
dotnet run --project src/Aspire/BadmintonParty.AppHost/BadmintonParty.AppHost.csproj
```
或者在您的 IDE 中（如 VS Code 或 Visual Studio）將 `BadmintonParty.AppHost` 設為啟動專案直接點擊執行。



請修改Aspire專案內 program.cs 內的 var postgresPassword = builder.AddParameter("postgres-password", "postgres", secret: true); 
改為從 user secret 取得密碼

# Paytak – Docker ve Azure’a Push

## Yerel çalıştırma (Docker Compose)

1. **Görüntü oluştur ve container’ı çalıştır:**
   ```bash
   cd c:\Users\BP\Desktop\Paytak
   docker compose up -d --build
   ```
2. Uygulama: **http://localhost:8080**
3. Durdurmak için: `docker compose down`

`.env` dosyası `Paytak/.env` yolunda olmalı (Azure OpenAI ve SQL bağlantı bilgileri).

---

## Azure’a push (Azure Container Registry)

### 1. Azure Container Registry (ACR) oluşturma

Azure CLI ile (PowerShell veya terminal):

```bash
# Giriş
az login

# Kaynak grubu (yoksa oluştur)
az group create --name rg-paytak --location eastus2

# ACR oluştur (ad benzersiz olmalı, küçük harf ve rakam)
az acr create --resource-group rg-paytak --name paytakacr --sku Basic
```

`paytakacr` yerine kendi ACR adınızı yazın (global benzersiz).

### 2. ACR’ye giriş ve görüntü push

```bash
# ACR’ye giriş
az acr login --name paytakacr

# Görüntüyü build et
docker compose build

# ACR için etiketle (paytakacr = ACR adınız, .azurecr.io otomatik eklenir)
docker tag paytak:latest paytakacr.azurecr.io/paytak:latest

# Push
docker push paytakacr.azurecr.io/paytak:latest
```

ACR adınız farklıysa `paytakacr` kısmını değiştirin.

### 3. Azure’da çalıştırma seçenekleri

**A) Azure Container Apps**

```bash
# Container Apps ortamı
az containerapp env create --name paytak-env --resource-group rg-paytak --location eastus2

# ACR admin kullanıcı aç (gerekirse)
az acr update --name paytakacr --admin-enabled true
az acr credential show --name paytakacr

# Container App oluştur (ortam değişkenlerini Azure portal veya CLI ile ekleyin)
az containerapp create \
  --name paytak \
  --resource-group rg-paytak \
  --environment paytak-env \
  --image paytakacr.azurecr.io/paytak:latest \
  --registry-server paytakacr.azurecr.io \
  --registry-username <ACR_USERNAME> \
  --registry-password <ACR_PASSWORD> \
  --target-port 8080 \
  --ingress external
```

Ortam değişkenleri (CONNECTION_STRING, AZURE_OPENAI_*) için: Portal → Container App → **Containers** → **Edit and deploy** → **Environment variables** kısmından ekleyin.

**B) Azure App Service (Web App for Containers)**

Portal’dan:

1. **Create a resource** → **Web App**.
2. **Publish**: Docker Container.
3. **Container source**: Azure Container Registry → ilgili ACR ve `paytak:latest` görüntüsünü seçin.
4. **App Service plan** ve **Resource group** seçin.
5. Oluşturduktan sonra **Configuration** → **Application settings** ile `CONNECTION_STRING`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION` ekleyin.

---

## Özet komutlar (ACR push)

```bash
cd c:\Users\BP\Desktop\Paytak
docker compose build
az acr login --name paytakacr
docker tag paytak:latest paytakacr.azurecr.io/paytak:latest
docker push paytakacr.azurecr.io/paytak:latest
```

`paytakacr` kısmını kendi ACR adınızla değiştirin.

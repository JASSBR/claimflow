#!/usr/bin/env bash
# Deploys the ClaimFlow API to Azure: PostgreSQL Flexible Server, Blob Storage, Azure Container Apps.
# Re-runnable: resources are created once, then the app is updated with a fresh image.
# Prerequisites: `az login`, Docker. Optional: ANTHROPIC_API_KEY (enables the AI review), SPA_ORIGINS (CORS).
#
# Azure for Students gotchas encoded here:
#  - the subscription policy only allows a few regions (italynorth, norwayeast, austriaeast, belgiumcentral, polandcentral);
#  - ACR Tasks (remote builds) is forbidden, so the image is built locally — for linux/amd64, mandatory on Apple Silicon.
set -euo pipefail
cd "$(dirname "$0")/.."

LOCATION="${LOCATION:-italynorth}"
RG="${RG:-rg-claimflow}"
ACR="${ACR:-ca4763678411acr}"            # registry shared with other projects (Basic SKU, admin user enabled)
ENVIRONMENT="${ENVIRONMENT:-claimflow-env}"
APP="${APP:-claimflow-api}"
# Comma-separated origins of the SPA, allowed by the API's CORS policy.
SPA_ORIGINS="${SPA_ORIGINS:-https://claimflow-insurance.vercel.app,https://claimflow-indol.vercel.app}"

# Generated once and kept outside git: re-running must not rotate the database password under a live app.
STATE=deploy/.azure.env
if [ ! -f "$STATE" ]; then
  umask 077
  cat > "$STATE" <<STATE_EOF
SUFFIX=$(openssl rand -hex 3)
PG_PASSWORD=$(openssl rand -base64 30 | tr -dc 'A-Za-z0-9' | head -c 32)Aa1
DEMO_SIGNING_KEY=$(openssl rand -base64 48 | tr -d '\n')
STATE_EOF
fi
# shellcheck source=/dev/null
source "$STATE"
PG="claimflow-pg-$SUFFIX"
STORAGE="claimflowst$SUFFIX"

az account show >/dev/null 2>&1 || { echo "✗ Not logged in: run 'az login' first."; exit 1; }
echo "→ Subscription: $(az account show --query name -o tsv)"
az extension add --name containerapp --upgrade --only-show-errors >/dev/null
for ns in Microsoft.App Microsoft.DBforPostgreSQL Microsoft.Storage; do
  az provider register --namespace "$ns" --wait --only-show-errors >/dev/null &
done
wait
az group create -n "$RG" -l "$LOCATION" --only-show-errors >/dev/null

echo "→ PostgreSQL $PG (Burstable B1ms)"
if ! az postgres flexible-server show -g "$RG" -n "$PG" --only-show-errors >/dev/null 2>&1; then
  # public-access 0.0.0.0 = only Azure services (the Container App) may connect; no public client IP is allowed.
  az postgres flexible-server create -g "$RG" -n "$PG" -l "$LOCATION" \
    --tier Burstable --sku-name Standard_B1ms --storage-size 32 --version 17 \
    --admin-user claimflow --admin-password "$PG_PASSWORD" \
    --public-access 0.0.0.0 --yes --only-show-errors >/dev/null
fi
# Created separately: recent CLI versions reserve --database-name on create for elastic clusters.
az postgres flexible-server db show -g "$RG" --server-name "$PG" --name claimflow --only-show-errors >/dev/null 2>&1 || \
  az postgres flexible-server db create -g "$RG" --server-name "$PG" --name claimflow --only-show-errors >/dev/null
DB_CONNECTION="Host=$PG.postgres.database.azure.com;Database=claimflow;Username=claimflow;Password=$PG_PASSWORD;SSL Mode=Require"

echo "→ Storage account $STORAGE (private blobs)"
if ! az storage account show -g "$RG" -n "$STORAGE" --only-show-errors >/dev/null 2>&1; then
  az storage account create -g "$RG" -n "$STORAGE" -l "$LOCATION" --sku Standard_LRS --kind StorageV2 \
    --min-tls-version TLS1_2 --allow-blob-public-access false --only-show-errors >/dev/null
fi
BLOB_CONNECTION="$(az storage account show-connection-string -g "$RG" -n "$STORAGE" --query connectionString -o tsv)"

SERVER="$(az acr show -n "$ACR" --query loginServer -o tsv)"
IMAGE="$SERVER/claimflow-api:$(git rev-parse --short HEAD)-$(date +%H%M%S)"
echo "→ Image $IMAGE (local linux/amd64 build)"
az acr login -n "$ACR" >/dev/null
docker buildx build --platform linux/amd64 -t "$IMAGE" --push . >/dev/null

echo "→ Container Apps environment $ENVIRONMENT"
# Recent CLIs default to "Express" environments, which reject revision suffixes and did not reliably roll out
# configuration changes. Ask for a standard workload-profiles environment (Consumption profile: pay per use).
MODE="$(az containerapp env show -n "$ENVIRONMENT" -g "$RG" --query properties.environmentMode -o tsv 2>/dev/null || true)"
if [ "$MODE" = "Express" ]; then
  echo "  replacing Express environment"
  az containerapp delete -n "$APP" -g "$RG" --yes --only-show-errors >/dev/null 2>&1 || true
  az containerapp env delete -n "$ENVIRONMENT" -g "$RG" --yes --only-show-errors >/dev/null
  MODE=""
fi
[ -n "$MODE" ] || az containerapp env create -n "$ENVIRONMENT" -g "$RG" -l "$LOCATION" \
  --environment-mode WorkloadProfiles --logs-destination none --only-show-errors >/dev/null

SECRETS=(db="$DB_CONNECTION" blobs="$BLOB_CONNECTION" demokey="$DEMO_SIGNING_KEY")
ENV_VARS=(
  ASPNETCORE_ENVIRONMENT=Production
  ConnectionStrings__claimflow=secretref:db
  ConnectionStrings__blobs=secretref:blobs
  Auth__Mode=Demo
  Auth__DemoSigningKey=secretref:demokey
  # Demo environment: migrate + seed at startup (production would run the EF bundle as a pipeline step, ADR 0003).
  Database__InitializeOnStartup=true
  Database__SeedDemoData=true
  # The app is only reachable through the Container Apps ingress, so its X-Forwarded-For can be trusted.
  ForwardedHeaders__TrustPlatformProxy=true
)
IFS=',' read -r -a ORIGINS <<< "$SPA_ORIGINS"
for index in "${!ORIGINS[@]}"; do ENV_VARS+=("Cors__AllowedOrigins__$index=${ORIGINS[$index]}"); done
if [ -n "${ANTHROPIC_API_KEY:-}" ]; then
  SECRETS+=(anthropic="$ANTHROPIC_API_KEY")
  ENV_VARS+=(Ai__ApiKey=secretref:anthropic)
fi

echo "→ Container App $APP"
if az containerapp show -n "$APP" -g "$RG" --only-show-errors >/dev/null 2>&1; then
  az containerapp secret set -n "$APP" -g "$RG" --secrets "${SECRETS[@]}" --only-show-errors >/dev/null
  # A unique suffix forces a fresh revision: every deployment is traceable and picks up secrets and settings.
  az containerapp update -n "$APP" -g "$RG" --image "$IMAGE" --set-env-vars "${ENV_VARS[@]}" \
    --revision-suffix "r$(date +%m%d%H%M%S)" --only-show-errors >/dev/null
else
  az containerapp create -n "$APP" -g "$RG" --environment "$ENVIRONMENT" --image "$IMAGE" \
    --registry-server "$SERVER" \
    --registry-username "$(az acr credential show -n "$ACR" --query username -o tsv)" \
    --registry-password "$(az acr credential show -n "$ACR" --query 'passwords[0].value' -o tsv)" \
    --target-port 8080 --ingress external \
    --cpu 0.5 --memory 1.0Gi \
    `# At most one replica: SignalR groups live in memory (scaling out needs Azure SignalR Service).` \
    `# Zero when idle: a demo, not a service; the first request after a quiet period wakes it in seconds.` \
    --min-replicas 0 --max-replicas 1 \
    --secrets "${SECRETS[@]}" --env-vars "${ENV_VARS[@]}" --only-show-errors >/dev/null
fi

URL="https://$(az containerapp show -n "$APP" -g "$RG" --query properties.configuration.ingress.fqdn -o tsv)"
echo "→ Waiting for $URL/health"
for _ in $(seq 1 60); do curl -sf "$URL/health" >/dev/null && break; sleep 5; done
echo "✓ API: $URL  (health: $(curl -s "$URL/health"))"

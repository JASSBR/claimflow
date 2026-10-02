#!/usr/bin/env bash
# Builds the Angular SPA (French and English) and deploys it to Vercel as static files.
# Usage: API_URL=https://<container-app-fqdn> ./deploy/vercel.sh   (requires `vercel login`)
set -euo pipefail
cd "$(dirname "$0")/.."
: "${API_URL:?Set API_URL to the API origin, e.g. https://claimflow-api.<region>.azurecontainerapps.io}"
API_HOST="${API_URL#https://}"

# The API origin is compiled into the production build (and allowed by the CSP below).
sed -i.bak -E "s#apiBaseUrl: '[^']*'#apiBaseUrl: '${API_URL}'#" web/src/environments/environment.production.ts
rm -f web/src/environments/environment.production.ts.bak

(cd web && npm ci --no-audit --no-fund >/dev/null && npx ng build --configuration production >/dev/null)
OUT=web/dist/web/browser

# Strict CSP: scripts and fonts from the app itself only; API calls and the SignalR WebSocket to the API origin only.
CSP="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self'; connect-src 'self' https://${API_HOST} wss://${API_HOST}; frame-src blob:; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'"
cat > "$OUT/vercel.json" <<JSON
{
  "redirects": [
    { "source": "/", "has": [{ "type": "header", "key": "accept-language", "value": "en.*" }], "destination": "/en/", "permanent": false },
    { "source": "/", "destination": "/fr/", "permanent": false }
  ],
  "rewrites": [
    { "source": "/fr/:path*", "destination": "/fr/index.html" },
    { "source": "/en/:path*", "destination": "/en/index.html" }
  ],
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        { "key": "Content-Security-Policy", "value": "${CSP}" },
        { "key": "X-Content-Type-Options", "value": "nosniff" },
        { "key": "Referrer-Policy", "value": "strict-origin-when-cross-origin" },
        { "key": "Permissions-Policy", "value": "camera=(), microphone=(), geolocation=()" }
      ]
    },
    {
      "source": "/(fr|en)/(.*)-([A-Z0-9]{8}).(js|css|woff2)",
      "headers": [{ "key": "Cache-Control", "value": "public, max-age=31536000, immutable" }]
    }
  ]
}
JSON

cd "$OUT"
VERCEL=$(command -v vercel || echo "npx --yes vercel")
DEPLOYMENT=$($VERCEL deploy --prod --yes 2>/dev/null | grep -Eo 'https://claimflow-[a-z0-9]+-[a-z0-9-]+\.vercel\.app' | head -1)
# Stable public name for the CV and the README (the project's default alias is auto-generated).
$VERCEL alias set "$DEPLOYMENT" "${ALIAS:-claimflow-insurance.vercel.app}" >/dev/null
echo "✓ SPA: https://${ALIAS:-claimflow-insurance.vercel.app}  (deployment $DEPLOYMENT)"

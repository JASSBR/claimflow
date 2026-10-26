---
applyTo: '**/*.sh'
---

# Rules for .sh files

Each rule cites the code it rests on. A rule marked not yet reviewed is the estate's best reading, not a decision.

- [BusinessRule] Static assets with 8-character uppercase hexadecimal/alphanumeric hash fingerprints matching '/(fr|en)/(.*)-([A-Z0-9]{8}).(js|css|woff2)' are served with long-term immutable caching headers set to 'public, max-age=31536000, immutable'. [deploy/vercel.sh:L36-L39]
- [BusinessRule] Requests to the root path '/' are redirected conditionally based on the Accept-Language header: requests matching 'en.*' redirect with a non-permanent redirect to '/en/', while all other requests default to '/fr/'. In addition, paths matching '/fr/:path*' rewrite to '/fr/index.html' and paths matching '/en/:path*' rewrite to '/en/index.html'. [deploy/vercel.sh:L19-L26]
- [BusinessRule] The deployment script requires the environment variable API_URL to be set to the API origin (such as https://claimflow-api.<region>.azurecontainerapps.io); execution halts immediately if API_URL is unset or empty. [deploy/vercel.sh:L6-L7]
- [BusinessRule] The deployed application enforces a strict Content-Security-Policy (CSP) that permits scripts and fonts only from 'self', styles from 'self' and 'unsafe-inline', images from 'self', data:, and blob:, frames from blob:, forbids object embeds and frame-ancestors ('none'), and restricts connect-src to 'self' alongside https:// and wss:// connections to the configured API_HOST. [deploy/vercel.sh:L16-L17]

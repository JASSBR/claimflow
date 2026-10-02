// k6 load test: a claims back-office under a busy day's traffic.
// 80 % reads (portfolio list, statistics, claim file) and 20 % writes (declaration + taking a claim into review),
// all authenticated, against the production container image.
//   docker run --rm -i --network <net> -e BASE_URL=http://claimflow-api:8080 grafana/k6 run - < tests/load/claims.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
// STRESS=1: no think time, constant pressure, to find the throughput ceiling instead of simulating users.
const STRESS = __ENV.STRESS === '1';

export const options = {
  scenarios: {
    backoffice: STRESS
      ? { executor: 'constant-vus', vus: 100, duration: '45s' }
      : {
          executor: 'ramping-vus',
          stages: [
            { duration: '20s', target: 25 },
            { duration: '60s', target: 50 },
            { duration: '20s', target: 0 },
          ],
        },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{kind:read}': ['p(95)<200'],
    'http_req_duration{kind:write}': ['p(95)<400'],
  },
};

export function setup() {
  const token = (persona) =>
    http.post(`${BASE_URL}/api/auth/token`, JSON.stringify({ personaId: persona }), { headers: { 'Content-Type': 'application/json' } }).json('accessToken');
  return { handler: token('lea') };
}

export default function (data) {
  const headers = { Authorization: `Bearer ${data.handler}`, 'Content-Type': 'application/json' };

  if (Math.random() < 0.8) {
    const list = http.get(`${BASE_URL}/api/claims?pageSize=20`, { headers, tags: { kind: 'read', name: 'list' } });
    check(list, { 'list 200': (r) => r.status === 200 });
    check(http.get(`${BASE_URL}/api/claims/stats`, { headers, tags: { kind: 'read', name: 'stats' } }), { 'stats 200': (r) => r.status === 200 });
    const items = list.json('items') || [];
    if (items.length > 0) {
      const id = items[Math.floor(Math.random() * items.length)].id;
      check(http.get(`${BASE_URL}/api/claims/${id}`, { headers, tags: { kind: 'read', name: 'detail' } }), { 'detail 200': (r) => r.status === 200 });
    }
  } else {
    const today = new Date().toISOString().slice(0, 10);
    const declared = http.post(
      `${BASE_URL}/api/claims`,
      JSON.stringify({
        policyNumber: `POL-${100000 + Math.floor(Math.random() * 899999)}`,
        type: 'Home',
        incidentDate: today,
        description: 'Load test: water damage in the kitchen.',
        claimedAmount: 1500,
      }),
      { headers, tags: { kind: 'write', name: 'declare' } },
    );
    check(declared, { 'declare 201': (r) => r.status === 201 });
    if (declared.status === 201) {
      const claim = declared.json();
      const reviewed = http.post(
        `${BASE_URL}/api/claims/${claim.id}/actions`,
        JSON.stringify({ action: 'StartReview', expectedVersion: claim.version }),
        { headers, tags: { kind: 'write', name: 'start-review' } },
      );
      check(reviewed, { 'review 200': (r) => r.status === 200 });
    }
  }

  if (!STRESS) sleep(Math.random() * 0.5 + 0.25);
}

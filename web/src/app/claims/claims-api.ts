import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClaimActionRequest, ClaimDetails, DeclareClaimRequest } from './claim.models';

export const CLAIMS_URL = '/api/claims';

/** Commands only. Reads are declared where they are rendered, with httpResource, so they stay signal-driven. */
@Injectable({ providedIn: 'root' })
export class ClaimsApi {
  private readonly http = inject(HttpClient);

  declare(request: DeclareClaimRequest): Observable<ClaimDetails> {
    return this.http.post<ClaimDetails>(CLAIMS_URL, request);
  }

  apply(claimId: string, request: ClaimActionRequest): Observable<ClaimDetails> {
    return this.http.post<ClaimDetails>(`${CLAIMS_URL}/${claimId}/actions`, request);
  }
}

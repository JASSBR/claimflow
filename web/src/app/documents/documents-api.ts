import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { ClaimAnalysis, ClaimDocument } from './documents.models';

@Injectable({ providedIn: 'root' })
export class DocumentsApi {
  private readonly http = inject(HttpClient);

  upload(claimId: string, file: File): Observable<ClaimDocument> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<ClaimDocument>(`/api/claims/${claimId}/documents`, form);
  }

  analyze(claimId: string): Observable<ClaimAnalysis> {
    return this.http.post<ClaimAnalysis>(`/api/claims/${claimId}/analysis`, null);
  }

  /**
   * Files are only served to authenticated callers, so a plain link cannot open them: fetch with the bearer token,
   * then open a local object URL. The tab is opened synchronously so popup blockers allow it.
   */
  async open(claimId: string, documentId: string, page?: number | null): Promise<void> {
    const tab = globalThis.open('', '_blank');
    const blob = await firstValueFrom(
      this.http.get(`/api/claims/${claimId}/documents/${documentId}/content`, {
        responseType: 'blob',
      }),
    );
    const url = URL.createObjectURL(blob);
    const target = page ? `${url}#page=${page}` : url;
    if (tab) {
      tab.location.href = target;
    } else {
      globalThis.open(target, '_blank');
    }
    setTimeout(() => URL.revokeObjectURL(url), 60_000);
  }
}

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ToastService } from '../../core/toast';
import { settle } from '../../testing';
import { DocumentsPanel } from './documents-panel';

describe('DocumentsPanel', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function render(canUpload: boolean) {
    const fixture = TestBed.createComponent(DocumentsPanel);
    fixture.componentRef.setInput('claimId', 'c1');
    fixture.componentRef.setInput('documents', []);
    fixture.componentRef.setInput('canUpload', canUpload);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('uploads dropped files as multipart and reports the outcome per file', async () => {
    const { fixture, element } = render(true);
    let uploaded = 0;
    fixture.componentInstance.uploaded.subscribe(() => uploaded++);
    // jsdom has no DataTransfer: the component only reads `.files`, so an equivalent object is enough.
    const transfer = {
      files: [
        new File(['%PDF-1.7'], 'devis.pdf', { type: 'application/pdf' }),
        new File(['MZ'], 'virus.pdf', { type: 'application/pdf' }),
      ],
    };

    const drop = new Event('drop') as DragEvent;
    Object.defineProperty(drop, 'dataTransfer', { value: transfer });
    element.querySelector('.dropzone')!.dispatchEvent(drop);

    const first = http.expectOne('/api/claims/c1/documents');
    expect((first.request.body as FormData).get('file')).toBeInstanceOf(File);
    first.flush({ id: 'd1' });
    await settle();
    http
      .expectOne('/api/claims/c1/documents')
      .flush(
        { errors: { 'document.unsupported_format': ['Only PDF…'] } },
        { status: 400, statusText: 'Bad Request' },
      );
    await settle();

    expect(
      TestBed.inject(ToastService)
        .toasts()
        .map((toast) => toast.tone),
    ).toEqual(['success', 'error']);
    expect(uploaded).toBe(1);
  });

  it('hides the drop zone from read-only users', () => {
    expect(render(false).element.querySelector('.dropzone')).toBeNull();
  });
});

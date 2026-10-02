import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { environment } from '../../environments/environment';
import { Auth } from '../core/auth/auth';
import { Persona } from '../core/auth/auth.models';
import { ClaimsRealtime } from '../core/realtime/claims-realtime';
import { ThemeService } from '../core/theme';
import { ClaimCapabilities } from '../claims/claim.models';
import { Avatar } from '../shared/avatar';
import { Icon } from '../shared/icon';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Avatar, Icon],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  protected readonly auth = inject(Auth);
  protected readonly realtime = inject(ClaimsRealtime);
  protected readonly theme = inject(ThemeService);
  private readonly router = inject(Router);

  protected readonly repositoryUrl = environment.repositoryUrl;
  protected readonly menuOpen = signal(false);
  protected readonly capabilities = httpResource<ClaimCapabilities>(
    () => '/api/claims/capabilities',
  );
  protected readonly personas = httpResource<Persona[]>(() =>
    this.menuOpen() ? '/api/auth/personas' : undefined,
  );

  constructor() {
    void this.realtime.connect();
    inject(DestroyRef).onDestroy(() => void this.realtime.disconnect());
  }

  protected switchTo(persona: Persona): void {
    this.menuOpen.set(false);
    this.auth.loginAs(persona.id).subscribe(() => {
      // A new identity changes permissions everywhere: reload the current view from scratch.
      const url = this.router.url;
      void this.router
        .navigateByUrl('/', { skipLocationChange: true })
        .then(() => this.router.navigateByUrl(url));
      this.capabilities.reload();
    });
  }
}

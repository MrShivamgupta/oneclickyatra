import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-placeholder',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-placeholder.html',
  styleUrl: './admin-placeholder.scss'
})
export class AdminPlaceholder {
  private readonly route = inject(ActivatedRoute);

  readonly title = this.route.snapshot.data['title'] as string;
  readonly description = this.route.snapshot.data['description'] as string;
  readonly icon = this.route.snapshot.data['icon'] as string | undefined;
  readonly phase = (this.route.snapshot.data['phase'] as string | undefined) ?? 'Phase 6';
}

import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { Spinner } from '../../../shared/components/spinner/spinner';
import { LeadService } from '../../../core/services/lead.service';
import { DestinationService } from '../../../core/services/destination.service';
import { UserService } from '../../../core/services/user.service';
import { FollowUpService } from '../../../core/services/followup.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Destination } from '../../../core/models/master-data.models';
import { FollowUp, FOLLOW_UP_TYPES, FollowUpType, Lead, LEAD_STATUSES, UserSummary } from '../../../core/models/crm.models';

@Component({
  selector: 'app-lead-detail',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, ConfirmationDialog, DatePipe, Spinner],
  templateUrl: './lead-detail.html',
  styleUrl: './lead-detail.scss'
})
export class LeadDetail {
  private readonly leadService = inject(LeadService);
  private readonly destinationService = inject(DestinationService);
  private readonly userService = inject(UserService);
  private readonly followUpService = inject(FollowUpService);
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly statuses = LEAD_STATUSES;
  readonly followUpTypes = FOLLOW_UP_TYPES;

  readonly leadId = signal<string | null>(null);
  readonly isNew = signal(false);
  readonly lead = signal<Lead | null>(null);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly destinations = signal<Destination[]>([]);
  readonly staff = signal<UserSummary[]>([]);
  readonly followUps = signal<FollowUp[]>([]);

  readonly basicError = signal<string | null>(null);
  readonly basicSaving = signal(false);

  readonly statusValue = signal('New');
  readonly statusSaving = signal(false);
  readonly statusError = signal<string | null>(null);

  readonly scoreValue = signal(0);
  readonly scoreSaving = signal(false);
  readonly scoreError = signal<string | null>(null);

  readonly assignValue = signal('');
  readonly assignSaving = signal(false);
  readonly assignError = signal<string | null>(null);

  readonly claimSaving = signal(false);
  readonly claimError = signal<string | null>(null);

  // Backend enforces the real rule (LeadAppFunction.EnsureCanManage / AssignAsync's self-claim
  // check) -- these mirror it here only so the UI doesn't invite an action that's guaranteed to
  // come back as a 403, not as a second, independently-trusted security boundary.
  readonly isSuperAdmin = computed(() => this.authService.currentUser()?.roles.includes('SuperAdmin') ?? false);
  readonly isOwner = computed(() => {
    const l = this.lead();
    const userId = this.authService.currentUser()?.userId;
    return l != null && userId != null && l.assignedToUserId === userId;
  });
  readonly canManage = computed(() => this.isNew() || this.isSuperAdmin() || this.isOwner());
  readonly isUnassigned = computed(() => (this.lead()?.assignedToUserId ?? null) === null);
  readonly canClaim = computed(() => !this.isNew() && !this.isSuperAdmin() && this.isUnassigned());

  readonly convertSaving = signal(false);
  readonly convertError = signal<string | null>(null);

  readonly followUpError = signal<string | null>(null);
  readonly followUpSaving = signal(false);
  readonly followUpStatusUpdatingId = signal<string | null>(null);
  readonly followUpStatusError = signal<string | null>(null);

  readonly deleteConfirmOpen = signal(false);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly basicForm = this.formBuilder.nonNullable.group({
    customerName: ['', [Validators.required, Validators.maxLength(150)]],
    mobile: ['', Validators.required],
    email: [''],
    destinationId: [''],
    travelDate: [''],
    budget: [null as number | null],
    source: ['']
  });

  readonly followUpForm = this.formBuilder.nonNullable.group({
    scheduledAt: ['', Validators.required],
    type: ['Call' as FollowUpType, Validators.required],
    notes: ['']
  });

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });
    this.userService.listStaff().subscribe((response) => {
      if (response.success && response.data) this.staff.set(response.data);
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam || idParam === 'new') {
      this.isNew.set(true);
      return;
    }

    this.leadId.set(idParam);
    this.load();
  }

  load(): void {
    const id = this.leadId();
    if (!id) return;
    this.loading.set(true);
    this.loadError.set(null);
    this.leadService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load this lead. Please try again.');
      }
    });
    this.loadFollowUps();
  }

  private applyLead(lead: Lead): void {
    this.lead.set(lead);
    this.basicForm.setValue({
      customerName: lead.customerName,
      mobile: lead.mobile,
      email: lead.email ?? '',
      destinationId: lead.destinationId ?? '',
      travelDate: lead.travelDate ?? '',
      budget: lead.budget ?? null,
      source: lead.source ?? ''
    });
    this.statusValue.set(lead.status);
    this.scoreValue.set(lead.leadScore);
    this.assignValue.set(lead.assignedToUserId ?? '');
  }

  loadFollowUps(): void {
    const id = this.leadId();
    if (!id) return;
    this.leadService.listFollowUps(id).subscribe((response) => {
      if (response.success && response.data) this.followUps.set(response.data);
    });
  }

  private buildLeadRequest() {
    const raw = this.basicForm.getRawValue();
    return {
      customerName: raw.customerName,
      mobile: raw.mobile,
      email: raw.email || null,
      destinationId: raw.destinationId || null,
      travelDate: raw.travelDate || null,
      budget: raw.budget,
      source: raw.source || null
    };
  }

  saveBasic(): void {
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }

    this.basicSaving.set(true);
    this.basicError.set(null);
    const request = this.buildLeadRequest();

    if (this.isNew()) {
      this.leadService.create(request).subscribe({
        next: (response) => {
          this.basicSaving.set(false);
          if (response.success && response.data) {
            // Apply the created lead's state directly instead of relying on the route
            // navigation to re-trigger init logic -- /leads/new and /leads/:id are the
            // same route, so Angular's route-reuse strategy keeps this component instance
            // alive across the navigate() below and its constructor-only load logic never
            // re-runs.
            this.isNew.set(false);
            this.leadId.set(response.data.id);
            this.applyLead(response.data);
            this.loadFollowUps();
            this.router.navigate(['/admin/leads', response.data.id], { replaceUrl: true });
          }
        },
        error: (error) => {
          this.basicSaving.set(false);
          this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
        }
      });
      return;
    }

    const id = this.leadId();
    if (!id) return;
    this.leadService.update(id, request).subscribe({
      next: (response) => {
        this.basicSaving.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.basicSaving.set(false);
        this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  saveStatus(): void {
    const id = this.leadId();
    if (!id) return;
    this.statusSaving.set(true);
    this.statusError.set(null);
    this.leadService.updateStatus(id, this.statusValue()).subscribe({
      next: (response) => {
        this.statusSaving.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.statusSaving.set(false);
        this.statusError.set(error?.error?.message ?? 'Could not update the status. Please try again.');
      }
    });
  }

  saveScore(): void {
    const id = this.leadId();
    if (!id) return;
    this.scoreSaving.set(true);
    this.scoreError.set(null);
    this.leadService.updateScore(id, this.scoreValue()).subscribe({
      next: (response) => {
        this.scoreSaving.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.scoreSaving.set(false);
        this.scoreError.set(error?.error?.message ?? 'Could not update the score. Please try again.');
      }
    });
  }

  saveAssign(): void {
    const id = this.leadId();
    if (!id || !this.assignValue()) return;
    this.assignSaving.set(true);
    this.assignError.set(null);
    this.leadService.assign(id, this.assignValue()).subscribe({
      next: (response) => {
        this.assignSaving.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.assignSaving.set(false);
        this.assignError.set(error?.error?.message ?? 'Could not assign this lead. Please try again.');
      }
    });
  }

  claimLead(): void {
    const id = this.leadId();
    const userId = this.authService.currentUser()?.userId;
    if (!id || !userId) return;
    this.claimSaving.set(true);
    this.claimError.set(null);
    this.leadService.assign(id, userId).subscribe({
      next: (response) => {
        this.claimSaving.set(false);
        if (response.success && response.data) this.applyLead(response.data);
      },
      error: (error) => {
        this.claimSaving.set(false);
        this.claimError.set(error?.error?.message ?? 'Could not claim this lead. Please try again.');
      }
    });
  }

  convertToCustomer(): void {
    const id = this.leadId();
    if (!id) return;
    this.convertSaving.set(true);
    this.convertError.set(null);
    this.leadService.convertToCustomer(id).subscribe({
      next: () => {
        this.convertSaving.set(false);
        this.load();
      },
      error: (error) => {
        this.convertSaving.set(false);
        this.convertError.set(error?.error?.message ?? 'Could not convert this lead. Please try again.');
      }
    });
  }

  scheduleFollowUp(): void {
    if (this.followUpForm.invalid) {
      this.followUpForm.markAllAsTouched();
      return;
    }
    const id = this.leadId();
    if (!id) return;

    this.followUpSaving.set(true);
    this.followUpError.set(null);
    const raw = this.followUpForm.getRawValue();

    this.leadService.createFollowUp(id, { scheduledAt: raw.scheduledAt, type: raw.type, notes: raw.notes || null }).subscribe({
      next: () => {
        this.followUpSaving.set(false);
        this.followUpForm.reset({ scheduledAt: '', type: 'Call', notes: '' });
        this.loadFollowUps();
      },
      error: (error) => {
        this.followUpSaving.set(false);
        this.followUpError.set(error?.error?.message ?? 'Could not schedule follow-up.');
      }
    });
  }

  markFollowUpStatus(followUp: FollowUp, status: 'Completed' | 'Cancelled'): void {
    this.followUpStatusUpdatingId.set(followUp.id);
    this.followUpStatusError.set(null);
    this.followUpService.updateStatus(followUp.id, { status }).subscribe({
      next: () => {
        this.followUpStatusUpdatingId.set(null);
        this.loadFollowUps();
      },
      error: (error) => {
        this.followUpStatusUpdatingId.set(null);
        this.followUpStatusError.set(error?.error?.message ?? 'Could not update this follow-up. Please try again.');
      }
    });
  }

  confirmDelete(): void {
    this.deleteError.set(null);
    this.deleteConfirmOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteConfirmOpen.set(false);
  }

  performDelete(): void {
    const id = this.leadId();
    if (!id) return;
    this.deleting.set(true);
    this.deleteError.set(null);
    this.leadService.delete(id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteConfirmOpen.set(false);
        this.router.navigate(['/admin/leads']);
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this lead. Please try again.');
      }
    });
  }
}

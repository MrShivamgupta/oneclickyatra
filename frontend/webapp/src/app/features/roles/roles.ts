import { Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { RoleService } from '../../core/services/role.service';
import { PermissionMatrix } from '../../core/models/role.models';
import { Spinner } from '../../shared/components/spinner/spinner';

interface PermissionGroup {
  name: string;
  permissions: { key: string; description?: string | null }[];
}

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [Spinner],
  templateUrl: './roles.html',
  styleUrl: './roles.scss'
})
export class Roles {
  private readonly roleService = inject(RoleService);

  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly saveSuccess = signal(false);

  readonly matrix = signal<PermissionMatrix | null>(null);

  /** roleId -> live-edited set of granted permission keys (a working copy the checkboxes bind to). */
  readonly edits = signal<Record<string, Set<string>>>({});
  /** roleId -> the permission keys as last loaded/saved from the server, for dirty-checking. */
  private original: Record<string, string[]> = {};

  readonly groups = computed<PermissionGroup[]>(() => {
    const permissions = this.matrix()?.permissions ?? [];
    const byPrefix = new Map<string, PermissionGroup>();
    for (const permission of permissions) {
      const prefix = permission.key.split('.')[0];
      if (!byPrefix.has(prefix)) {
        byPrefix.set(prefix, { name: prefix, permissions: [] });
      }
      byPrefix.get(prefix)!.permissions.push(permission);
    }
    return [...byPrefix.values()].sort((a, b) => a.name.localeCompare(b.name));
  });

  readonly dirtyRoleIds = computed<string[]>(() => {
    const edits = this.edits();
    return Object.keys(edits).filter((roleId) => this.isDirty(roleId, edits));
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.roleService.getPermissionMatrix().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.applyMatrix(response.data);
        } else {
          this.loadError.set(response.message || 'Could not load roles and permissions.');
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load roles and permissions.');
      }
    });
  }

  private applyMatrix(matrix: PermissionMatrix): void {
    this.matrix.set(matrix);
    this.original = Object.fromEntries(matrix.roles.map((role) => [role.roleId, [...role.permissionKeys].sort()]));
    this.edits.set(Object.fromEntries(matrix.roles.map((role) => [role.roleId, new Set(role.permissionKeys)])));
  }

  isChecked(roleId: string, permissionKey: string): boolean {
    return this.edits()[roleId]?.has(permissionKey) ?? false;
  }

  toggle(roleId: string, permissionKey: string): void {
    this.edits.update((current) => {
      const next = { ...current };
      const roleSet = new Set(next[roleId]);
      if (roleSet.has(permissionKey)) {
        roleSet.delete(permissionKey);
      } else {
        roleSet.add(permissionKey);
      }
      next[roleId] = roleSet;
      return next;
    });
    this.saveSuccess.set(false);
  }

  private isDirty(roleId: string, edits: Record<string, Set<string>>): boolean {
    const current = [...(edits[roleId] ?? [])].sort();
    const before = this.original[roleId] ?? [];
    return JSON.stringify(current) !== JSON.stringify(before);
  }

  hasChanges(): boolean {
    return this.dirtyRoleIds().length > 0;
  }

  saveChanges(): void {
    const dirtyRoleIds = this.dirtyRoleIds();
    if (dirtyRoleIds.length === 0) return;

    this.saving.set(true);
    this.saveError.set(null);
    this.saveSuccess.set(false);

    const edits = this.edits();
    const updates = dirtyRoleIds.map((roleId) =>
      this.roleService.updateRolePermissions(roleId, { permissionKeys: [...(edits[roleId] ?? [])] })
    );

    forkJoin(updates).subscribe({
      next: (responses) => {
        this.saving.set(false);
        const last = responses[responses.length - 1];
        if (last.success && last.data) {
          this.applyMatrix(last.data);
          this.saveSuccess.set(true);
        } else {
          this.saveError.set(last.message || 'Could not save one or more roles. Reloading current state.');
          this.load();
        }
      },
      error: (error) => {
        this.saving.set(false);
        this.saveError.set(error?.error?.message ?? 'Could not save one or more roles. Reloading current state.');
        this.load();
      }
    });
  }

  discardChanges(): void {
    if (!this.matrix()) return;
    this.applyMatrix(this.matrix()!);
    this.saveError.set(null);
    this.saveSuccess.set(false);
  }
}

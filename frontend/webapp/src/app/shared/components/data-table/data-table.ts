import { NgTemplateOutlet } from '@angular/common';
import { AfterContentInit, Component, ContentChild, TemplateRef, input } from '@angular/core';

export interface DataTableColumn {
  key: string;
  label: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [NgTemplateOutlet],
  templateUrl: './data-table.html',
  styleUrl: './data-table.scss'
})
export class DataTable<T> implements AfterContentInit {
  readonly columns = input.required<DataTableColumn[]>();
  readonly rows = input.required<T[]>();
  readonly loading = input(false);
  readonly emptyMessage = input('No records found.');

  @ContentChild('rowTemplate') rowTemplate?: TemplateRef<{ $implicit: T }>;

  ngAfterContentInit(): void {
    if (!this.rowTemplate) {
      throw new Error('DataTable requires a <ng-template #rowTemplate let-row> for row content.');
    }
  }
}

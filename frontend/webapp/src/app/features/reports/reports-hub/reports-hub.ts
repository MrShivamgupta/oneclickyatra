import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { REPORT_DEFINITIONS, ReportDefinition } from '../../../core/models/report.models';

@Component({
  selector: 'app-reports-hub',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './reports-hub.html',
  styleUrl: './reports-hub.scss'
})
export class ReportsHub {
  readonly reports: ReportDefinition[] = REPORT_DEFINITIONS;
}

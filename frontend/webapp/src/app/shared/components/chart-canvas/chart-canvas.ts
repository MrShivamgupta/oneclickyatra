import { AfterViewInit, Component, ElementRef, OnChanges, OnDestroy, SimpleChanges, ViewChild, input } from '@angular/core';
import { Chart, ChartConfiguration, ChartData, ChartOptions, ChartType } from 'chart.js/auto';

@Component({
  selector: 'app-chart-canvas',
  standalone: true,
  template: `<div class="chart-wrapper"><canvas #canvas></canvas></div>`,
  styles: [
    `
      .chart-wrapper {
        position: relative;
        width: 100%;
        height: 260px;
      }
      canvas {
        width: 100% !important;
        height: 100% !important;
      }
    `
  ]
})
export class ChartCanvas implements AfterViewInit, OnChanges, OnDestroy {
  readonly type = input.required<ChartType>();
  readonly data = input.required<ChartData>();
  readonly options = input<ChartOptions>({});

  @ViewChild('canvas') private readonly canvasRef!: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;
  private viewReady = false;

  ngAfterViewInit(): void {
    this.viewReady = true;
    this.render();
  }

  ngOnChanges(_changes: SimpleChanges): void {
    if (this.viewReady) {
      this.render();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private render(): void {
    this.chart?.destroy();
    const config: ChartConfiguration = {
      type: this.type(),
      data: this.data(),
      options: { responsive: true, maintainAspectRatio: false, ...this.options() }
    };
    this.chart = new Chart(this.canvasRef.nativeElement, config);
  }
}

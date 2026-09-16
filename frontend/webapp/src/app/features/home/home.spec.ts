import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Home } from './home';
import { DestinationService } from '../../core/services/destination.service';
import { PackageService } from '../../core/services/package.service';

describe('Home', () => {
  let fixture: ComponentFixture<Home>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideRouter([]),
        {
          provide: DestinationService,
          useValue: {
            search: () => of({ success: true, data: { items: [], totalCount: 0 } })
          }
        },
        {
          provide: PackageService,
          useValue: {
            search: () => of({ success: true, data: { items: [], totalCount: 0 } })
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should navigate to packages with search params when destination is provided', () => {
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.componentInstance.destination = 'Goa';
    fixture.componentInstance.travelDate = '2026-12-01';
    fixture.componentInstance.travelers = 2;

    fixture.componentInstance.search();

    expect(navigateSpy).toHaveBeenCalledWith(['/packages'], {
      queryParams: {
        searchTerm: 'Goa',
        travelDate: '2026-12-01',
        travelers: 2
      }
    });
  });

  it('should navigate to enquiry when only travel details are provided', () => {
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.componentInstance.destination = '';
    fixture.componentInstance.travelDate = '2026-12-01';
    fixture.componentInstance.travelers = 3;

    fixture.componentInstance.search();

    expect(navigateSpy).toHaveBeenCalledWith(['/enquiry'], {
      queryParams: {
        travelDate: '2026-12-01',
        travelers: 3
      }
    });
  });
});

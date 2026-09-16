import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { vi } from 'vitest';
import { PackageSearch } from './package-search';
import { PackageService } from '../../core/services/package.service';
import { DestinationService } from '../../core/services/destination.service';

describe('PackageSearch', () => {
  let fixture: ComponentFixture<PackageSearch>;
  let router: Router;
  const queryParams$ = new BehaviorSubject(convertToParamMap({ searchTerm: 'beach' }));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PackageSearch],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            queryParamMap: queryParams$.asObservable(),
            snapshot: { queryParamMap: convertToParamMap({ searchTerm: 'beach' }) }
          }
        },
        {
          provide: PackageService,
          useValue: {
            search: () => of({ success: true, data: { items: [], totalCount: 0 } })
          }
        },
        {
          provide: DestinationService,
          useValue: {
            search: () => of({ success: true, data: { items: [], totalCount: 0 } })
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PackageSearch);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should read searchTerm from query params', () => {
    expect(fixture.componentInstance.searchTerm()).toBe('beach');
  });

  it('should sync url when destination filter changes', () => {
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.componentInstance.onDestinationFilterChange('dest-123');

    expect(navigateSpy).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: {
          searchTerm: 'beach',
          destinationId: 'dest-123'
        }
      })
    );
  });
});

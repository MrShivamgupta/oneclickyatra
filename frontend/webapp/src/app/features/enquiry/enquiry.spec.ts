import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { Enquiry } from './enquiry';
import { DestinationService } from '../../core/services/destination.service';
import { EnquiryService } from '../../core/services/enquiry.service';

describe('Enquiry', () => {
  let fixture: ComponentFixture<Enquiry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Enquiry],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: {
                get: (key: string) => {
                  if (key === 'destinationId') return 'dest-1';
                  if (key === 'travelDate') return '2026-12-01';
                  if (key === 'packageTitle') return 'Golden Triangle';
                  if (key === 'travelers') return '4';
                  return null;
                }
              }
            }
          }
        },
        {
          provide: DestinationService,
          useValue: {
            search: () => of({ success: true, data: { items: [], totalCount: 0 } })
          }
        },
        {
          provide: EnquiryService,
          useValue: {
            submit: () => of({ success: true, data: null })
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Enquiry);
    fixture.detectChanges();
  });

  it('should prefill enquiry form from query params', () => {
    const form = fixture.componentInstance.form;
    expect(form.controls.destinationId.value).toBe('dest-1');
    expect(form.controls.travelDate.value).toBe('2026-12-01');
    expect(form.controls.message.value).toContain('Golden Triangle');
    expect(form.controls.message.value).toContain('4 traveler(s)');
  });

  it('should require core fields before submit', () => {
    fixture.componentInstance.form.setValue({
      fullName: '',
      email: '',
      phone: '',
      destinationId: '',
      travelDate: '',
      message: ''
    });

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.form.invalid).toBeTruthy();
    expect(fixture.componentInstance.submitted()).toBeFalsy();
  });
});

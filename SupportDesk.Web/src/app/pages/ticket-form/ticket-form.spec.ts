import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../api-config';
import { TicketPriority } from '../../models/ticket.model';
import { TicketForm } from './ticket-form';

describe('TicketForm', () => {
  let component: TicketForm;
  let fixture: ComponentFixture<TicketForm>;
  let element: HTMLElement;
  let http: HttpTestingController;

  const ticketsUrl = `${API_BASE_URL}/tickets`;

  function fill(selector: string, value: string) {
    const control = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(selector)!;
    control.value = value;
    control.dispatchEvent(new Event('input'));
  }

  function submit() {
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketForm],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TicketForm);
    component = fixture.componentInstance;
    element = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not submit an empty form and shows the required errors', () => {
    submit();

    http.expectNone(ticketsUrl);
    expect(element.textContent).toContain('Title is required.');
    expect(element.textContent).toContain('Customer email is required.');
  });

  it('posts the entered values when the form is valid', () => {
    fill('#title', 'Printer offline');
    fill('#description', 'The office printer does not respond.');
    fill('#customerName', 'Jane Doe');
    fill('#customerEmail', 'jane@example.com');

    submit();

    const request = http.expectOne(ticketsUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      title: 'Printer offline',
      description: 'The office printer does not respond.',
      customerName: 'Jane Doe',
      customerEmail: 'jane@example.com',
      priority: TicketPriority.Normal,
    });
  });
});

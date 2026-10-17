import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TicketDetail } from './ticket-detail';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

describe('TicketDetail', () => {
  let component: TicketDetail;
  let fixture: ComponentFixture<TicketDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketDetail],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    fixture = TestBed.createComponent(TicketDetail);
    fixture.componentRef.setInput('id', 1);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

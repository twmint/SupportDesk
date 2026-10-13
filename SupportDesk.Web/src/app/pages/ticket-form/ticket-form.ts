import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, map } from 'rxjs';

import { TicketPriority, TicketStatus } from '../../models/ticket.model';
import { TicketService } from '../../services/ticket';
import { getErrorMessages } from '../../shared/http-error';

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-ticket-form',
  styleUrl: './ticket-form.scss',
  templateUrl: './ticket-form.html',
})
export class TicketForm implements OnInit {
  private ticketService = inject(TicketService);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);

  protected id = input(undefined, {
    transform: (v: string | undefined) => (v ? Number(v) : undefined),
  });

  protected isEdit = computed(() => this.id() !== undefined);

  protected loading = signal(false);
  protected saving = signal(false);
  protected isClosed = signal(false);
  protected serverError = signal<string[]>([]);

  protected TicketPriority = TicketPriority;
  protected priorityOptions = Object.values(TicketPriority)
    .filter((v): v is TicketPriority => typeof v === 'number');

  protected ticketForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    customerEmail: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    priority: [TicketPriority.Normal, Validators.required],
  });

  ngOnInit() {
    const id = this.id();
    if (!id) return;

    this.loading.set(true);
    this.ticketService.getTicket(id).subscribe({
      next: (t) => {
        this.ticketForm.patchValue(t);
        if (t.status === TicketStatus.Closed) {
          this.isClosed.set(true);
          this.ticketForm.disable();
        }
        this.loading.set(false);
      },
      error: () => this.router.navigate(['/tickets']),
    });
  }

  protected save() {
    this.serverError.set([]);
    this.ticketForm.markAllAsTouched();
    if (this.ticketForm.invalid) return;

    this.saving.set(true);
    const dto = this.ticketForm.getRawValue();
    const id = this.id();

    const save$: Observable<number> = id
      ? this.ticketService.updateTicket(id, dto).pipe(map(() => id))
      : this.ticketService.createTicket(dto).pipe(map((t) => t.id));

    save$.subscribe({
      next: (ticketId) => {
        this.saving.set(false);
        this.router.navigate(['/tickets', ticketId]);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(getErrorMessages(err));
      },
    });
  }
}

import { Component, OnInit, computed, inject, input, numberAttribute, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { Observable, finalize, switchMap } from 'rxjs';

import { Agent } from '../../models/agent.model';
import { TICKET_STATUS_LABELS, Ticket, TicketPriority, TicketStatus } from '../../models/ticket.model';
import { AgentService } from '../../services/agent';
import { TicketService } from '../../services/ticket';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';
import { getErrorMessages } from '../../shared/http-error';
import { formatPageTitle } from '../../page-title-strategy';

// BR-2
const ALLOWED_TRANSITIONS: Record<TicketStatus, TicketStatus[]> = {
  [TicketStatus.New]: [TicketStatus.InProgress],
  [TicketStatus.InProgress]: [TicketStatus.Resolved],
  [TicketStatus.Resolved]: [TicketStatus.Closed, TicketStatus.InProgress],
  [TicketStatus.Closed]: [],
};

@Component({
  imports: [ReactiveFormsModule, RouterLink, DatePipe, ConfirmDialog],
  selector: 'app-ticket-detail',
  styleUrl: './ticket-detail.scss',
  templateUrl: './ticket-detail.html',
})
export class TicketDetail implements OnInit {
  private ticketService = inject(TicketService);
  private agentService = inject(AgentService);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private title = inject(Title);

  protected id = input.required({ transform: numberAttribute });

  protected ticket = signal<Ticket | null>(null);
  protected agents = signal<Agent[]>([]);

  protected loading = signal(true);
  protected loadError = signal<string | null>(null);
  protected actionError = signal<string | null>(null);
  protected busy = signal(false);
  protected showDeleteConfirm = signal(false);

  protected isClosed = computed(() => this.ticket()?.status === TicketStatus.Closed);
  protected nextStatuses = computed(() => {
    const t = this.ticket();
    return t ? ALLOWED_TRANSITIONS[t.status] : [];
  });
  protected activeAgents = computed(() => this.agents().filter((a) => a.active));
  protected assignedAgent = computed(() => {
    const agentId = this.ticket()?.agentId;
    return this.agents().find((a) => a.id === agentId) ?? null;
  });

  protected STATUS_LABELS = TICKET_STATUS_LABELS;
  protected TicketPriority = TicketPriority;

  protected agentControl = new FormControl<number | null>(null);
  protected commentForm = this.formBuilder.nonNullable.group({
    authorName: ['', [Validators.required, Validators.maxLength(200)]],
    body: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  ngOnInit() {
    this.agentService.getAgents().subscribe({
      next: (agents) => this.agents.set(agents),
      error: () => this.agents.set([]),
    });

    this.loadTicket();
  }

  private loadTicket() {
    this.loading.set(true);
    this.ticketService.getTicket(this.id())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (t) => this.applyTicket(t),
        error: () => this.loadError.set('Ticket not found.'),
      });
  }

  protected changeStatus(newStatus: TicketStatus) {
    this.runAction(this.ticketService.changeStatus(this.id(), { newStatus }));
  }

  protected assignAgent() {
    this.runAction(this.ticketService.assignAgent(this.id(), { agentId: this.agentControl.value }));
  }

  protected addComment() {
    this.commentForm.markAllAsTouched();
    if (this.commentForm.invalid) return;

    this.busy.set(true);
    this.actionError.set(null);
    this.ticketService.addComment(this.id(), this.commentForm.getRawValue())
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (comment) => {
          this.ticket.update((t) => t && { ...t, comments: [...t.comments, comment] });
          this.commentForm.reset();
        },
        error: (err: HttpErrorResponse) => this.actionError.set(this.toMessage(err)),
      });
  }

  protected deleteTicket() {
    this.busy.set(true);
    this.actionError.set(null);
    this.ticketService.deleteTicket(this.id()).subscribe({
      next: () => this.router.navigate(['/tickets']),
      error: (err: HttpErrorResponse) => {
        this.actionError.set(this.toMessage(err));
        this.showDeleteConfirm.set(false);
        this.busy.set(false);
      },
    });
  }

  private runAction(action$: Observable<void>) {
    this.busy.set(true);
    this.actionError.set(null);
    action$
      .pipe(
        switchMap(() => this.ticketService.getTicket(this.id())),
        finalize(() => this.busy.set(false)),
      )
      .subscribe({
        next: (t) => this.applyTicket(t),
        error: (err: HttpErrorResponse) => {
          this.actionError.set(this.toMessage(err));
          this.agentControl.setValue(this.ticket()?.agentId ?? null);
        },
      });
  }

  private applyTicket(t: Ticket) {
    this.ticket.set(t);
    this.title.setTitle(formatPageTitle(t.reference));
    this.agentControl.setValue(t.agentId);
    if (t.status === TicketStatus.Closed) {
      this.agentControl.disable();
      this.commentForm.disable();
    }
  }

  private toMessage(err: HttpErrorResponse): string {
    return getErrorMessages(err).join(' ');
  }
}

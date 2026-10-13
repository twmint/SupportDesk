import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { Agent } from '../../models/agent.model';
import { TICKET_STATUS_LABELS, Ticket, TicketPriority, TicketStatus } from '../../models/ticket.model';
import { AgentService } from '../../services/agent';
import { TicketService } from '../../services/ticket';

@Component({
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  selector: 'app-ticket-list',
  styleUrl: './ticket-list.scss',
  templateUrl: './ticket-list.html',
})
export class TicketList implements OnInit {
  private ticketService = inject(TicketService);
  private agentService = inject(AgentService);
  private formBuilder = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);

  protected tickets = signal<Ticket[]>([]);
  protected totalCount = signal(0);
  protected agents = signal<Agent[]>([]);

  protected loading = signal(false);
  protected error = signal<string | null>(null);

  protected page = signal(1);
  protected readonly pageSize = 10;
  protected totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  protected agentNames = computed(() => new Map(this.agents().map((a) => [a.id, a.fullName])));

  protected STATUS_LABELS = TICKET_STATUS_LABELS;
  protected TicketPriority = TicketPriority;
  protected statusOptions = Object.values(TicketStatus)
    .filter((v): v is TicketStatus => typeof v === 'number');
  protected priorityOptions = Object.values(TicketPriority)
    .filter((v): v is TicketPriority => typeof v === 'number');

  protected filters = this.formBuilder.nonNullable.group({
    search: '',
    status: null as TicketStatus | null,
    priority: null as TicketPriority | null,
    agentId: null as number | null,
    overdueOnly: false,
  });

  ngOnInit() {
    this.agentService.getAgents().subscribe({
      next: (agents) => this.agents.set(agents),
    });

    this.filters.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => {
      this.page.set(1);
      this.loadTickets();
    });

    this.loadTickets();
  }

  protected loadTickets() {
    this.loading.set(true);
    this.error.set(null);

    const f = this.filters.getRawValue();
    this.ticketService.getTickets({
      search: f.search || undefined,
      status: f.status ?? undefined,
      priority: f.priority ?? undefined,
      agentId: f.agentId ?? undefined,
      overdueOnly: f.overdueOnly || undefined,
      page: this.page(),
      pageSize: this.pageSize,
    }).subscribe({
      next: (result) => {
        this.tickets.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tickets.');
        this.loading.set(false);
      },
    });
  }

  protected goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.page.set(page);
    this.loadTickets();
  }

  protected clearFilters() {
    this.filters.reset();
  }
}

import { Routes } from '@angular/router';

import { TicketDetail } from './pages/ticket-detail/ticket-detail';
import { TicketForm } from './pages/ticket-form/ticket-form';
import { TicketList } from './pages/ticket-list/ticket-list';

export const routes: Routes = [
  { path: '', redirectTo: 'tickets', pathMatch: 'full' },
  {
    path: 'tickets',
    children: [
      { path: '', component: TicketList, title: 'Tickets' },
      { path: 'new', component: TicketForm, title: 'New ticket' },
      { path: ':id', component: TicketDetail, title: 'Ticket' },
      { path: ':id/edit', component: TicketForm, title: 'Edit ticket' },
    ],
  },
  { path: '**', redirectTo: 'tickets' },
];

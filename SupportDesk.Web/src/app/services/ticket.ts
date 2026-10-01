import { Service, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { PagedResult } from '../models/paged-result.model';
import { Comment, CommentCreateRequest } from '../models/comment.model';
import {
  AssignAgentRequest,
  ChangeStatusRequest,
  Ticket,
  TicketCreateRequest,
  TicketFilters,
  TicketUpdateRequest,
} from '../models/ticket.model';

@Service()
export class TicketService {
  private http = inject(HttpClient);
  private baseUrl = `${API_BASE_URL}/tickets`;

  getTickets(filters: TicketFilters): Observable<PagedResult<Ticket>> {
    let params = new HttpParams();
    if (filters.search) params = params.set('search', filters.search);
    if (filters.status !== undefined) params = params.set('status', filters.status);
    if (filters.priority !== undefined) params = params.set('priority', filters.priority);
    if (filters.agentId !== undefined) params = params.set('agentId', filters.agentId);
    if (filters.overdueOnly !== undefined) params = params.set('overdueOnly', filters.overdueOnly);
    if (filters.page !== undefined) params = params.set('page', filters.page);
    if (filters.pageSize !== undefined) params = params.set('pageSize', filters.pageSize);

    return this.http.get<PagedResult<Ticket>>(this.baseUrl, { params });
  }

  getTicket(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.baseUrl}/${id}`);
  }

  createTicket(dto: TicketCreateRequest): Observable<Ticket> {
    return this.http.post<Ticket>(this.baseUrl, dto);
  }

  updateTicket(id: number, dto: TicketUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  changeStatus(id: number, dto: ChangeStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, dto);
  }

  assignAgent(id: number, dto: AssignAgentRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/assign`, dto);
  }

  addComment(id: number, dto: CommentCreateRequest): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/${id}/comments`, dto);
  }
}

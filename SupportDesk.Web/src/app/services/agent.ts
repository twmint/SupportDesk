import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { Agent, AgentCreateRequest, AgentUpdateRequest } from '../models/agent.model';

@Service()
export class AgentService {
  private http = inject(HttpClient);
  private baseUrl = `${API_BASE_URL}/agents`;

  getAgents(search?: string): Observable<Agent[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);

    return this.http.get<Agent[]>(this.baseUrl, { params });
  }

  getAgent(id: number): Observable<Agent> {
    return this.http.get<Agent>(`${this.baseUrl}/${id}`);
  }

  createAgent(dto: AgentCreateRequest): Observable<Agent> {
    return this.http.post<Agent>(this.baseUrl, dto);
  }

  updateAgent(id: number, dto: AgentUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  /** Soft delete — resignation workflow (blocks if agent has InProgress tickets). */
  deactivateAgent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /** Hard delete — true row removal, unassigns tickets automatically, no blocking. */
  deleteAgentPermanently(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/hard`);
  }
}

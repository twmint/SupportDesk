import { Comment } from './comment.model';

export enum TicketPriority {
  Critical = 0,
  High = 1,
  Normal = 2,
  Low = 3,
}

export enum TicketStatus {
  New = 0,
  InProgress = 1,
  Resolved = 2,
  Closed = 3,
}

export interface Ticket {
  id: number;
  reference: string;
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
  status: TicketStatus;
  comments: Comment[];
  agentId: number | null;
  createdAt: string;
  lastModifiedAt: string | null;
  resolvedAt: string | null;
  closedAt: string | null;
  dueDate: string;
  isOverdue: boolean;
}

export interface TicketCreateRequest {
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
}

export interface TicketUpdateRequest {
  title?: string;
  description?: string;
  customerName?: string;
  customerEmail?: string;
  priority?: TicketPriority;
}

export interface ChangeStatusRequest {
  newStatus: TicketStatus;
}

export interface AssignAgentRequest {
  agentId: number | null;
}

export interface TicketFilters {
  search?: string;
  status?: TicketStatus;
  priority?: TicketPriority;
  agentId?: number;
  overdueOnly?: boolean;
  page?: number;
  pageSize?: number;
}

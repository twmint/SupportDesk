export enum Department {
  Technical = 0,
  Billing = 1,
  General = 2,
}

export interface Agent {
  id: number;
  fullName: string;
  email: string;
  department: Department;
  active: boolean;
}

export interface AgentCreateRequest {
  fullName: string;
  email: string;
  department: Department;
}

export interface AgentUpdateRequest {
  fullName?: string;
  email?: string;
  department?: Department;
  active?: boolean;
}

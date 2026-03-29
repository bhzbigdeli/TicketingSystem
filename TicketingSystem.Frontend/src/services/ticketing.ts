import { request } from '@umijs/max';

export type TicketStatus = 1 | 2 | 3 | 4;
export type TicketPriority = 1 | 2 | 3 | 4;

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
}

export interface TicketDto {
  id: string;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  createdById: string;
  assignedToId?: string;
  createdAt: string;
  updatedAt?: string;
}

export async function getUsers() {
  return request<UserDto[]>('/api/users', { method: 'GET' });
}

export async function getTickets(params: { page?: number; pageSize?: number }) {
  return request<TicketDto[]>('/api/tickets', {
    method: 'GET',
    params,
  });
}

export async function createTicket(data: {
  title: string;
  description: string;
  priority: TicketPriority;
  createdById: string;
}) {
  return request<TicketDto>('/api/tickets', {
    method: 'POST',
    data,
  });
}

export async function updateTicket(
  id: string,
  data: { title: string; description: string; priority: TicketPriority },
) {
  return request<TicketDto>(`/api/tickets/${id}`, {
    method: 'PUT',
    data,
  });
}

export async function changeTicketStatus(id: string, status: TicketStatus) {
  return request<TicketDto>(`/api/tickets/${id}/status`, {
    method: 'PATCH',
    data: { status },
  });
}

export async function deleteTicket(id: string) {
  return request<void>(`/api/tickets/${id}`, {
    method: 'DELETE',
  });
}

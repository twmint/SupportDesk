import { HttpErrorResponse } from '@angular/common/http';

export function getErrorMessages(err: HttpErrorResponse): string[] {
  const body = err.error;
  if (body?.errors) {
    return Object.values(body.errors).flat() as string[];
  }
  return [body?.detail ?? body?.title ?? 'An unexpected error occurred.'];
}

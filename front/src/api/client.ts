export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5207';

export interface ApiError {
  error: string;
  status?: number;
}

export async function apiClient<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem('token');
  const headers = new Headers(options.headers || {});

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint}`;

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let errorMessage = 'Ocurrió un error inesperado al procesar la solicitud.';
    try {
      const data = await response.json();
      if (data?.error) {
        errorMessage = data.error;
      } else if (typeof data === 'string') {
        errorMessage = data;
      } else if (data?.errors) {
        errorMessage = Object.values(data.errors).flat().join(', ');
      }
    } catch {
      errorMessage = response.statusText || errorMessage;
    }

    const error: ApiError = {
      error: errorMessage,
      status: response.status,
    };
    throw error;
  }

  // Handle empty responses (like 204 No Content)
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

import { z } from 'zod';

const htmlDecoder = new DOMParser();
const apiBaseUrl = '/api';

export function decodeHtml(value: string): string {
  const document = htmlDecoder.parseFromString(value, 'text/html');
  return document.documentElement.textContent ?? value;
}

function buildUrl(
  path: string,
  params?: Record<string, string | number | undefined>,
): string {
  const url = new URL(`${apiBaseUrl}${path}`, window.location.origin);

  if (params) {
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined) {
        url.searchParams.set(key, String(value));
      }
    }
  }

  return url.toString();
}

export async function fetchJson<T>(
  path: string,
  schema: z.ZodType<T>,
  init?: RequestInit,
  params?: Record<string, string | number | undefined>,
): Promise<T> {
  const response = await fetch(buildUrl(path, params), init);

  if (!response.ok) {
    let message = `Request failed with ${response.status}`;

    try {
      const problem = (await response.json()) as { title?: string };
      if (problem?.title) {
        message = problem.title;
      }
    } catch {
      // If parsing fails we fallback to the generic message
    }

    throw new Error(message);
  }

  const payload = (await response.json()) as unknown;

  return schema.parse(payload);
}

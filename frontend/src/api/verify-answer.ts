import { z } from 'zod';
import { fetchJson } from './client';

const verifyAnswerResponseSchema = z.object({
  isCorrect: z.boolean(),
});

export async function validateAnswer(
  questionId: number,
  selectedOptionIndex: number,
): Promise<boolean> {
  const response = await fetchJson('/questions/verify', verifyAnswerResponseSchema, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ questionId, selectedOptionIndex }),
  });

  return response.isCorrect;
}

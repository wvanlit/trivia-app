import useSWR from 'swr';
import { z } from 'zod';
import { decodeHtml, fetchJson } from './client';

const questionSchema = z
  .object({
    questionId: z.number(),
    categoryId: z.number(),
    text: z.string(),
    options: z.array(z.string()),
    difficulty: z.string(),
  })
  .transform((question) => ({
    ...question,
    text: decodeHtml(question.text),
    options: question.options.map((option) => decodeHtml(option)),
  }));

const randomQuestionsResponseSchema = z.object({
  questions: z.array(questionSchema),
});

export type Question = z.infer<typeof questionSchema>;

export interface QuestionsParams {
  count?: number
  categoryId?: number
  cacheKey?: number | string
}

export function useQuestions(
  params?: QuestionsParams,
): {
    questions: Question[]
    isLoading: boolean
    error: Error | null
    refetch: () => void
  } {
  const key = params
    ? ([
      'questions',
      params.count ?? null,
      params.categoryId ?? null,
      params.cacheKey ?? null,
    ] as const)
    : null;
  const { data, error, isLoading, mutate } = useSWR<Question[], Error>(
    key,
    key
      ? async () => {
        const response = await fetchJson(
          '/questions',
          randomQuestionsResponseSchema,
          undefined,
          {
            count: params?.count,
            categoryId: params?.categoryId,
          },
        );

        return response.questions;
      }
      : null,
    { keepPreviousData: true },
  );

  return {
    questions: data ?? [],
    isLoading,
    error: error instanceof Error ? error : null,
    refetch: () => {
      if (key) {
        void mutate();
      }
    },
  };
}

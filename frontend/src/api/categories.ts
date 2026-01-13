import useSWR from 'swr';
import { z } from 'zod';
import { decodeHtml, fetchJson } from './client';

const categorySchema = z
  .object({
    categoryId: z.number(),
    name: z.string(),
  })
  .transform((category) => ({
    ...category,
    name: decodeHtml(category.name),
  }));

const categoriesResponseSchema = z.object({
  categories: z.array(categorySchema),
});

export type Category = z.infer<typeof categorySchema>;

export function useCategories(): {
  categories: Category[]
  isLoading: boolean
  error: Error | null
  refetch: () => void
} {
  const { data, error, isLoading, mutate } = useSWR<Category[], Error>(
    'categories',
    async () => {
      const response = await fetchJson('/categories', categoriesResponseSchema);
      return response.categories;
    },
  );

  return {
    categories: data ?? [],
    isLoading,
    error: error instanceof Error ? error : null,
    refetch: () => {
      void mutate();
    },
  };
}

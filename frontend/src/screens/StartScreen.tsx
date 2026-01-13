import {
  Box,
  Button,
  Group,
  Select,
  SimpleGrid,
  Stack,
  Text,
  Title,
} from '@mantine/core';
import { useCategories } from '../api/categories';
import { Card } from '../components/Card';
import { useQuizActions, useQuizState } from '../state/quiz-hooks';

const questionOptions = Array.from({ length: 10 }, (_, index) => {
  const value = String(index + 1);
  return { value, label: value };
});

export function StartScreen() {
  const { categories, isLoading, error } = useCategories();
  const categoryOptions = categories.map((category) => ({
    value: String(category.categoryId),
    label: category.name,
  }));
  const { questionCount, categoryId } = useQuizState();
  const { setQuestionCount, setCategory, startQuiz } = useQuizActions();

  return (
    <Stack gap="xl" data-testid="start-screen">
      <Card>
        <Stack gap="lg">
          <Stack gap={4}>
            <Title order={1} size="h1">
              Trivia Quiz
            </Title>
          </Stack>

          <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
            <Box
              style={{
                padding: '0.75rem 1rem',
              }}
            >
              <Stack gap="xs">
                <Text fw={700}>Questions</Text>
                <Select
                  data-testid="question-count-select"
                  data={questionOptions}
                  value={String(questionCount)}
                  onChange={(value) => {
                    if (value) {
                      setQuestionCount(Number(value));
                    }
                  }}
                />
                <Text size="sm" c="dimmed">
                  Min 1, max 10. Default is 5.
                </Text>
              </Stack>
            </Box>

            <Box
              style={{
                padding: '0.75rem 1rem',
              }}
            >
              <Stack gap="xs">
                <Text fw={700}>Category</Text>
                <Select
                  data-testid="category-select"
                  data={categoryOptions}
                  value={categoryId ? String(categoryId) : null}
                  onChange={(value) => {
                    const nextValue = value ? Number(value) : null;
                    const nextLabel = value
                      ? categoryOptions.find((option) => option.value === value)?.label
                      ?? 'All categories'
                      : 'All categories';
                    setCategory(nextValue, nextLabel);
                  }}
                  placeholder={
                    isLoading
                      ? 'Loading categories...'
                      : 'All categories'
                  }
                  nothingFoundMessage={
                    error ? 'Failed to load categories' : 'No categories found'
                  }
                  disabled={isLoading || Boolean(error)}
                  clearable
                />
                <Text size="sm" c="dimmed">
                  Leave empty to play across all categories.
                </Text>
              </Stack>
            </Box>
          </SimpleGrid>

          <Group justify="end" wrap="wrap">

            <Button
              type="button"
              variant="filled"
              size="lg"
              radius={0}
              styles={{
                root: {
                  justifyContent: 'flex-start',
                  borderColor: '#1c1010ff',
                },
              }}
              onClick={startQuiz}
              data-testid="start-quiz-button"
            >
              Start Quiz
            </Button>
          </Group>
        </Stack>
      </Card>
    </Stack>
  );
}

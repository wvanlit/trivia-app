import {
  Box,
  Button,
  Center,
  Group,
  RingProgress,
  SimpleGrid,
  Stack,
  Text,
} from '@mantine/core';
import { Card } from '../components/Card';
import { useQuizActions, useQuizState } from '../state/quiz-hooks';

export function ResultScreen() {
  const { correctCount, totalQuestions, categoryLabel } = useQuizState();
  const { restart } = useQuizActions();
  const percentage = totalQuestions > 0
    ? Math.round((correctCount / totalQuestions) * 100)
    : 0;
  const performanceColor = percentage >= 80 ? 'lime' : percentage >= 55 ? 'yellow' : 'orange';

  const statItems = [
    { label: 'Category', value: categoryLabel },
    { label: 'Total questions', value: totalQuestions },
    { label: 'Correct answers', value: correctCount },
    { label: 'Accuracy', value: `${percentage}%` },
  ];

  return (
    <Stack gap="lg" data-testid="result-screen">
      <Card>
        <Stack gap="lg">
          <Stack align="center">
            <RingProgress
              size={160}
              thickness={14}
              sections={[{ value: percentage, color: performanceColor }]}
              label={(
                <Center>
                  <Stack gap={0} align="center">
                    <Text size="xs" c="dimmed">
                      Score
                    </Text>
                    <Text fw={700} size="xl">
                      {percentage}
                      %
                    </Text>
                  </Stack>
                </Center>
              )}
            />
          </Stack>

          <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
            {statItems.map((item) => (
              <Box
                key={item.label}
                style={{
                  border: '1px solid #0d0d0d',
                  padding: '0.75rem 1rem',
                  backgroundColor: '#fff',
                }}
              >
                <Text size="sm" c="dimmed">
                  {item.label}
                </Text>
                <Text fw={700} size="lg">
                  {item.value}
                </Text>
              </Box>
            ))}
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
              onClick={restart}
              data-testid="play-again-button"
            >
              Play again
            </Button>
          </Group>
        </Stack>
      </Card>
    </Stack>
  );
}

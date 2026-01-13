import { Stack, Text, Title } from '@mantine/core';
import { Card } from './Card';

export function QuestionCard({
  question,
  helper,
  'data-testid': dataTestId,
}: {
  'question': string
  'helper'?: string
  'data-testid'?: string
}) {
  return (
    <Card data-testid={dataTestId}>
      <Stack gap="xs">
        {helper
          ? (
            <Text size="sm" c="dimmed">
              {helper}
            </Text>
          )
          : null}
        <Title order={2}>{question}</Title>
      </Stack>
    </Card>
  );
}

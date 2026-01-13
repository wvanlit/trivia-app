import { Group, Progress, Text } from '@mantine/core';

export function ProgressHeader({
  current,
  total,
  'data-testid': dataTestId,
}: {
  'current': number
  'total': number
  'data-testid'?: string
}) {
  const value = total > 0 ? Math.round((current / total) * 100) : 0;

  return (
    <div data-testid={dataTestId}>
      <Group justify="space-between" mb="xs">
        <Text fw={700}>
          Question
          {' '}
          {current}
          {' '}
          of
          {' '}
          {total}
        </Text>
        <Text size="sm" c="dimmed">
          {value}
          %
        </Text>
      </Group>
      <Progress value={value} size="lg" radius={0} />
    </div>
  );
}

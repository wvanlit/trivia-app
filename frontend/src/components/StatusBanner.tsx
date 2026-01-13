import { Group, Loader, Text } from '@mantine/core';
import { Card } from './Card';

const statusStyles = {
  success: {
    backgroundColor: 'var(--mantine-color-lime-2)',
  },
  failure: {
    backgroundColor: 'var(--mantine-color-red-2)',
  },
  error: {
    backgroundColor: 'var(--mantine-color-orange-2)',
  },
};

export function StatusBanner({
  tone,
  message,
  showSpinner = false,
  'data-testid': dataTestId,
}: {
  'tone': 'success' | 'failure' | 'error'
  'message': string
  'showSpinner'?: boolean
  'data-testid'?: string
}) {
  return (
    <Card
      data-testid={dataTestId}
      style={statusStyles[tone]}
    >
      <Group gap="sm">
        {showSpinner ? <Loader size="sm" color="black" /> : null}
        <Text fw={700}>{message}</Text>
      </Group>
    </Card>
  );
}

import { Button } from '@mantine/core';

export function AnswerOption({
  label,
  onClick,
  disabled,
  'data-testid': dataTestId,
}: {
  'label': string
  'onClick': () => void
  'disabled'?: boolean
  'data-testid'?: string
}) {
  return (
    <Button
      type="button"
      variant="filled"
      size="lg"
      radius={0}
      onClick={onClick}
      disabled={disabled}
      fullWidth
      data-testid={dataTestId}
      styles={{
        root: {
          justifyContent: 'flex-start',
          borderColor: '#0d0d0d',
        },
      }}
    >
      {label}
    </Button>
  );
}

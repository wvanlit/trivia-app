import { type ReactNode } from 'react';
import { Paper, type PaperProps } from '@mantine/core';

export function Card({
  children,
  ...props
}: PaperProps & { children?: ReactNode }) {
  return (
    <Paper
      withBorder
      radius={0}
      p="lg"
      shadow="xl"
      style={{ borderColor: '#0d0d0d' }}
      {...props}
    >
      {children}
    </Paper>
  );
}

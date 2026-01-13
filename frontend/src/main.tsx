import '@mantine/core/styles.css';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { MantineProvider } from '@mantine/core';
import App from './App.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider
      theme={{
        fontFamily: 'Space Grotesk, system-ui, sans-serif',
        headings: { fontFamily: 'Space Grotesk, system-ui, sans-serif' },
        primaryColor: 'blue',
        defaultRadius: 0,
        black: '#0d0d0d',
      }}
    >
      <App />
    </MantineProvider>
  </StrictMode>,
);

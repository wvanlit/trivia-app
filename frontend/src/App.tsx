import { Box, Container, Stack } from '@mantine/core';
import { QuizProvider } from './state/QuizProvider';
import { useQuizState } from './state/quiz-hooks';
import { StartScreen } from './screens/StartScreen';
import { QuizScreen } from './screens/QuizScreen';
import { ResultScreen } from './screens/ResultScreen';

function AppShell() {
  const { screen } = useQuizState();

  return (
    <Box
      style={{
        minHeight: '100vh',
        background: '#fff',
      }}
    >
      <Container size="md" py="xl">
        <Stack gap="xl">
          {screen === 'start' ? <StartScreen /> : null}
          {screen === 'quiz' ? <QuizScreen /> : null}
          {screen === 'result' ? <ResultScreen /> : null}
        </Stack>
      </Container>
    </Box>
  );
}

function App() {
  return (
    <QuizProvider>
      <AppShell />
    </QuizProvider>
  );
}

export default App;

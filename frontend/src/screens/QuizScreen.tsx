import {
  Center,
  LoadingOverlay,
  Stack,
  Text,
} from '@mantine/core';
import { AnswerOption } from '../components/AnswerOption';
import { Card } from '../components/Card';
import { ProgressHeader } from '../components/ProgressHeader';
import { QuestionCard } from '../components/QuestionCard';
import { StatusBanner } from '../components/StatusBanner';
import { useQuizActions, useQuizState } from '../state/quiz-hooks';

export function QuizScreen() {
  const {
    activeQuestion,
    currentIndex,
    totalQuestions,
    status,
    isLoading,
    errorMessage,
    isLocked,
  } = useQuizState();
  const { answer } = useQuizActions();
  const question = activeQuestion?.text ?? null;
  const options = activeQuestion?.options ?? [];

  return (
    <Stack gap="lg" data-testid="quiz-screen" pos="relative">
      <LoadingOverlay visible={isLoading} overlayProps={{ blur: 2 }} />

      <Card>
        <ProgressHeader
          current={Math.min(currentIndex + 1, totalQuestions)}
          total={totalQuestions}
          data-testid="progress-header"
        />
        <Stack gap="md" mt="md">
          {question
            ? (
              <Stack gap="md">
                <QuestionCard
                  question={question}
                  helper="Pick one answer. You can only choose once."
                  data-testid="question-card"
                />
                <Stack gap="sm" data-testid="answer-options">
                  {options.map((option, index) => (
                    <AnswerOption
                      key={`option-${index}`}
                      label={option}
                      disabled={isLocked}
                      onClick={() => {
                        void answer(index);
                      }}
                      data-testid={`answer-option-${index}`}
                    />
                  ))}
                </Stack>
              </Stack>
            )
            : (
              <Center>
                <Text size="lg" c="dimmed">
                  No questions available.
                </Text>
              </Center>
            )}
        </Stack>
      </Card>

      {errorMessage
        ? (
          <Center
            data-testid="status-banner"
            style={{
              position: 'absolute',
              inset: 0,
              pointerEvents: 'none',
              zIndex: 5,
            }}
          >
            <StatusBanner tone="error" message={errorMessage} />
          </Center>
        )
        : null}

      {status === 'correct'
        ? (
          <Center
            data-testid="status-banner"
            style={{
              position: 'absolute',
              inset: 0,
              pointerEvents: 'none',
              zIndex: 5,
            }}
          >
            <StatusBanner
              tone="success"
              message="Correct!"
            />
          </Center>
        )
        : null}

      {status === 'wrong'
        ? (
          <Center
            data-testid="status-banner"
            style={{
              position: 'absolute',
              inset: 0,
              pointerEvents: 'none',
              zIndex: 5,
            }}
          >
            <StatusBanner
              tone="failure"
              message="Wrong."
            />
          </Center>
        )
        : null}
    </Stack>
  );
}

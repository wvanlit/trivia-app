import { useContext } from 'react';
import { QuizActionsContext, QuizStateContext } from './quiz-context';

export function useQuizState() {
  const context = useContext(QuizStateContext);
  if (!context) {
    throw new Error('useQuizState must be used within a QuizProvider.');
  }
  return context;
}

export function useQuizActions() {
  const context = useContext(QuizActionsContext);
  if (!context) {
    throw new Error('useQuizActions must be used within a QuizProvider.');
  }
  return context;
}

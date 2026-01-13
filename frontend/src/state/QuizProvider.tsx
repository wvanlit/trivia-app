import { useEffect, useReducer, useRef } from 'react';
import type { ReactNode } from 'react';
import { useQuestions } from '../api/questions';
import { validateAnswer } from '../api/verify-answer';
import {
  QuizActionsContext,
  QuizStateContext,
  initialState,
  quizReducer,
} from './quiz-context';

const FEEDBACK_DELAY_MS = 500;

export function QuizProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(quizReducer, initialState);
  const advanceTimer = useRef<number | null>(null);

  const { questions, isLoading, error } = useQuestions(
    state.screen === 'quiz'
      ? {
        count: state.questionCount,
        categoryId: state.categoryId ?? undefined,
        cacheKey: state.quizId,
      }
      : undefined,
  );

  useEffect(() => {
    return () => {
      if (advanceTimer.current) {
        window.clearTimeout(advanceTimer.current);
      }
    };
  }, []);

  const activeQuestion = questions[state.currentIndex] ?? null;
  const totalQuestions = questions.length;
  const correctCount = state.score.filter(Boolean).length;
  const errorMessage = error ? error.message : state.answerError;
  const isFinalQuestion = state.currentIndex + 1 >= totalQuestions;
  const isLocked = state.status !== 'idle';

  const setQuestionCount = (value: number) => {
    dispatch({ type: 'setQuestionCount', value });
  };

  const setCategory = (categoryId: number | null, label: string) => {
    dispatch({ type: 'setCategory', categoryId, label });
  };

  const startQuiz = () => {
    if (advanceTimer.current) {
      window.clearTimeout(advanceTimer.current);
      advanceTimer.current = null;
    }
    dispatch({ type: 'startQuiz' });
  };

  const restart = () => {
    if (advanceTimer.current) {
      window.clearTimeout(advanceTimer.current);
      advanceTimer.current = null;
    }
    dispatch({ type: 'restart' });
  };

  const answer = async (index: number) => {
    if (!activeQuestion || state.status !== 'idle') {
      return;
    }

    if (advanceTimer.current) {
      window.clearTimeout(advanceTimer.current);
    }

    dispatch({ type: 'answerChecking' });

    try {
      const isCorrect = await validateAnswer(activeQuestion.questionId, index);
      dispatch({ type: 'answerResult', isCorrect });

      advanceTimer.current = window.setTimeout(() => {
        dispatch({ type: 'advanceQuestion', totalQuestions });
      }, FEEDBACK_DELAY_MS);
    } catch {
      dispatch({
        type: 'answerFailed',
        message: 'Could not verify the answer. Try again.',
      });
    }
  };

  const stateValue = {
    ...state,
    questions,
    activeQuestion,
    totalQuestions,
    correctCount,
    isFinalQuestion,
    isLocked,
    isLoading,
    errorMessage,
  };

  const actionsValue = {
    setQuestionCount,
    setCategory,
    startQuiz,
    restart,
    answer,
  };

  return (
    <QuizStateContext.Provider value={stateValue}>
      <QuizActionsContext.Provider value={actionsValue}>
        {children}
      </QuizActionsContext.Provider>
    </QuizStateContext.Provider>
  );
}

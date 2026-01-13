import { createContext } from 'react';
import type { Question } from '../api/questions';

type Screen = 'start' | 'quiz' | 'result';
type AnswerStatus = 'idle' | 'checking' | 'correct' | 'wrong';

export interface QuizState {
  screen: Screen
  questionCount: number
  categoryId: number | null
  categoryLabel: string
  quizId: number
  currentIndex: number
  score: boolean[]
  status: AnswerStatus
  answerError: string | null
}

export type QuizAction =
  | { type: 'setQuestionCount', value: number }
  | { type: 'setCategory', categoryId: number | null, label: string }
  | { type: 'startQuiz' }
  | { type: 'restart' }
  | { type: 'answerChecking' }
  | { type: 'answerResult', isCorrect: boolean }
  | { type: 'answerFailed', message: string }
  | { type: 'advanceQuestion', totalQuestions: number };

export type QuizStateContextValue = QuizState & {
  questions: Question[]
  activeQuestion: Question | null
  totalQuestions: number
  correctCount: number
  isFinalQuestion: boolean
  isLocked: boolean
  isLoading: boolean
  errorMessage: string | null
};

export interface QuizActions {
  setQuestionCount: (value: number) => void
  setCategory: (categoryId: number | null, label: string) => void
  startQuiz: () => void
  restart: () => void
  answer: (index: number) => Promise<void>
}

export const initialState: QuizState = {
  screen: 'start',
  questionCount: 5,
  categoryId: null,
  categoryLabel: 'All categories',
  quizId: 0,
  currentIndex: 0,
  score: [],
  status: 'idle',
  answerError: null,
};

export const QuizStateContext = createContext<QuizStateContextValue | null>(null);
export const QuizActionsContext = createContext<QuizActions | null>(null);

export function quizReducer(state: QuizState, action: QuizAction): QuizState {
  switch (action.type) {
    case 'setQuestionCount':
      return { ...state, questionCount: action.value };
    case 'setCategory':
      return {
        ...state,
        categoryId: action.categoryId,
        categoryLabel: action.label,
      };
    case 'startQuiz':
      return {
        ...state,
        screen: 'quiz',
        quizId: state.quizId + 1,
        currentIndex: 0,
        score: [],
        status: 'idle',
        answerError: null,
      };
    case 'restart':
      return { ...state, screen: 'start' };
    case 'answerChecking':
      return { ...state, status: 'checking', answerError: null };
    case 'answerResult':
      return {
        ...state,
        status: action.isCorrect ? 'correct' : 'wrong',
        score: [...state.score, action.isCorrect],
        answerError: null,
      };
    case 'answerFailed':
      return { ...state, status: 'idle', answerError: action.message };
    case 'advanceQuestion': {
      const nextIndex = state.currentIndex + 1;
      if (nextIndex >= action.totalQuestions) {
        return { ...state, screen: 'result', status: 'idle' };
      }
      return { ...state, currentIndex: nextIndex, status: 'idle' };
    }
    default:
      return state;
  }
}

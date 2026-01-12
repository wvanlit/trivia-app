CREATE TABLE category (
    category_id BIGINT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE questions (
    question_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    category_id BIGINT NOT NULL REFERENCES category(category_id),
    question TEXT NOT NULL,
    correct_answer TEXT NOT NULL,
    options JSONB NOT NULL,
    difficulty TEXT NOT NULL
);

CREATE INDEX idx_questions_category_id ON questions(category_id);
CREATE INDEX idx_questions_difficulty ON questions(difficulty);

# Open Trivia Database

[Documentation](https://opentdb.com/api_config.php)

API URL: https://opentdb.com/api.php

## Session Token

Session token can be used as a "cursor" when iterating through the requests.

Session Tokens will be deleted after 6 hours of inactivity.

URL to get a new token: `GET https://opentdb.com/api_token.php?command=request`

URL to reset a token: `GET https://opentdb.com/api_token.php?command=reset&token=$TOKEN`

## Get Trivia Questions

URL: `GET https://opentdb.com/api.php?$PARAMS`

Available Parameters:
- `amount`: integer – number of questions (max 50)
- `category`: integer – id of category to filter by
- `difficulty`: string – enum (easy, medium, hard) of question difficulty
- `type`: string – "boolean" (true/false) or "multiple" (multiple choice)
- `encode`: string – enum (base64, urlLegacy, url3986) or none for `default`
- `token`: string – Session token

### Response Codes

0) Success Returned results successfully.
1) No Results Could not return results. The API doesn't have enough questions for your query. (Ex. Asking for 50 Questions in a Category that only has 20.)
2) Invalid Parameter Contains an invalid parameter. Arguements passed in aren't valid. (Ex. Amount = Five)
3) Token Not Found Session Token does not exist.
4) Token Empty Session Token has returned all possible questions for the specified query. Resetting the Token is necessary.
5) Rate Limit Too many requests have occurred. Each IP can only access the API once every 5 seconds.

### Encoding

Example Sentence (Non Encoded):
`"Don't forget that π = 3.14 & doesn't equal 3."`

Default Encoding (HTML Codes):
`Don&‌#039;t forget that &‌pi; = 3.14 &‌amp; doesn&‌#039;t equal 3`.

Legacy URL Encoding:
`Don%27t+forget+that+%CF%80+%3D+3.14+%26+doesn%27t+equal+3`.

URL Encoding (RFC 3986):
`Don%27t%20forget%20that%20%CF%80%20%3D%203.14%20%26%20doesn%27t%20equal%203.`

Base64 Encoding:
`RG9uJ3QgZm9yZ2V0IHRoYXQgz4AgPSAzLjE0ICYgZG9lc24ndCBlcXVhbCAzLg==`

### Example Response

```json
{
  "response_code": 0,
  "results": [
    {
      "type": "multiple",
      "difficulty": "easy",
      "category": "Entertainment: Video Games",
      "question": "Which Game Boy from the Game Boy series of portable video game consoles was released first?",
      "correct_answer": "Game Boy Color",
      "incorrect_answers": [
        "Game Boy Advance",
        "Game Boy Micro",
        "Game Boy Advance SP"
      ]
    },
    {
      "type": "multiple",
      "difficulty": "medium",
      "category": "Science: Computers",
      "question": "Which of the following is a personal computer made by the Japanese company Fujitsu?",
      "correct_answer": "FM-7",
      "incorrect_answers": [
        "PC-9801",
        "Xmillennium ",
        "MSX"
      ]
    },
    {
      "type": "multiple",
      "difficulty": "medium",
      "category": "Entertainment: Video Games",
      "question": "Which of the following is NOT an official game in Nintendo&#039;s Super Smash Bros. series?",
      "correct_answer": "Super Smash Bros. Crusade",
      "incorrect_answers": [
        "Super Smash Bros. Melee",
        "Super Smash Bros. Brawl",
        "Super Smash Bros. for Nintendo 3DS and Wii U"
      ]
    }
  ]
}
```

## Get Categories

URL: `GET https://opentdb.com/api_category.php`

```json
{
  "trivia_categories": [
    {
      "id": 9,
      "name": "General Knowledge"
    },
    {
      "id": 10,
      "name": "Entertainment: Books"
    },
    {
      "id": 32,
      "name": "Entertainment: Cartoon & Animations"
    }
  ]
}
```

## Get Category Question Count

URL: `GET https://opentdb.com/api_count.php?category=$ID`

```json
{
  "category_id": 10,
  "category_question_count": {
    "total_question_count": 115,
    "total_easy_question_count": 37,
    "total_medium_question_count": 49,
    "total_hard_question_count": 29
  }
}
```

## Get Global Question Count

URL: `GET https://opentdb.com/api_count_global.php`

```json
{
  "overall": {
    "total_num_of_questions": 21302,
    "total_num_of_pending_questions": 11428,
    "total_num_of_verified_questions": 4738,
    "total_num_of_rejected_questions": 5152
  },
  "categories": {
    "9": {
      "total_num_of_questions": 5319,
      "total_num_of_pending_questions": 2909,
      "total_num_of_verified_questions": 401,
      "total_num_of_rejected_questions": 2009
    },
    "10": {
      "total_num_of_questions": 505,
      "total_num_of_pending_questions": 289,
      "total_num_of_verified_questions": 115,
      "total_num_of_rejected_questions": 101
    },
    "32": {
      "total_num_of_questions": 326,
      "total_num_of_pending_questions": 168,
      "total_num_of_verified_questions": 105,
      "total_num_of_rejected_questions": 53
    }
  }
}
```
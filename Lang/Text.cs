using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lang
{
    class Text
    {
        public readonly List<LexemeContainer> Lexemes;

        private int _position;
        public readonly int Length;

        public int Position => _position;

        public Text(List<LexemeContainer> lexemes)
        {
            Lexemes = lexemes;
            Length = lexemes.Count;
        }
        
        public bool CheckNext(params Lexeme[] expectedLexemes)
        {
            return CheckNext(1, expectedLexemes);
        }

        public bool CheckNext(int count, params Lexeme[] expectedLexemes)
        {
            var next = GetNextWord(count);
            if (next == null)
                return false;
            return expectedLexemes.Contains(next.Lexeme);
        }

        public LexemeContainer GetNextWordExpected(params Lexeme[] expectedLexemes)
        {
            return GetNextWordExpected(1, expectedLexemes);
        }

        public LexemeContainer GetNextWordExpected(int count, params Lexeme[] expectedLexemes)
        {
            var next = GetNextWord(count);
            if (next == null)
            {
                throw new ParseException(Position, expectedLexemes);
            }
            if (!expectedLexemes.Contains(next.Lexeme))
            {
                throw new ParseException(Position + count, next, expectedLexemes);
            }

            return next;
        }

        public LexemeContainer GetNextWord(int count = 1)
        {
            var watchPosition = _position + count;
            if (watchPosition < Length)
            {
                return Lexemes[watchPosition];
            }

            return null;
        }

        public LexemeContainer CurrentWord => Lexemes[_position];

        public bool MoveNext()
        {
            var watchPosition = _position + 1;
            if (watchPosition < Length)
            {
                _position = watchPosition;
                return true;
            }
            else
            {
                return false;
            }
        }

        public LexemeContainer MoveExpected(params Lexeme[] expectedLexemes)
        {
            if (!MoveNext())
            {
                throw new ParseException(Position, expectedLexemes);
            }

            if (!expectedLexemes.Contains(CurrentWord.Lexeme))
            {
                throw new ParseException(Position, CurrentWord, expectedLexemes);
            }

            return CurrentWord;
        }

        public override string ToString()
        {
            return $"{CurrentWord.Lexeme} {CurrentWord.Value}";
        }
    }
}

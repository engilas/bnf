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

        const string Int = @"^[0-7]+$";
        const string Var = @"^[A-z]+[0-7A-z]*$";
        const string Func = @"^(sin|cos|abs)([0-7]+|[A-z]+[A-z0-7]*)$";

        static readonly Regex IntRegex = new Regex(Int);
        static readonly Regex VarRegex = new Regex(Var);
        static readonly Regex FuncRegex = new Regex(Func);
        //static readonly R
        
        static bool IsVar(string word)
        {
            foreach (var key in LexemeHelper.LexemeTable.Values)
            {
                if (word.Equals(key))
                {
                    return false;
                }
            }
            return VarRegex.IsMatch(word) && !FuncRegex.IsMatch(word);
        }

        static Lexeme GetFuncType(string func)
        {
            var f = func.Substring(0, 3);
            if (f == "sin")
                return Lexeme.SIN;
            if (f == "cos")
                return Lexeme.COS;
            if (f == "abs")
                return Lexeme.ABS;
            throw new ParseException("Can't get func type of word " + func);
        }

        static LexemeContainer GetFuncContent(string func)
        {
            var content = func.Substring(3);
            if (IsVar(content))
            {
                return new LexemeContainer(Lexeme.VAR, content);
            }

            if (IntRegex.IsMatch(content))
            {
                return new LexemeContainer(Lexeme.INT, content);
            }

            throw new ParseException("Can't get func content of word " + func);
        }

        private Text(List<LexemeContainer> lexemes)
        {
            Lexemes = lexemes;
            Length = lexemes.Count;
        }

        public static Text Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Can't create text object: input is empty");

            input = input
                .Replace(",", " , ")
                .Replace(";", " ; ")
                .Replace(":", " : ")
                .Replace("=", " = ")
                .Replace("*", " * ")
                .Replace("/", " / ")
                .Replace("+", " + ")
                .Replace("-", " - ")
                .Replace("^", " ^ ");

            var lexemes = new List<LexemeContainer>();

            var words = input.Split(null).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var foundLexeme = false;

                foreach (var lexeme in LexemeHelper.LexemeTable)
                {
                    if (word.Equals(lexeme.Value))
                    {
                        lexemes.Add(new LexemeContainer(lexeme.Key, word));
                        foundLexeme = true;
                    }
                }

                if (foundLexeme) continue;

                if (FuncRegex.IsMatch(word))
                {
                    lexemes.Add(new LexemeContainer(GetFuncType(word), word) {FuncContent = GetFuncContent(word)});
                    continue;
                }

                if (IsVar(word))
                {
                    lexemes.Add(new LexemeContainer(Lexeme.VAR, word));
                    continue;
                }

                if (IntRegex.IsMatch(word))
                {
                    lexemes.Add(new LexemeContainer(Lexeme.INT, word));
                    continue;
                }

                throw new ParseException($"Неизвестное слово: {word}", i);
            }

            return new Text(lexemes);
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

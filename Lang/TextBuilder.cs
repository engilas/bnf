using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lang
{
    class TextBuilder
    {
        const string Int = @"^[0-7]+$";
        const string Var = @"^[A-z]+[0-7A-z]*$";
        const string Func = @"^(sin|cos|abs)([0-7]+|[A-z]+[A-z0-7]*)$";

        static readonly Regex IntRegex = new Regex(Int);
        static readonly Regex VarRegex = new Regex(Var);
        static readonly Regex FuncRegex = new Regex(Func);
        
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

                throw new ParseException($"Неизвестное слово: {word}. Разрешен латинский алфавит и восьмеричная арифметика.", i);
            }

            return new Text(lexemes);
        }
    }
}

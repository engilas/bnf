using System;
using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    public class ParseException : Exception
    {
        public string Error { get; set; }

        public ParseException(){}
        public ParseException(string message) => Error = message;

        public ParseException(string message, int position) => Error = $"позиция {position}: {message}";

        public ParseException(int position, params Lexeme[] expected)
        {
            Error = $"позиция {position}: Конец строки. Ожидалось: {string.Join(", ", expected.Select(x => _translate[x]).Distinct())}";
        }

        public ParseException(int position, LexemeContainer actual, params Lexeme[] expected)
        {
            Error = $"позиция {position}: Ожидалось: {string.Join(", ", expected.Select(x => _translate[x]).Distinct())}, получено: {_translate[actual.Lexeme]} ({actual.Value})";
        }

        private readonly Dictionary<Lexeme, string> _translate = new Dictionary<Lexeme, string>
        {
            {Lexeme.BEGIN, "Begin"},
            {Lexeme.END, "End"},
            {Lexeme.FIRST, "First"},
            {Lexeme.SECOND, "Second"},
            {Lexeme.SEMICOLON, "точка с запятой"},
            {Lexeme.COMMA, "запятая"},
            {Lexeme.EQUAL, "знак равно"},
            {Lexeme.COLON, "двоеточие"},

            {Lexeme.PLUS, "операция"},
            {Lexeme.MINUS, "операция"},
            {Lexeme.MULTIPLY, "операция"},
            {Lexeme.DIVIDE, "операция"},
            {Lexeme.POW, "операция"},

            {Lexeme.ABS, "функция"},
            {Lexeme.COS, "функция"},
            {Lexeme.SIN, "функция"},

            {Lexeme.VAR, "переменная"},
            {Lexeme.INT, "число"},
        };
    }
}

using System;
using System.Collections.Generic;

namespace Lang
{
    public enum Lexeme
    {
        //key words
        BEGIN,
        END,
        FIRST,
        SECOND,

        SEMICOLON,
        COMMA,
        EQUAL,
        COLON,

        INT,
        //internal type
        DOUBLE,
        VAR,

        SIN,
        COS,
        ABS,

        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        POW

    }

    public class LexemeContainer
    {
        public readonly Lexeme Lexeme;
        public readonly string Value;

        public LexemeContainer FuncContent;

        public LexemeContainer(Lexeme l, string v)
        {
            Lexeme = l;
            Value = v;
        }
    }

    static class LexemeHelper
    {
        public static readonly Dictionary<Lexeme, string> LexemeTable = new Dictionary<Lexeme, string>
        {
            {Lexeme.BEGIN, "Begin"},
            {Lexeme.END, "End"},
            {Lexeme.FIRST, "First"},
            {Lexeme.SECOND, "Second"},
            {Lexeme.SEMICOLON, ";"},
            {Lexeme.COMMA, ","},
            {Lexeme.EQUAL, "="},
            {Lexeme.COLON, ":"},
            {Lexeme.PLUS, "+"},
            {Lexeme.MINUS, "-"},
            {Lexeme.MULTIPLY, "*"},
            {Lexeme.DIVIDE, "/"},
            {Lexeme.POW, "^"},
        };
    }
}

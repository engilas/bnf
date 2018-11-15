using System.Collections.Generic;

namespace Lang
{
    class Parser
    {
        private readonly Dictionary<string, double> _labelValues = new Dictionary<string, double>();
        private readonly Dictionary<string, double> _varValues = new Dictionary<string, double>();

        public Dictionary<string, double> GetLabelValues() => _labelValues;
        public Dictionary<string, double> GetVarValues() => _varValues;

        public void ParseLang(Text input)
        {
            var word = input.CurrentWord;
            if (word.Lexeme != Lexeme.BEGIN) 
                throw new ParseException(input.Position, word, Lexeme.BEGIN);
            
            //условие выхода: не first, не second
            while (true)
            {
                ParseZveno(input);
                var next = input.GetNextWordExpected(Lexeme.FIRST, Lexeme.SECOND, Lexeme.VAR);
                if (next.Lexeme != Lexeme.FIRST && next.Lexeme != Lexeme.SECOND)
                    break;
            }

            ParseLast(input);

            while (true)
            {
                ParseOper(input);
                if (input.CheckNext(Lexeme.END))
                    break;
            }

            input.MoveExpected(Lexeme.END);
            if (input.GetNextWord() != null)
            {
                throw new ParseException("После 'End' не должно быть символов", input.Position);
            }
        }

        private void ParseZveno(Text input)
        {
            var lexeme = input.MoveExpected(Lexeme.FIRST, Lexeme.SECOND).Lexeme;

            if (lexeme == Lexeme.FIRST)
            {
                while (true)
                {
                    input.MoveExpected(Lexeme.INT);

                    if (!input.CheckNext(Lexeme.COMMA))
                    {
                        break;
                    }

                    input.MoveNext();
                }
            }
            else if (lexeme == Lexeme.SECOND)
            {
                while (true)
                {
                    input.MoveExpected(Lexeme.VAR);

                    //условие выхода: 1. след. не перем. 2. след + 1 не переменная

                    if (!input.CheckNext(Lexeme.VAR))
                        break;

                    if (input.CheckNext(2, Lexeme.FIRST, Lexeme.SECOND))
                    {
                        input.MoveNext();
                        break;
                    }
                    
                    if (!input.CheckNext(2, Lexeme.VAR))
                    {
                        break;
                    }
                } 
            }
        }

        private void ParseLast(Text input)
        {
            while (true)
            {
                input.MoveExpected(Lexeme.VAR);

                if (!input.CheckNext(Lexeme.SEMICOLON))
                    break;

                input.MoveNext();
            }
        }

        private void ParseOper(Text input)
        {
            var label = input.MoveExpected(Lexeme.INT);
            input.MoveExpected(Lexeme.COLON);
            var variable = input.MoveExpected(Lexeme.VAR);
            input.MoveExpected(Lexeme.EQUAL);

            var calculator = new Calculator(input, _varValues);

            var val = calculator.Calculate();
            
            _labelValues[label.Value] = val;
            _varValues[variable.Value] = val;
        }
    }
}

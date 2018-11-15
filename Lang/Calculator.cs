using System;
using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    class Calculator
    {
        private readonly Text _input;
        private readonly Dictionary<string, double> _variables;

        public Calculator(Text input, Dictionary<string, double> variables)
        {
            _input = input;
            _variables = variables;
        }

        public double Calculate()
        {
            var operands = new[] {Lexeme.SIN, Lexeme.COS, Lexeme.ABS, Lexeme.INT, Lexeme.VAR};
            var operators = new[] {Lexeme.PLUS, Lexeme.MINUS, Lexeme.DIVIDE, Lexeme.MULTIPLY, Lexeme.POW};

            //условие выхода: 1. след слово END 2. след слово + 1 = ":"
            var expression = new List<LexemeContainer>();

            if (_input.CheckNext(Lexeme.MINUS))
            {
                expression.Add(new LexemeContainer(Lexeme.INT, "0"));
                expression.Add(_input.MoveExpected(Lexeme.MINUS));
            }

            expression.Add(_input.MoveExpected(operands));

            while (true)
            {
                expression.Add(_input.MoveExpected(operators));
                expression.Add(_input.MoveExpected(operands));
                if (_input.CheckNext(Lexeme.COLON))
                {
                    throw new ParseException("После операции должен быть операнд", _input.Position);
                }

                if (_input.CheckNext(Lexeme.INT) && _input.CheckNext(2, Lexeme.COLON))
                    break;

                if (_input.CheckNext(Lexeme.END))
                {
                    break;
                }
            }

            return CalculateExpression(expression);
        }

        private double GetOperandValue(LexemeContainer lexeme)
        {
            if (lexeme.Lexeme == Lexeme.VAR)
            {
                if (_variables.TryGetValue(lexeme.Value, out var value))
                {
                    return value;
                }

                return 0;
            }

            if (lexeme.Lexeme == Lexeme.INT)
            {
                return Convert.ToInt32(lexeme.Value, 8);
            }
            if (lexeme.Lexeme == Lexeme.DOUBLE)
            {
                return double.Parse(lexeme.Value);
            }

            if (lexeme.Lexeme == Lexeme.SIN)
            {
                var contentValue = GetOperandValue(lexeme.FuncContent);
                return Math.Sin(contentValue);
            }

            if (lexeme.Lexeme == Lexeme.COS)
            {
                var contentValue = GetOperandValue(lexeme.FuncContent);
                return Math.Cos(contentValue);
            }

            if (lexeme.Lexeme == Lexeme.ABS)
            {
                var contentValue = GetOperandValue(lexeme.FuncContent);
                return Math.Abs(contentValue);
            }

            throw new Exception("Can't get operand value for lexeme " + lexeme.Lexeme);
        }

        private void ProcessValue(LinkedList<LexemeContainer> list, Func<Lexeme, double, double, double> processFunc, params Lexeme[] lexemes)
        {
            var current = list.First;
            while (true)
            {
                var value = current.Value;
                if (!lexemes.Contains(value.Lexeme))
                {
                    current = current.Next;
                    if (current == null) break;
                    continue;
                }
                var rvalue = GetOperandValue(current.Previous.Value);
                var lvalue = GetOperandValue(current.Next.Value);

                var result = processFunc(value.Lexeme, rvalue, lvalue);
                var newValue = new LexemeContainer(Lexeme.DOUBLE, result.ToString());

                var prev = current.Previous.Previous;
                var next = current.Next.Next;

                var linkedList = current.List;

                linkedList.Remove(current.Next);
                linkedList.Remove(current.Previous);
                linkedList.Remove(current);
                    
                if (prev != null)
                {
                    linkedList.AddAfter(prev, newValue);
                }
                else
                {
                    linkedList.AddFirst(newValue);
                }

                current = next;
                if (current == null) break;
            }
        }

        private double CalculateExpression(List<LexemeContainer> lexemes)
        {
            var linkedList = new LinkedList<LexemeContainer>(lexemes);

            ProcessValue(linkedList, (_, d1, d2) => Math.Pow(d1, d2), Lexeme.POW);
            ProcessValue(linkedList,
                (l, d1, d2) =>
                {
                    if (l == Lexeme.MULTIPLY) return d1 * d2;
                    if (l == Lexeme.DIVIDE) return d1 / d2;
                    throw new Exception();
                },
                Lexeme.MULTIPLY, Lexeme.DIVIDE);
            ProcessValue(linkedList,
                (l, d1, d2) =>
                {
                    if (l == Lexeme.PLUS) return d1 + d2;
                    if (l == Lexeme.MINUS) return d1 - d2;
                    throw new Exception();
                },
                Lexeme.PLUS, Lexeme.MINUS);

            if (linkedList.Count != 1)
            {
                throw new Exception("Должен был остаться один элемент");
            }

            var lastValue = linkedList.Single();
            if (lastValue.Lexeme != Lexeme.DOUBLE)
            {
                throw new Exception("Последний элемент должен быть DOUBLE");
            }

            return double.Parse(lastValue.Value);
        }
    }
}

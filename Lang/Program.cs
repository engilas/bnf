using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lang
{
    static class Program
    {
        private static Dictionary<string, double> _labelValues = new Dictionary<string, double>();
        private static Dictionary<string, double> _varValues = new Dictionary<string, double>();

        static void ParseLang(Text input)
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
        }

        static void ParseZveno(Text input)
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

        static void ParseLast(Text input)
        {
            while (true)
            {
                input.MoveExpected(Lexeme.VAR);

                if (!input.CheckNext(Lexeme.SEMICOLON))
                    break;

                input.MoveNext();
            }
        }

        static void ParseOper(Text input)
        {
            var label = input.MoveExpected(Lexeme.INT);
            input.MoveExpected(Lexeme.COLON);
            var variable = input.MoveExpected(Lexeme.VAR);
            input.MoveExpected(Lexeme.EQUAL);
            var val = CalculateRightPart(input);
            
            _labelValues[label.Value] = val;
            _varValues[variable.Value] = val;
        }

        static double CalculateRightPart(Text input)
        {
            var operands = new[] {Lexeme.SIN, Lexeme.COS, Lexeme.ABS, Lexeme.INT, Lexeme.VAR};
            var operators = new[] {Lexeme.PLUS, Lexeme.MINUS, Lexeme.DIVIDE, Lexeme.MULTIPLY, Lexeme.POW};

            //условие выхода: 1. след слово END 2. след слово + 1 = ":"
            var expression = new List<LexemeContainer>();

            if (input.CheckNext(Lexeme.MINUS))
            {
                expression.Add(new LexemeContainer(Lexeme.INT, "0"));
                expression.Add(input.MoveExpected(Lexeme.MINUS));
            }

            expression.Add(input.MoveExpected(operands));

            while (true)
            {
                expression.Add(input.MoveExpected(operators));
                expression.Add(input.MoveExpected(operands));
                if (input.CheckNext(Lexeme.COLON))
                {
                    throw new ParseException("После оператора должен быть операнд", input.Position);
                }

                if (input.CheckNext(Lexeme.INT) && input.CheckNext(2, Lexeme.COLON))
                    break;

                if (input.CheckNext(Lexeme.END))
                {
                    break;
                }
            }

            return CalculateExpression(expression);
        }

        static double GetOperandValue(LexemeContainer lexeme)
        {
            if (lexeme.Lexeme == Lexeme.VAR)
            {
                if (_varValues.TryGetValue(lexeme.Value, out var value))
                {
                    return value;
                }

                return 0;
            }

            if (lexeme.Lexeme == Lexeme.INT)
            {
                return int.Parse(lexeme.Value);
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

        static void ProcessValue(LinkedList<LexemeContainer> list, Func<Lexeme, double, double, double> processFunc, params Lexeme[] lexemes)
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

        static double CalculateExpression(List<LexemeContainer> lexemes)
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

        static void Main(string[] args)
        {
            var input = @"
fBegin 
First 123, 456 
Second ag1 tt6 
First 4 
First 5,6,5 
Second agr 
Second ag1 tt6  fa 
fq ; tt1 ; ga
5: a = 5  / 2
12: a = a+1
12: b=-a^2
End";
            Console.WriteLine(input);
            Text text;
            try
            {
                text = Text.Create(input);

                if (text.Length == 0)
                {
                    Console.WriteLine("Text is empty");
                    Console.ReadKey();
                    return;
                }

                for (var i = 0; i < text.Lexemes.Count; i++)
                {
                    var lexemeContainer = text.Lexemes[i];
                    Console.WriteLine($"{i}. {lexemeContainer.Lexeme}: {lexemeContainer.Value}");
                }

                ParseLang(text);
                
                Console.WriteLine("OK");
                Console.WriteLine("Variables");
                foreach (var varValue in _varValues)
                {
                    Console.WriteLine($"{varValue.Key} = {varValue.Value}");
                }
                Console.WriteLine("Lables");
                foreach (var labelValue in _labelValues)
                {
                    Console.WriteLine($"{labelValue.Key} : {labelValue.Value}");
                }
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Error);
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.ReadKey();
        }
    }
}

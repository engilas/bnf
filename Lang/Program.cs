using System;

namespace Lang
{
    static class Program
    {
        static void Main(string[] args)
        {
            var input = @"
Begin
First 123, 456 
Second ag1 tt6 
First 4 
First 5,6,5 
Second agr 
Second ag1 tt6  fa 
fq ; tt1 ; ga
5: a = 5  / 3
12: a = a +1
14: b=-a^2
End a";
            Console.WriteLine(input);
            Text text;
            try
            {
                text = TextBuilder.Create(input);

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

                var parser = new Parser();
                parser.ParseLang(text);
                var varValues = parser.GetVarValues();
                var labelValues = parser.GetLabelValues();
                
                Console.WriteLine("OK");
                Console.WriteLine("Variables");
                foreach (var varValue in varValues)
                {
                    Console.WriteLine($"{varValue.Key} = {OctConverter.Convert(varValue.Value)}");
                }
                Console.WriteLine("Lables");
                foreach (var labelValue in labelValues)
                {
                    Console.WriteLine($"{labelValue.Key} : {OctConverter.Convert(labelValue.Value)}");
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

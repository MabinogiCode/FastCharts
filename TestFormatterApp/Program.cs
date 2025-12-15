using System;
using System.Globalization;
using FastCharts.Core.Formatting;

namespace TestFormatterApp
{
    class Program
    {
        static void Main()
        {
            var formatter2 = new ScientificNumberFormatter(2);
            
            Console.WriteLine($"2.5 (2 digits) = '{formatter2.Format(2.5)}'");
            Console.WriteLine($"Expected: '2.50E+00'");
            
            // Debug the format string generation
            int significantDigits = 2;
            var formatString = "0." + new string('0', Math.Max(0, significantDigits - 1));
            Console.WriteLine($"Format string: '{formatString}'");
            Console.WriteLine($"2.5.ToString('{formatString}') = '{2.5.ToString(formatString, CultureInfo.InvariantCulture)}'");
            
            // The issue might be that significantDigits-1 gives us only 1 decimal for 2 significant digits
            // But 2.5 naturally has 2 significant digits already
        }
    }
}
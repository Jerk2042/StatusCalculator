using System;
using System.Data;

namespace StatusCalculator
{
    public class ExpressionEvaluator
    {
        public static double Evaluate(string expression)
        {
            var dataTable = new DataTable();
            try
            {
                // Use the DataTable.Compute method to evaluate the expression
                var result = dataTable.Compute(expression, string.Empty);
                return Convert.ToDouble(result);
            }
            catch (Exception)
            {
                // In case of an invalid expression, return 0.
                return 0;
            }
        }
    }
}

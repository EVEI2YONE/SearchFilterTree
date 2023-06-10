using Models.DictionaryNamespace;
using Models.EnumNamespace;
using Models.FilterExpressionNamespace;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Models.FilterExpressionTreeAnalyzerNamespace
{
    public partial class FilterExpressionTreeAnalyzer
    {
        /*
        1: A or B
        2: C or B //A or B | 2 => 1 (Index expression with their tokens, and then map expressions to find duplicates)

        A: 3 or 4
        C: 3 or 4 //C => A
        */

        /*
        1: A or B
        1: C or B //1 is already defined
        A: 2 or 3
        C: 2 or 3 //A = C
        //Both defined expressions of 1 are equivalent
            1                   1                       1
        A   or  B           C   or  B       =>      A/C or  B
    2   or  3          2   or  3                2   or  3

        1: A or B
        1: C or B //1 is already defined, Alias C = A
        A: 2 or 3 //Register C: 2 to 3
        C: 4 or 3 //A = C, Alias 2 = 4
        3: A or B //3 = 1, but A: 2 or 3 is invalid. Cannot contain itself in a defintion
        */

        private AliasDictionary aliases;
        private IDictionary<string, List<FilterExpressionWrapper>> expressionChildren;
        private IDictionary<string, List<string>> expressionMapper;
        
        public IEnumerable<FilterExpression> NormalizeExpressions(IEnumerable<FilterExpression> expressions)
        {
            InitializeVariables();

            var (valid, invalid) = ValidateExpressions(new List<FilterExpression>(expressions));
            var wrappers = CreateWrappers(valid);
            bool duplicateName, duplicateExpression, hasSelfReference;
            foreach(var wrapper in wrappers)
            {
                duplicateName = IsDuplicateAlias(wrapper.Name); 
                duplicateExpression = HasDuplicateExpression(wrapper);
                hasSelfReference = HasSelfReference(wrapper);

            }


            return valid;
        }
        private void InitializeVariables()
        {
            aliases = new AliasDictionary();
            expressionChildren = new Dictionary<string, List<FilterExpressionWrapper>>();
            expressionMapper = new Dictionary<string, List<string>>();
        }

        private (IEnumerable<FilterExpression> Valid, IEnumerable<FilterExpression> Invalid) 
            ValidateExpressions(IEnumerable<FilterExpression> expressions)
        {
            List<FilterExpression> invalid = new List<FilterExpression>();
            List<FilterExpression> valid = new List<FilterExpression>();
            //Register expression names
            foreach (var expr in expressions)
            {
                //validate syntax per expression
                try { valid.Add((FilterExpressionMetadata)expr); }
                catch (Exception) { invalid.Add(expr); }
            }
            return (valid, invalid);
        }

        private IEnumerable<FilterExpressionWrapper> CreateWrappers(IEnumerable<FilterExpression> expressions)
            => expressions.Select(expr =>
            {
                var (left, op, right) = ((FilterExpressionMetadata)expr).ExtractTokensFromSyntax();
                var wrapper = new FilterExpressionWrapper()
                {
                    Left = left,
                    Right = right,
                    Operator = op,
                    Name = expr.Name,
                    Value = expr.Value
                };
                return wrapper;
            });

        private bool IsDuplicateAlias(string alias)
        {
            if (aliases.ContainsAlias(alias)) 
                return true;
            aliases[alias] = alias;
            return false;
        }

        public bool HasDuplicateExpression(FilterExpressionWrapper wrapper)
        {
            var (name, left, op, right) = (wrapper.Name, wrapper.Left, wrapper.Operator, wrapper.Right);
            var expression_v1 = $"{left} {op} {right}";
            var expression_v2 = $"{right} {op} {left}";
            RegisterList(name);
            RegisterList(expression_v1);
            RegisterList(expression_v2);

            return TryAddDuplicate(name, expression_v1) | TryAddDuplicate(name, expression_v2); //bitwise OR
        }

        private void RegisterList(string name)
        {
            if (!expressionMapper.ContainsKey(name))
                expressionMapper[name] = new List<string>();
        }
        private bool TryRegisterListElement(string name, string value)
        {
            bool duplicate = false;
            if (expressionMapper[name].Contains(value))
                duplicate = true;
            else
                expressionMapper[name].Add(value);
            return duplicate;
        }

        private bool TryAddDuplicate(string name, string value)
        {
            bool duplicate = false;
            //register name => value
            duplicate = !TryRegisterListElement(name, value);
            return duplicate;
        }

        public bool HasSelfReference(FilterExpressionWrapper wrapper)
        {
            var name = wrapper.Name;
            if (!expressionChildren.ContainsKey(name))
                expressionChildren[name] = new List<FilterExpressionWrapper>();
            return expressionChildren[name].Contains(wrapper);
        }
    }

    public class FilterExpressionWrapper
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public LogicalOperator Operator { get; set; }
    }
}

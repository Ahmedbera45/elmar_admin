using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace WorkflowEngine.Infrastructure.Services;

public class RuleEvaluator
{
    public bool Evaluate(string ruleExpression, Dictionary<string, object> inputs)
    {
        if (string.IsNullOrWhiteSpace(ruleExpression))
            return true;

        if (ruleExpression.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            return true;

        try
        {
            // Prepare inputs. Dynamic Core requires parameters to be passed.
            // We can convert the dictionary to an array of parameters if the expression uses placeholders like @0, @1
            // OR we can generate a lambda.

            // For simplicity with user-defined variables (e.g. "Age > 18"),
            // we can parse the expression with specific parameters.
            // However, System.Linq.Dynamic.Core usually works on IQueryable or objects.

            // Approach: Use a dummy list with one item and query it.
            var dummyData = new[] { inputs }.AsQueryable();

            // Important: System.Linq.Dynamic.Core accesses dictionary keys via indexer string or properties.
            // But inputs are `Dictionary<string, object>`.
            // Standard Syntax: `inputs["Age"] > 18` might be required if we pass the dict as object.
            // To support "Age > 18", we need to substitute or use a dynamic object.

            // Let's use a dynamic object logic or basic replacement for this "Commercial" phase.
            // BUT, strictly using the library:

            // 1. Create config
            var config = new ParsingConfig();

            // 2. We can try to replace known keys in expression with `np(...)` or similar,
            // but the best way is to pass values as parameters.
            // If the rule is "Age > 18" and inputs has "Age", we want it to work.

            // Let's try constructing a Lambda.
            // Parameter: Dictionary<string, object> x
            // Expression: Convert "Age" to `Convert.ToDouble(x["Age"])`? Complex.

            // Easier approach for this task:
            // "Age > 18" -> replace with @0 > 18 and pass value? No, we don't know the order.

            // Let's use the DynamicClass feature of the library if possible, or simpler text replacement for this phase
            // if strict parsing is hard without a typed model.

            // Better yet: Convert Dictionary to Dynamic Object.
            var dynamicObj = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
            foreach (var kvp in inputs)
            {
                dynamicObj[kvp.Key] = kvp.Value;
            }

            // Now we have an object where properties exist.
            // "Age > 18"
            // We can use `new[] { dynamicObj }.AsQueryable().Any("Age > 18")`

            var queryable = new[] { dynamicObj }.AsQueryable();
            return queryable.Any(ruleExpression);
        }
        catch (Exception)
        {
            // Log error in real world
            return false;
        }
    }
}

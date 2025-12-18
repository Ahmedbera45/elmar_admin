using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WorkflowEngine.Core.Common;

public static class TemplateHelper
{
    public static string ReplacePlaceholders(string template, Dictionary<string, object?> values)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        // Pattern to find {{Key}}
        return Regex.Replace(template, @"\{\{([a-zA-Z0-9_]+)\}\}", match =>
        {
            var key = match.Groups[1].Value;
            if (values.TryGetValue(key, out var val))
            {
                return val?.ToString() ?? "";
            }
            return "";
        });
    }
}

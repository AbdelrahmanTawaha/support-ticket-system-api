namespace BusinessLayer.Services.AiSuggest
{
    public static class JsonNormalizer
    {
        public static string ExtractJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "{}";
            text = text.Trim();


            if (text.StartsWith("```", StringComparison.Ordinal))
            {
                var end = text.LastIndexOf("```", StringComparison.Ordinal);
                if (end > 0)
                    text = text.Substring(3, end - 3).Trim();


            }


            var firstObj = text.IndexOf('{');
            var lastObj = text.LastIndexOf('}');
            if (firstObj >= 0 && lastObj > firstObj)
                return text.Substring(firstObj, lastObj - firstObj + 1).Trim();


            var firstArr = text.IndexOf('[');
            var lastArr = text.LastIndexOf(']');
            if (firstArr >= 0 && lastArr > firstArr)
                return text.Substring(firstArr, lastArr - firstArr + 1).Trim();

            return "{}";
        }
    }
}

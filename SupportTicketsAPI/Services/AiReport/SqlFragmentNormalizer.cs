namespace SupportTicketsAPI.Services.AiReport
{
    public static class SqlFragmentNormalizer
    {
        public static string Normalize(string? fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment))
                return string.Empty;

            var text = fragment.Trim();


            if (text.StartsWith("```"))
            {
                var end = text.LastIndexOf("```", StringComparison.Ordinal);
                if (end > 0)
                    text = text.Substring(3, end - 3).Trim();


                text = text.Replace("sql", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            var lower = text.ToLowerInvariant();

            var whereIndex = lower.IndexOf("where ");
            var orderIndex = lower.IndexOf("order by ");

            int startIndex = -1;
            if (whereIndex >= 0) startIndex = whereIndex;
            if (orderIndex >= 0 && (startIndex == -1 || orderIndex < startIndex))
                startIndex = orderIndex;

            if (startIndex > 0)
            {
                text = text.Substring(startIndex).Trim();
                lower = text.ToLowerInvariant();
            }

            if (lower.StartsWith("and "))
            {
                text = "WHERE 1=1 " + text;
                lower = text.ToLowerInvariant();
            }


            text = text.Replace("Status = 'Open'", "Status IN (0,1,2)", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status='Open'", "Status IN (0,1,2)", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status = \"Open\"", "Status IN (0,1,2)", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status=\"Open\"", "Status IN (0,1,2)", StringComparison.OrdinalIgnoreCase)

                       .Replace("Status = 'Closed'", "Status = 4", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status='Closed'", "Status = 4", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status = \"Closed\"", "Status = 4", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status=\"Closed\"", "Status = 4", StringComparison.OrdinalIgnoreCase)

                       .Replace("Status = 'Resolved'", "Status = 3", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status='Resolved'", "Status = 3", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status = \"Resolved\"", "Status = 3", StringComparison.OrdinalIgnoreCase)
                       .Replace("Status=\"Resolved\"", "Status = 3", StringComparison.OrdinalIgnoreCase);


            lower = text.ToLowerInvariant();
            if (lower.StartsWith("where and "))
            {
                text = "WHERE 1=1 " + text.Substring("where".Length).Trim();
            }

            return text.Trim();
        }

        public static string ExtractJson(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "{}";

            var s = text.Trim();

            if (s.StartsWith("```"))
            {
                var end = s.LastIndexOf("```", StringComparison.Ordinal);
                if (end > 0) s = s.Substring(3, end - 3).Trim();
                s = s.Replace("json", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            var start = s.IndexOf('{');
            var last = s.LastIndexOf('}');
            if (start < 0 || last <= start) return "{}";

            return s.Substring(start, last - start + 1).Trim();
        }
    }
}

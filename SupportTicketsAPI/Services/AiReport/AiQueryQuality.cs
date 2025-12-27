using System.Text.RegularExpressions;

namespace SupportTicketsAPI.Services.AiReport
{
    public static class AiQueryQuality
    {
        private static readonly HashSet<string> DefaultFragments = new(StringComparer.OrdinalIgnoreCase)
        {
            "order by createdat desc",
            "where status in (0,1,2) order by createdat desc",
            "where status<>4 order by createdat desc",
            "where status != 4 order by createdat desc"
        };

        public static (bool isFallback, string? warning) Evaluate(string? fragment, int rowsCount)
        {
            var canon = Canon(fragment);


            if (canon == "")
            {
                if (rowsCount > 0) return (false, null);

                return (true, "لا توجد نتائج. جرّب إعادة المحاولة أو اكتب تفاصيل أكثر.");
            }

            if (DefaultFragments.Contains(canon))
            {
                return (true, "هذه نتيجة تجريبية وقد لا تكون مرتبطة بسؤالك. جرّب إعادة المحاولة أو اكتب تفاصيل أكثر.");
            }

            return (false, null);
        }

        private static string Canon(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var x = s.Trim().ToLowerInvariant();
            x = Regex.Replace(x, @"\s+", " ");
            return x;
        }
    }
}

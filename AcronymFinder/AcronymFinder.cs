using System.Text.RegularExpressions;

namespace AcronymFinder;

public static class AcronymFinder
{
    private static readonly string BasicRegex = @"\b([A-Z]{2,})s?\b";

    private static readonly string ExpandedRegex =
        @"([A-Z][\w\-]+(?:[, ]{1,2}[A-Z][\w\-]+)+)[ .,]{0,2}\(" + BasicRegex + @"\)";

    private static readonly string ExpandedTolerantRegex =
        @"([A-Z][\w\-]+(?:[ ][\w\-]+)?(?:[, ]{1,2}[A-Z][\w\-]+)+)[ .,]{0,2}\("
        + BasicRegex
        + @"\)";

    public static Dictionary<string, List<string>> FindAllAcronyms(string text)
    {
        var acronyms = FindAcronyms(text);
        var expanded = FindExpandedAcronyms(text);
        return CombineAcronyms(acronyms, expanded);
    }

    public static Dictionary<string, List<string>> FindAcronyms(string text)
    {
        var acronyms = new Dictionary<string, List<string>>();
        var matches = Regex.Matches(text, BasicRegex);

        foreach (Match m in matches)
        {
            var acronym = m.Groups[1].Value;
            acronyms[acronym] = [""];
        }

        return acronyms;
    }

    public static Dictionary<string, List<string>> FindExpandedAcronyms(string text)
    {
        var acronyms = new Dictionary<string, List<string>>();
        var matches = Regex.Matches(text, ExpandedTolerantRegex);

        foreach (Match m in matches)
        {
            var expansion = m.Groups[1].Value;
            var acronym = m.Groups[2].Value;

            expansion = FixDividedExpansion(expansion, acronym, m);
            expansion = StripExtraneousWords(expansion, acronym);

            AddExpansion(acronyms, acronym, expansion);
        }

        return acronyms;
    }

    public static string StripExtraneousWords(string expansion, string acronym)
    {
        var exp = expansion.Split(' ').ToList();

        // Basic case where everything matches up nicely
        if (
            expansion[0] == acronym[0]
            && exp.Count <= acronym.Length
        )
            return expansion;

        // At least two words in the expansion, two characters in the acronym,
        // the first two expansion words have the same first letter,
        // BUT the acronym does not start with a double letter.
        while (
            exp.Count > 1
            && acronym.Length > 1
            && exp[0][0] == exp[1][0]
            && acronym[0] != acronym[1]
        )
            exp.RemoveAt(0);

        // Simple case where the first word of the expansion doesn't match the first letter of the acronym
        while (exp.Count > 0 && exp[0][0] != acronym[0]) exp.RemoveAt(0);

        return string.Join(" ", exp);
    }

    private static string FixDividedExpansion(
        string expansion,
        string acronym,
        Match match
    )
    {
        var wordsInExpansion = Regex.Split(expansion, @"\W+");
        if (wordsInExpansion.Length >= acronym.Length)
            return expansion;

        var capsInExpansion = Regex.Split(expansion, "[A-Z]");
        if (capsInExpansion.Length >= acronym.Length)
            return expansion;

        var start = Math.Max(0, match.Index - 100);
        var preceding = match.Value.Substring(start, match.Index - start - 1);
        var pre = Regex.Split(preceding, @"\W+").Where(s => !string.IsNullOrEmpty(s)).ToList();

        var lowercaseCount = 0;
        var candidates = new List<string>();

        while (pre.Count > 0 && lowercaseCount < 2)
        {
            var curr = pre[pre.Count - 1];
            pre.RemoveAt(pre.Count - 1);

            if (char.IsLower(curr[0]))
                lowercaseCount++;
            else
                lowercaseCount = 0;

            candidates.Insert(0, curr);
        }

        candidates = candidates.Skip(lowercaseCount).ToList();
        var newExpansion = (
            string.Join(" ", candidates) + " " + expansion
        ).Trim();

        return newExpansion;
    }

    public static Dictionary<string, List<string>> CombineAcronyms(
        Dictionary<string, List<string>> first,
        Dictionary<string, List<string>> second
    )
    {
        var combined = new Dictionary<string, List<string>>(first);

        foreach (var kvp in second)
            if (combined.ContainsKey(kvp.Key))
                combined[kvp.Key] = combined[kvp.Key]
                    .Union(kvp.Value)
                    .ToList();
            else
                combined[kvp.Key] = kvp.Value;
        
        foreach (var kvp in combined)
            if (combined[kvp.Key].Count > 1)
                combined[kvp.Key] = combined[kvp.Key].Where(v => v != "").ToList();

        return combined;
    }

    private static void AddExpansion(
        Dictionary<string, List<string>> acronyms,
        string acronym,
        string expansion
    )
    {
        if (!acronyms.ContainsKey(acronym))
            acronyms[acronym] = [expansion];
        else if (!acronyms[acronym].Contains(expansion)) acronyms[acronym].Add(expansion);
    }

    public static Dictionary<string, List<string>> FindUnusedAcronyms(
        Dictionary<string, List<string>> foundAcronyms,
        Dictionary<string, List<string>> definedAcronyms
    )
    {
        return definedAcronyms
            .Where(kvp => !foundAcronyms.ContainsKey(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
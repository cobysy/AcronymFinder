using System.Text.Json;

namespace AcronymFinder.Test;

public class AcronymFindingTests
{
    [Fact]
    public void TestAcronymsFound()
    {
        var acronyms = AcronymFinder.FindAcronyms("Hello");
        Assert.Empty(acronyms);

        acronyms = AcronymFinder.FindAcronyms("Did you work on the ABC project?");
        Assert.Single(acronyms);

        acronyms = AcronymFinder.FindAcronyms("GE Corp built XYZ's froznoTRON");
        Assert.Equal(2, acronyms.Count);
        Assert.Contains("GE", acronyms.Keys);
        Assert.Contains("XYZ", acronyms.Keys);

        Assert.Equal([""], acronyms["GE"]);
        Assert.Equal([""], acronyms["XYZ"]);
    }

    [Fact]
    public void TestFindExpandedAcronyms()
    {
        var acronyms = AcronymFinder.FindExpandedAcronyms("Howdy Neighbor");
        Assert.Empty(acronyms);

        acronyms = AcronymFinder.FindExpandedAcronyms(
            "Did you work on the Alpha Beta Company (ABC) project?"
        );
        Assert.Single(acronyms);
        Assert.Contains("ABC", acronyms.Keys);
        Assert.Equal(["Alpha Beta Company"], acronyms["ABC"]);

        var expansions = new List<string>
        {
            "Different Expansions Fail",
            "Dubious Expressions Fly"
        };
        acronyms = AcronymFinder.FindExpandedAcronyms(
            $"{expansions[0]} (DEF) if {expansions[1]} (DEF)"
        );
        Assert.Single(acronyms);
        Assert.Contains("DEF", acronyms.Keys);
        Assert.Equal(2, acronyms["DEF"].Count);
        Assert.Equal(expansions, acronyms["DEF"]);

        // Test multiple occurrences of same expansion
        acronyms = AcronymFinder.FindExpandedAcronyms(
            $"{expansions[0]} (DEF) if {expansions[1]} (DEF) {expansions[0]} (DEF)"
        );
        Assert.Single(acronyms);
        Assert.Contains("DEF", acronyms.Keys);
        Assert.Equal(2, acronyms["DEF"].Count);
        Assert.Equal(expansions, acronyms["DEF"]);

        // Test Inc. case
        acronyms = AcronymFinder.FindExpandedAcronyms(
            "I think he works at My Favorite Company, Inc. (MFCI), or at least he used to."
        );
        Assert.Single(acronyms);
        Assert.Contains("MFCI", acronyms.Keys);
        Assert.Equal("My Favorite Company, Inc", acronyms["MFCI"][0]);
    }

    [Fact]
    public void TestExpandedAcronymsSentenceLeadingCaps()
    {
        var acronyms = AcronymFinder.FindExpandedAcronyms(
            "The New Old Cat Factory (NOCF) is big."
        );
        Assert.Single(acronyms);
        Assert.Contains("NOCF", acronyms.Keys);
        Assert.Equal(["New Old Cat Factory"], acronyms["NOCF"]);

        acronyms = AcronymFinder.FindExpandedAcronyms(
            "Come visit The New Old Dog Factory (TNODF)"
        );
        Assert.Single(acronyms);
        Assert.Contains("TNODF", acronyms.Keys);
        Assert.Equal(
            ["The New Old Dog Factory"],
            acronyms["TNODF"]
        );
    }

    [Fact]
    public void TestSentenceLeadingCapsStrip()
    {
        var extraneousExpansion = "The Noodle Flopper";
        var expansion = "Noodle Flopper";
        var acronym = "NF";

        Assert.Equal(
            expansion,
            AcronymFinder.StripExtraneousWords(extraneousExpansion, acronym)
        );

        var notExtraneous = "The Leading Article Company";
        acronym = "TLAC";

        Assert.Equal(
            notExtraneous,
            AcronymFinder.StripExtraneousWords(notExtraneous, acronym)
        );

        var text =
            "Thus The Leading Article Company (TLAC) realized its need for following verbs.";
        Assert.Equal(
            notExtraneous,
            AcronymFinder.StripExtraneousWords(
                "Thus The Leading Article Company",
                acronym
            )
        );

        var acronyms = AcronymFinder.FindExpandedAcronyms(text);
        Assert.Single(acronyms);
        Assert.Contains("TLAC", acronyms.Keys);
        Assert.Equal([notExtraneous], acronyms["TLAC"]);
    }

    [Fact]
    public void TestCatchDividedExpansions()
    {
        var correctExpansion = "The Leading Article Preposition Company, Inc";
        var acronym = "TLAPCI";
        var text =
            $"When I worked for {correctExpansion} ({acronym}), "
            + "we made sure that prepositions were never something we ended sentences with.";

        var acronyms = AcronymFinder.FindExpandedAcronyms(text);
        Assert.Single(acronyms);
        Assert.Contains(acronym, acronyms.Keys);
        Assert.Equal(correctExpansion, acronyms[acronym][0]);
    }

    [Fact]
    public void TestShouldNotSpanExpansionsAcrossLineBreaks()
    {
        var acronyms = AcronymFinder.FindExpandedAcronyms(
            "Section 1.3.2 Alpha Beta Company\nThe Alpha Beta Company (ABC)"
        );
        Assert.Single(acronyms);
        Assert.Contains("ABC", acronyms.Keys);
        Assert.Equal(["Alpha Beta Company"], acronyms["ABC"]);

        acronyms = AcronymFinder.FindExpandedAcronyms(
            "Section 1.3.2 Alpha Beta Company\rThe Alpha Beta Company (ABC)"
        );
        Assert.Single(acronyms);
        Assert.Contains("ABC", acronyms.Keys);
        Assert.Equal(["Alpha Beta Company"], acronyms["ABC"]);
    }

    [Fact]
    public void TestAcronymsWithEmbeddedAcronyms()
    {
        var text =
            "The ALPha Beta Company (ALPBC) is a company. Yes, ALPBC is for realz. "
            + "More in Appendix Q.6 ALPha Beta Company (ALPBC).";
        var acronyms = AcronymFinder.FindExpandedAcronyms(text);
        Assert.Single(acronyms);
        Assert.Contains("ALPBC", acronyms.Keys);
        Assert.Single(acronyms["ALPBC"]);
        Assert.Equal("ALPha Beta Company", acronyms["ALPBC"][0]);
    }

    [Fact]
    public void TestFindAllAcronyms()
    {
        var text =
            "My BUS went to Zither Yak Xylophone (ZYX) school. IDK why. "
            + "I hope you Like My Story (LMS), LOL. "
            + "FYI, I meant Laughing Out Loud (LOL). LOL.";

        var acronyms = AcronymFinder.FindAllAcronyms(text);
        Assert.Equal(6, acronyms.Count);

        var expected = new Dictionary<string, List<string>>
        {
            { "BUS", [""] },
            { "ZYX", ["Zither Yak Xylophone"] },
            { "IDK", [""] },
            { "LMS", ["Like My Story"] },
            { "LOL", ["Laughing Out Loud"] },
            { "FYI", [""] }
        };

        Assert.Equal(expected, acronyms);
    }

    [Fact]
    public void TestCombineAcronymLists()
    {
        var firstText = "I bought a TV from QVC Pretty Darn Quick (PDQ).";
        var secondText = "My TV is made by Broken Equipment Corp (BEC). OMG.";

        var combinedText = $"{firstText} {secondText}";
        var correctCombined = AcronymFinder.FindAllAcronyms(combinedText);

        var firstAcronyms = AcronymFinder.FindAllAcronyms(firstText);
        var secondAcronyms = AcronymFinder.FindAllAcronyms(secondText);

        var testCombined = AcronymFinder.CombineAcronyms(
            firstAcronyms,
            secondAcronyms
        );
        Assert.Equal(correctCombined, testCombined);
    }

    [Fact]
    public void TestPluralizedAcronyms()
    {
        var acronyms = AcronymFinder.FindAcronyms("There are four CDFs due today");
        Assert.Single(acronyms);
        Assert.Contains("CDF", acronyms.Keys);
        Assert.Equal([""], acronyms["CDF"]);

        acronyms = AcronymFinder.FindExpandedAcronyms(
            "The Corporate Data Files (CDFs) are missing."
        );
        Assert.Single(acronyms);
        Assert.Contains("CDF", acronyms.Keys);
        Assert.Equal(["Corporate Data Files"], acronyms["CDF"]);
    }

    [Fact]
    public void TestFindUnusedAcronyms()
    {
        var acronyms = AcronymFinder.FindExpandedAcronyms(
            "The Alpha Beta Company (ABC) project was Really Really Good (RRG)"
        );
        var definedAcronyms = AcronymFinder.FindAcronyms("ABC RRG LOL");
        Assert.Equal(2, acronyms.Count);
        Assert.Equal(3, definedAcronyms.Count);

        var unusedAcronyms = AcronymFinder.FindUnusedAcronyms(
            acronyms,
            definedAcronyms
        );
        Assert.Single(unusedAcronyms);
        Assert.Contains("LOL", unusedAcronyms.Keys);
    }
}
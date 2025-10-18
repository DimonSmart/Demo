namespace MarkdownToWordFeature.Services
{
    public enum InvisibleCharacterCategory
    {
        C0C1Controls = 1,
        LineBreaks = 2,
        Tab = 3,
        WideSpaces = 4,
        NoBreakSpaces = 5,
        ZeroWidthFormat = 6,
        BiDiControls = 7,
        SoftHyphen = 8,
        InvisibleMath = 9,
        VariationSelectors = 10,
        EmojiTags = 11,
        CombiningMarks = 12,
        Confusables = 13
    }

    public enum CleaningPreset
    {
        Safe,           // Default universal cleaning
        Aggressive,     // Maximum cleaning
        ASCIIStrict,    // Code/documentation, CI builds  
        TypographySoft, // Publications/typesetting
        RTLSafe,        // Texts with Arabic/Hebrew
        SEOPlain        // Blogs/content transfer
    }

    public class CleaningOptions
    {
        public bool SkipCodeBlocks { get; set; } = true;
        public int TabSize { get; set; } = 4;
        public bool PreserveZWJZWNJ { get; set; } = false;
        public bool InvisibleMathToSpace { get; set; } = true;
        public HashSet<int> WhitelistedCharacters { get; set; } = new();
        public HashSet<int> BlacklistedCharacters { get; set; } = new();
    }

    public class CleaningStatistics
    {
        public Dictionary<InvisibleCharacterCategory, int> RemovedCounts { get; set; } = new();
        public int TotalRemoved { get; set; } = 0;
    }
}
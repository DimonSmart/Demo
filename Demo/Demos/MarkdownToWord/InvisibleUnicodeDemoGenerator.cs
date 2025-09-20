namespace Demo.Demos.MarkdownToWord;

using System.Text;

public static class InvisibleUnicodeDemoGenerator
{
    public static string BuildMarkdown()
    {
        var builder = new StringBuilder();
        builder.AppendLine("**Comprehensive invisible character test - each bullet demonstrates a different category.**");
        
        // Category 1: C0/C1 Controls
        builder.AppendLine($"- C0 Control character: ping{'\u0001'}pong");
        builder.AppendLine($"- C1 Control character: data{'\u0081'}stream");
        
        // Category 2: Line breaks
        const char lineSeparator = '\u2028';
        const char paragraphSeparator = '\u2029';
        builder.AppendLine($"- Line separator: top{lineSeparator}bottom");
        builder.AppendLine($"- Paragraph separator: first{paragraphSeparator}second");
        
        // Category 3: Tab
        const char tab = '\t';
        const string tabReplacement = "    ";
        builder.AppendLine($"- Tab character: left{tab}right → left{tabReplacement}right");
        
        // Category 4: Extended Wide spaces (comprehensive list)
        builder.AppendLine($"- EN QUAD: a{'\u2000'}b");
        builder.AppendLine($"- EM QUAD: x{'\u2001'}y");
        builder.AppendLine($"- EN SPACE: label{'\u2002'}value");
        builder.AppendLine($"- EM SPACE: word{'\u2003'}word");
        builder.AppendLine($"- THREE-PER-EM: num{'\u2004'}ber");
        builder.AppendLine($"- FOUR-PER-EM: test{'\u2005'}case");
        builder.AppendLine($"- SIX-PER-EM: item{'\u2006'}list");
        builder.AppendLine($"- FIGURE SPACE: 1{'\u2007'}234");
        builder.AppendLine($"- PUNCTUATION SPACE: word{'\u2008'}end");
        builder.AppendLine($"- THIN SPACE: label{'\u2009'}value");
        builder.AppendLine($"- HAIR SPACE: tight{'\u200A'}fit");
        builder.AppendLine($"- MEDIUM MATHEMATICAL SPACE: a{'\u205F'}=");
        builder.AppendLine($"- IDEOGRAPHIC SPACE: 中{'\u3000'}文");
        
        // Category 5: Non-breaking spaces
        const char nonBreakingSpace = '\u00A0';
        const char narrowNoBreakSpace = '\u202F';
        builder.AppendLine($"- Non-breaking space: keep{nonBreakingSpace}together → keep together");
        builder.AppendLine($"- Narrow no-break space: unite{narrowNoBreakSpace}tight → unite tight");
        
        // Category 6: Zero-width formatting (extended)
        const char zeroWidthSpace = '\u200B';
        const char zeroWidthNonJoiner = '\u200C';
        const char zeroWidthJoiner = '\u200D';
        const char wordJoiner = '\u2060';
        const char bom = '\uFEFF';
        const char mongolianVowelSeparator = '\u180E';
        builder.AppendLine($"- Zero-width space: join{zeroWidthSpace}ed");
        builder.AppendLine($"- Zero-width non-joiner: compound{zeroWidthNonJoiner}word");
        builder.AppendLine($"- Zero-width joiner: ligature{zeroWidthJoiner}text");
        builder.AppendLine($"- Word joiner: no{wordJoiner}break");
        builder.AppendLine($"- BOM/ZWNBSP: start{bom}text");
        builder.AppendLine($"- Mongolian vowel separator: old{mongolianVowelSeparator}style");
        
        // Category 7: BiDi controls (complete set)
        const char leftToRightMark = '\u200E';
        const char rightToLeftMark = '\u200F';
        const char leftToRightEmbedding = '\u202A';
        const char rightToLeftEmbedding = '\u202B';
        const char popDirectionalFormatting = '\u202C';
        const char leftToRightOverride = '\u202D';
        const char rightToLeftOverride = '\u202E';
        const char leftToRightIsolate = '\u2066';
        const char rightToLeftIsolate = '\u2067';
        const char firstStringIsolate = '\u2068';
        const char popDirectionalIsolate = '\u2069';
        builder.AppendLine($"- Left-to-right mark: start{leftToRightMark}end");
        builder.AppendLine($"- Right-to-left mark: text{rightToLeftMark}here");
        builder.AppendLine($"- LR embedding: begin{leftToRightEmbedding}middle{popDirectionalFormatting}end");
        builder.AppendLine($"- RL embedding: start{rightToLeftEmbedding}center{popDirectionalFormatting}finish");
        builder.AppendLine($"- LR override: force{leftToRightOverride}left{popDirectionalFormatting}text");
        builder.AppendLine($"- RL override: force{rightToLeftOverride}right{popDirectionalFormatting}text");
        builder.AppendLine($"- LR isolate: text{leftToRightIsolate}isolated{popDirectionalIsolate}text");
        builder.AppendLine($"- RL isolate: word{rightToLeftIsolate}separate{popDirectionalIsolate}word");
        builder.AppendLine($"- First string isolate: auto{firstStringIsolate}detect{popDirectionalIsolate}dir");
        
        // Category 8: Soft hyphen
        const char softHyphen = '\u00AD';
        builder.AppendLine($"- Soft hyphen: re{softHyphen}order");
        
        // Category 9: Invisible math operators (complete set)
        const char invisibleTimes = '\u2062';
        const char invisibleSeparator = '\u2063';
        const char invisiblePlus = '\u2064';
        builder.AppendLine($"- Invisible times: 2{invisibleTimes}x");
        builder.AppendLine($"- Invisible separator: a{invisibleSeparator}b");
        builder.AppendLine($"- Invisible plus: x{invisiblePlus}y");
        
        // Category 10: Variation selectors (expanded examples)
        const char variationSelector16 = '\uFE0F';
        const char variationSelector1 = '\uFE00';
        const string variationSelectorSupplement = "\U000E0100";
        builder.AppendLine($"- Variation selector 16: heart\u2764{variationSelector16}");
        builder.AppendLine($"- Variation selector 1: text{variationSelector1}variant");
        builder.AppendLine($"- Variation selector supplement: char{variationSelectorSupplement}alt");
        
        // Category 11: TAG characters (expanded examples)
        const string emojiTagSequence = "\U0001F3F4\U000E0067\U000E0062\U000E0073\U000E0063\U000E0074\U000E007F";
        const string tagSpace = "\U000E0020";
        const string tagLetter = "\U000E0041";
        builder.AppendLine($"- Emoji tag sequence: flag{emojiTagSequence} → flag");
        builder.AppendLine($"- TAG space: word{tagSpace}separator");
        builder.AppendLine($"- TAG letter: text{tagLetter}tagged");
        
        // Category 12: Combining marks
        const char combiningAcute = '\u0301';
        const char combiningGrave = '\u0300';
        const char combiningCircumflex = '\u0302';
        builder.AppendLine($"- Combining acute: accent e{combiningAcute}");
        builder.AppendLine($"- Combining grave: accent e{combiningGrave}");
        builder.AppendLine($"- Combining circumflex: accent e{combiningCircumflex}");
        
        // Category 13: Confusables (separated from invisible chars as suggested)
        builder.AppendLine();
        builder.AppendLine("**Confusable/suspicious characters (not invisible, but potentially problematic):**");
        const char confusableDash = '\u2014';
        const char leftDoubleQuote = '\u201C';
        const char rightDoubleQuote = '\u201D';
        const string mixedAlphabetWord = "pa\u0441sword";
        const char minusSign = '\u2212';
        builder.AppendLine($"- Em dash: word{confusableDash}dash → word-dash");
        builder.AppendLine($"- Smart quotes: {leftDoubleQuote}text{rightDoubleQuote} → \"text\"");
        builder.AppendLine($"- Mixed alphabet word: {mixedAlphabetWord} → password");
        builder.AppendLine($"- Minus sign: 5{minusSign}3 → 5-3");
        
        return builder.ToString();
    }
}

namespace Demo.Demos.MarkdownToWord;

using System.Text;

public static class InvisibleUnicodeDemoGenerator
{
    public static string BuildMarkdown()
    {
        const char control = '\u0001';
        const char tab = '\t';
        const char lineSeparator = '\u2028';
        const char thinSpace = '\u2009';
        const char nonBreakingSpace = '\u00A0';
        const char zeroWidthSpace = '\u200B';
        const char leftToRightMark = '\u200E';
        const char softHyphen = '\u00AD';
        const char invisibleSeparator = '\u2063';
        const char variationSelector = '\uFE0F';
        const char combiningAcute = '\u0301';
        const char confusableDash = '\u2014';
        const string emojiTagSequence = "\U0001F3F4\U000E0067\U000E0062\U000E0073\U000E0065\U000E0063\U000E0074\U000E007F";
        const string tabReplacement = "    ";

        var builder = new StringBuilder();
        builder.AppendLine("**Each bullet hides one character type handled by the cleaner.**");
        builder.AppendLine($"- Control character: ping{control}pong");
        builder.AppendLine($"- Tab character: left{tab}right → left{tabReplacement}right");
        builder.AppendLine($"- Exotic line break: top{lineSeparator}bottom");
        builder.AppendLine($"- Wide space: label{thinSpace}value");
        builder.AppendLine($"- Non-breaking space: keep{nonBreakingSpace}together → keep together");
        builder.AppendLine($"- Zero-width format: join{zeroWidthSpace}ed");
        builder.AppendLine($"- BiDi mark: start{leftToRightMark}end");
        builder.AppendLine($"- Soft hyphen: re{softHyphen}order");
        builder.AppendLine($"- Invisible math separator: 2{invisibleSeparator}x");
        builder.AppendLine($"- Variation selector: heart\u2764{variationSelector}");
        builder.AppendLine($"- Emoji tag sequence: flag{emojiTagSequence} → flag");
        builder.AppendLine($"- Combining mark (accent helper): accent e{combiningAcute}");
        builder.AppendLine($"- Confusable dash: word{confusableDash}dash → word-dash");
        return builder.ToString();
    }
}

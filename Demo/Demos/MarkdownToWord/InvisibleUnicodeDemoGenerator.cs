namespace Demo.Demos.MarkdownToWord;

using System.Collections.Generic;
using System.Text;

public static class InvisibleUnicodeDemoGenerator
{
    public static string BuildMarkdown()
    {
        var inv = new Dictionary<char, (string Alias, string UnicodeName)>
        {
            ['\u200B'] = ("ZWSP", "ZERO WIDTH SPACE"),
            ['\u200C'] = ("ZWNJ", "ZERO WIDTH NON-JOINER"),
            ['\u200D'] = ("ZWJ", "ZERO WIDTH JOINER"),
            ['\u2060'] = ("WJ", "WORD JOINER"),
            ['\uFEFF'] = ("FEFF", "ZERO WIDTH NO-BREAK SPACE / BOM"),
            ['\u00AD'] = ("SHY", "SOFT HYPHEN"),
            ['\u00A0'] = ("NBSP", "NO-BREAK SPACE"),
            ['\u200E'] = ("LRM", "LEFT-TO-RIGHT MARK"),
            ['\u200F'] = ("RLM", "RIGHT-TO-LEFT MARK"),
            ['\u2063'] = ("INVISIBLE SEPARATOR", "INVISIBLE SEPARATOR"),
            ['\u2062'] = ("INVISIBLE TIMES", "INVISIBLE TIMES"),
            ['\u2064'] = ("INVISIBLE PLUS", "INVISIBLE PLUS"),
        };
        var invSet = new HashSet<char>(inv.Keys);

        char ZWSP = '\u200B', ZWNJ = '\u200C', ZWJ = '\u200D', WJ = '\u2060',
             FEFF = '\uFEFF', SHY = '\u00AD', NBSP = '\u00A0', LRM = '\u200E',
             RLM = '\u200F', INVS = '\u2063', INVT = '\u2062', INVP = '\u2064';

        var lines = new[]
        {
            $"Это строка с невидимым символом между сло{ZWSP}вами.",
            $"В этом слове межбуквенный ZWNJ: ма{ZWNJ}штаб.",
            $"А здесь ZWJ: ко{ZWJ}мпания.",
            $"Неразрывный пробел между словами: 'Красная{NBSP}площадь'.",
            $"Мягкий перенос в слове: пре{SHY}ображение.",
            $"WORD JOINER между цифрами: 123{WJ}456.",
            $"FEFF внутри строки: до{FEFF}м.",
            $"LRM после запятой: привет,{LRM} мир.",
            $"RLM перед словом: привет {RLM}мир.",
            $"Invisible Separator U+2063: A{INVS}B.",
            $"Invisible Times U+2062: 2{INVT}x.",
            $"Invisible Plus U+2064: A{INVP}B.",
        };

        // Сбор попаданий: (№, line, col, alias, code, uname, context)
        var hits = new List<(int N, int Line, int Col, string Alias, string Code, string UName, string Ctx)>();
        var counter = 0;

        for (var ln = 0; ln < lines.Length; ln++)
        {
            var text = lines[ln];
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (!invSet.Contains(ch)) continue;

                counter++;
                var code = $"U+{(int)ch:X4}";
                var meta = inv[ch];
                var ctx = BuildContext(text, i, invSet, 8, 40);

                hits.Add((counter, ln + 1, i + 1, meta.Alias, code, meta.UnicodeName, ctx));
            }
        }

        // Markdown
        var sb = new StringBuilder();
        sb.AppendLine("# Invisible Unicode Characters — Demonstration File");
        sb.AppendLine("> This file is UTF-8. It **intentionally** hides invisible Unicode characters inside the sample text below.");
        sb.AppendLine("> Use the **Map of occurrences** to locate each hidden character by line and column.");
        sb.AppendLine();
        sb.AppendLine("## Sample text (contains invisible characters)");
        sb.AppendLine("```text");
        foreach (var l in lines) sb.AppendLine(l);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Map of invisible characters (positions)");
        sb.AppendLine("Each row has: index, 1-based line and column, short alias, code point, Unicode name, and context where the exact position is marked as `⟦◻⟧`.");
        sb.AppendLine();
        sb.AppendLine("| # | Line | Col | Alias | Code | Unicode name | Context |");
        sb.AppendLine("|---:|----:|----:|:------|:-----|:-------------|:--------|");
        foreach (var h in hits)
        {
            sb.Append("| ").Append(h.N)
              .Append(" | ").Append(h.Line)
              .Append(" | ").Append(h.Col)
              .Append(" | ").Append(h.Alias)
              .Append(" | ").Append(h.Code)
              .Append(" | ").Append(h.UName)
              .Append(" | ").Append(EscapePipes(h.Ctx))
              .AppendLine(" |");
        }

        sb.AppendLine();
        sb.AppendLine("## Legend");
        sb.AppendLine("- **ZWSP** — ZERO WIDTH SPACE (\\u200B)");
        sb.AppendLine("- **ZWNJ** — ZERO WIDTH NON-JOINER (\\u200C)");
        sb.AppendLine("- **ZWJ** — ZERO WIDTH JOINER (\\u200D)");
        sb.AppendLine("- **WJ** — WORD JOINER (\\u2060)");
        sb.AppendLine("- **FEFF** — ZERO WIDTH NO-BREAK SPACE / BOM (\\uFEFF)");
        sb.AppendLine("- **SHY** — SOFT HYPHEN (\\u00AD)");
        sb.AppendLine("- **NBSP** — NO-BREAK SPACE (\\u00A0)");
        sb.AppendLine("- **LRM** — LEFT-TO-RIGHT MARK (\\u200E)");
        sb.AppendLine("- **RLM** — RIGHT-TO-LEFT MARK (\\u200F)");
        sb.AppendLine("- **INVISIBLE SEPARATOR** — \\u2063");
        sb.AppendLine("- **INVISIBLE TIMES** — \\u2062");
        sb.AppendLine("- **INVISIBLE PLUS** — \\u2064");
        sb.AppendLine();
        sb.AppendLine("### Tips");
        sb.AppendLine("- To reveal positions in many editors, toggle *Show Invisibles* or paste the text into a hex/Unicode inspector.");
        sb.AppendLine("- Some tools may strip or normalize these characters on copy/paste.");

        return sb.ToString();
    }

    private static string BuildContext(string text, int index, HashSet<char> invSet, int windowVisible = 8, int maxWidth = 40)
    {
        // Собираем по windowVisible видимых символов слева/справа, игнорируя другие невидимые
        var left = new StringBuilder();
        for (var i = index - 1; i >= 0 && left.Length < windowVisible; i--)
        {
            if (!invSet.Contains(text[i])) left.Insert(0, text[i]);
        }
        var right = new StringBuilder();
        for (var i = index + 1; i < text.Length && right.Length < windowVisible; i++)
        {
            if (!invSet.Contains(text[i])) right.Append(text[i]);
        }

        var ctx = $"…{left}⟦◻⟧{right}…";
        return Shorten(ctx, maxWidth);
    }

    private static string Shorten(string s, int maxWidth)
    {
        if (s.Length <= maxWidth) return s;
        // Простейшее усечение с многоточием посередине
        var keep = maxWidth - 1; // с учётом '…'
        if (keep <= 0) return "…";
        return s.Substring(0, keep) + "…";
    }

    private static string EscapePipes(string s) => s.Replace("|", "\\|");
}

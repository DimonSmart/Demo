using Demo.Demos.MarkdownToWord;

var detector = new InvisibleCharacterDetectorService();
var cleaner = new InvisibleCharacterCleanerService();

Console.WriteLine("=== Tab handling ===");
var textWithTabs = "Hello\tworld\twith\ttabs";
var tabResult = cleaner.CleanText(textWithTabs, CleaningPreset.Safe);
Console.WriteLine($"Input:  '{textWithTabs}'");
Console.WriteLine($"Output: '{tabResult.CleanedText}'");
Console.WriteLine();

Console.WriteLine("=== Line breaks ===");
var textWithLineBreaks = "Line1\rLine2\u0085Line3\u2028Line4\u2029Line5";
var lineBreakResult = cleaner.CleanText(textWithLineBreaks, CleaningPreset.Safe);
Console.WriteLine($"Output: '{lineBreakResult.CleanedText}'");
Console.WriteLine();

Console.WriteLine("=== Wide spaces ===");
var textWithWideSpaces = "Word1\u2003Word2\u3000Word3";
var wideSpaceResult = cleaner.CleanText(textWithWideSpaces, CleaningPreset.Safe);
Console.WriteLine($"Output: '{wideSpaceResult.CleanedText}'");
Console.WriteLine();

Console.WriteLine("=== Invisible character detection ===");
var textWithInvisible = "Text\u200Bwith\u200Cinvisible\u2060marks";
var detection = detector.DetectInvisibleCharacters(textWithInvisible);
Console.WriteLine($"Detected: {detection.TotalCount}");
foreach (var item in detection.DetectedCharacters)
{
    Console.WriteLine($"- {item.Name} (U+{item.CodePoint:X4}) at {item.Position}");
}

var invisibleResult = cleaner.CleanText(textWithInvisible, CleaningPreset.Safe);
Console.WriteLine($"Cleaned: '{invisibleResult.CleanedText}'");

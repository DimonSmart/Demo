using Demo.Services;
using System;

class TestDetection
{
    static void Main()
    {
        var detector = new InvisibleCharacterDetectorService();
        var testText = "Hello\u0000World\u3000Test"; // NULL + широкий пробел
        
        var result = detector.DetectInvisibleCharacters(testText);
        
        Console.WriteLine($"Detected {result.DetectedCharacters.Count} characters:");
        foreach (var c in result.DetectedCharacters)
        {
            Console.WriteLine($"Position {c.Position}: U+{c.CodePoint:X4} - {c.Category}");
        }
    }
}
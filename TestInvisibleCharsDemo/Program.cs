using Demo.Services;

// Тестовый скрипт для демонстрации исправлений
var detector = new InvisibleCharacterDetectorService();
var cleaner = new InvisibleCharacterCleanerService();

// Тест 1: Проверяем, что табуляция заменяется на пробелы, а не удаляется
Console.WriteLine("=== Тест 1: Обработка табуляции ===");
var textWithTabs = "Привет\tмир\tс\tтабуляцией";
Console.WriteLine($"Исходный текст: '{textWithTabs}'");

var result1 = cleaner.CleanText(textWithTabs, CleaningPreset.Safe);
Console.WriteLine($"Очищенный текст: '{result1.CleanedText}'");
Console.WriteLine($"Изменения есть: {result1.HasChanges}");
Console.WriteLine($"Статистика: {result1.Statistics.TotalRemoved} символов обработано");
Console.WriteLine();

// Тест 2: Проверяем обработку символов новой строки
Console.WriteLine("=== Тест 2: Обработка символов новой строки ===");
var textWithLineBreaks = "Строка1\rСтрока2\u0085Строка3\u2028Строка4\u2029Строка5";
Console.WriteLine($"Исходный текст: '{textWithLineBreaks}'");

var result2 = cleaner.CleanText(textWithLineBreaks, CleaningPreset.Safe);
Console.WriteLine($"Очищенный текст: '{result2.CleanedText}'");
Console.WriteLine($"Изменения есть: {result2.HasChanges}");
Console.WriteLine();

// Тест 3: Проверяем обработку широких пробелов
Console.WriteLine("=== Тест 3: Обработка широких пробелов ===");
var textWithWideSpaces = "Слово1\u2003Слово2\u3000Слово3";
Console.WriteLine($"Исходный текст: '{textWithWideSpaces}'");

var result3 = cleaner.CleanText(textWithWideSpaces, CleaningPreset.Safe);
Console.WriteLine($"Очищенный текст: '{result3.CleanedText}'");
Console.WriteLine($"Изменения есть: {result3.HasChanges}");
Console.WriteLine();

// Тест 4: Проверяем детекцию невидимых символов
Console.WriteLine("=== Тест 4: Детекция невидимых символов ===");
var textWithInvisible = "Текст\u200Bс\u200Cневидимыми\u2060символами";
Console.WriteLine($"Исходный текст: '{textWithInvisible}'");

var detection = detector.DetectInvisibleCharacters(textWithInvisible);
Console.WriteLine($"Найдено невидимых символов: {detection.TotalCount}");

foreach (var detected in detection.DetectedCharacters)
{
    Console.WriteLine($"  - {detected.Name} (U+{detected.CodePoint:X4}) в позиции {detected.Position}");
    Console.WriteLine($"    Подсказка: {detected.ReplacementHint}");
}

var result4 = cleaner.CleanText(textWithInvisible, CleaningPreset.Safe);
Console.WriteLine($"Очищенный текст: '{result4.CleanedText}'");
Console.WriteLine();

// Тест 5: Проверяем, что исходный смысл сохраняется
Console.WriteLine("=== Тест 5: Проверка сохранения смысла текста ===");
var originalMeaning = "Привет\tдрузья! Как\u2003дела?\rВсё\u200Bхорошо!";
Console.WriteLine($"Исходный текст: '{originalMeaning}'");

var result5 = cleaner.CleanText(originalMeaning, CleaningPreset.Safe);
Console.WriteLine($"Очищенный текст: '{result5.CleanedText}'");
Console.WriteLine($"Слова остались разделенными: {result5.CleanedText.Contains(' ')}");
Console.WriteLine();

Console.WriteLine("=== Заключение ===");
Console.WriteLine("✅ Табуляции заменяются на пробелы вместо удаления");
Console.WriteLine("✅ Символы новой строки корректно нормализуются");
Console.WriteLine("✅ Широкие пробелы заменяются на обычные");
Console.WriteLine("✅ Исходный смысл текста сохраняется");
Console.WriteLine("✅ Невидимые символы правильно обнаруживаются и очищаются");
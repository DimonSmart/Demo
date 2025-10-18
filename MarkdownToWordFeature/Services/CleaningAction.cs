using System.Text;

namespace MarkdownToWordFeature.Services
{
    /// <summary>
    /// Defines the action to take when cleaning an invisible character
    /// </summary>
    public enum CleaningActionType
    {
        /// <summary>
        /// Remove the character completely
        /// </summary>
        Remove,
        
        /// <summary>
        /// Replace with a specific string
        /// </summary>
        Replace,
        
        /// <summary>
        /// Keep the character as-is (for safe mode)
        /// </summary>
        Keep,
        
        /// <summary>
        /// Normalize to standard form (e.g., CR+LF to LF)
        /// </summary>
        Normalize,
        
        /// <summary>
        /// Conditional action based on context
        /// </summary>
        Conditional
    }

    /// <summary>
    /// Represents a specific cleaning action for an invisible character
    /// </summary>
    public class CleaningAction
    {
        public CleaningActionType ActionType { get; init; }
        public string? ReplacementText { get; init; }
        public string? Description { get; init; }
        
        /// <summary>
        /// For conditional actions - additional context needed for decision
        /// </summary>
        public Func<CleaningContext, string?>? ConditionalResolver { get; init; }

        public static CleaningAction Remove(string? description = null) => 
            new() { ActionType = CleaningActionType.Remove, Description = description };

        public static CleaningAction Replace(string replacementText, string? description = null) => 
            new() { ActionType = CleaningActionType.Replace, ReplacementText = replacementText, Description = description };

        public static CleaningAction Keep(string? description = null) => 
            new() { ActionType = CleaningActionType.Keep, Description = description };

        public static CleaningAction Normalize(string normalizedText, string? description = null) => 
            new() { ActionType = CleaningActionType.Normalize, ReplacementText = normalizedText, Description = description };

        public static CleaningAction Conditional(Func<CleaningContext, string?> resolver, string? description = null) => 
            new() { ActionType = CleaningActionType.Conditional, ConditionalResolver = resolver, Description = description };
    }

    /// <summary>
    /// Context information for conditional cleaning actions
    /// </summary>
    public class CleaningContext
    {
        public required string OriginalText { get; init; }
        public required int Position { get; init; }
        public required Rune CurrentCharacter { get; init; }
        public Rune? PreviousCharacter { get; init; }
        public Rune? NextCharacter { get; init; }
        public CleaningPreset Preset { get; init; }
        public CleaningOptions Options { get; init; } = new();
        public bool IsInCodeBlock { get; init; }
    }
}
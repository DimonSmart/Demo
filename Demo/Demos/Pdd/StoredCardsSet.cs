namespace Demo.Demos.Pdd
{
    public class StoredCardsSet
    {
        public string Version { get; set; } = "1.0";
        public IReadOnlyCollection<QuestionStudyCard> Cards { get; set; } = [];
    }

}
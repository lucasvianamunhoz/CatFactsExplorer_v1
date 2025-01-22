namespace CatFactsExplorer.Domain.Entities
{
    public class CatFact
    {
        public int Id { get; private set; }
        public string Fact { get; private set; }
        public int Length { get; private set; }

        // Construtor para EF
        private CatFact() { }

        public CatFact(string fact, int length)
        {
            Fact = fact;
            Length = length;
        }
    }
}
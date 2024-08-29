using Demo.Demos.TSM.GeneralGenetic;

namespace Demo.Demos.TSM
{
    public class TsmProblemDataFactory
    {
        private readonly IRandomProvider _randomProvider;

        public TsmProblemDataFactory(IRandomProvider randomProvider)
        {
            _randomProvider = randomProvider;
        }

        public TsmProblemData Create (int citiesCount, int mapWidth, int mapHeight)
        {
            var cities = new City[citiesCount];
            for (var i = 0; i < citiesCount; i++)
                cities[i] = new City(_randomProvider.Next(mapWidth), _randomProvider.Next(mapHeight));
            return new TsmProblemData(cities, mapWidth, mapHeight);
        }
    }
}

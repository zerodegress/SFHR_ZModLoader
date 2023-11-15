#nullable enable

namespace SFHR_ZModLoader
{
    public class GameContext
    {
        public GlobalData GlobalData { get; set; }
        public SD SaveData { get; set; }

        public GameContext(GlobalData gd, SD sd)
        {
            GlobalData = gd;
            SaveData = sd;
        }
    }
}
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;

namespace AimBot.Utilities
{
    public class Player
    {
        public static EntityWrapper Entity     => BasePlugin.API.GameController.Player;
        public static long          Experience => Entity.GetComponent<PoeHUD.Poe.Components.Player>().XP;
        public static string        Name       => Entity.GetComponent<PoeHUD.Poe.Components.Player>().PlayerName;
        public static float         X          => Entity.GetComponent<Render>().X;
        public static float         Y          => Entity.GetComponent<Render>().Y;
        public static int           Level      => Entity.GetComponent<PoeHUD.Poe.Components.Player>().Level;
        public static Life          Health     => Entity.GetComponent<Life>();
        public static AreaInstance  Area       => BasePlugin.API.GameController.Area.CurrentArea;
        public static int           AreaHash   => BasePlugin.API.GameController.Game.IngameState.Data.CurrentAreaHash;

        public static bool HasBuff(string buffName) => Entity.GetComponent<Life>().HasBuff(buffName);
    }
}
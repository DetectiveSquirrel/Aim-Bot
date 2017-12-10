using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace Aimbot.Core
{
    public class Settings : SettingsBase
    {
        public Settings()
        {
            AimKey = Keys.A;
            AimRange = new RangeNode<int>(600, 1, 1000);
            AimLoopDelay = new RangeNode<int>(124, 1, 200);
            AimUniqueFirst = true;
            RMousePos = false;
        }

        [Menu("Aim Key")]
        public HotkeyNode AimKey { get; set; }

        [Menu("Aim Radius")]
        public RangeNode<int> AimRange { get; set; }

        [Menu("Aim Loop Delay")]
        public RangeNode<int> AimLoopDelay { get; set; }

        [Menu("Aim Highest Rarity First")]
        public ToggleNode AimUniqueFirst { get; set; }
        
        [Menu("Restore Mouse Position")]
        public ToggleNode RMousePos { get; set; }
    }
}

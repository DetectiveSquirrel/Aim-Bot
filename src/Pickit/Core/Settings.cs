using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace Aimbot.Core
{
    public class Settings : SettingsBase
    {
        public Settings()
        {
            AimKey         = Keys.A;
            AimRange       = new RangeNode<int>(600, 1, 1000);
            AimLoopDelay   = new RangeNode<int>(124, 1, 200);
            AimUniqueFirst = true;
            RMousePos      = false;
        }
        
        public HotkeyNode AimKey { get; set; }
        public RangeNode<int> AimRange { get; set; }
        public RangeNode<int> AimLoopDelay { get; set; }
        public ToggleNode AimUniqueFirst { get; set; }
        public ToggleNode RMousePos { get; set; }

        public ToggleNode AimPlayers { get; set; } = true;
    }
}
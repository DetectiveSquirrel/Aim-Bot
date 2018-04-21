using System.Reflection;
using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace Aimbot.Core
{
    public class Settings : SettingsBase
    {
        public HotkeyNode AimKey { get; set; } = Keys.A;
        public RangeNode<int> AimRange { get; set; } = new RangeNode<int>(600, 1, 1000);
        public RangeNode<int> AimLoopDelay { get; set; } = new RangeNode<int>(124, 1, 200);
        public ToggleNode RMousePos { get; set; } = false;
        public ToggleNode AimPlayers { get; set; } = true;
        public ToggleNode DebugMonsterWeight { get; set; } = false;
        public RangeNode<int> UniqueRarityWeight { get; set; } = new RangeNode<int>(20, -200, 200);
        public RangeNode<int> RareRarityWeight { get; set; } = new RangeNode<int>(15, -200, 200);
        public RangeNode<int> MagicRarityWeight { get; set; } = new RangeNode<int>(10, -200, 200);
        public RangeNode<int> NormalRarityWeight { get; set; } = new RangeNode<int>(5, -200, 200);
        public RangeNode<int> CannotDieAura { get; set; } = new RangeNode<int>(100, -200, 200);
        public RangeNode<int> capture_monster_trapped { get; set; } = new RangeNode<int>(200, -200, 200);
        public RangeNode<int> capture_monster_enraged { get; set; } = new RangeNode<int>(-50, -200, 200);
        public RangeNode<int> BeastHearts { get; set; } = new RangeNode<int>(80, -200, 200);
        public RangeNode<int> TukohamaShieldTotem { get; set; } = new RangeNode<int>(70, -200, 200);
        public RangeNode<int> StrongBoxMonster { get; set; } = new RangeNode<int>(25, -200, 200);
        public RangeNode<int> SummonedSkeoton { get; set; } = new RangeNode<int>(-30, -200, 200);
        public RangeNode<int> RaisedZombie { get; set; } = new RangeNode<int>(-30, -200, 200);
        public RangeNode<int> LightlessGrub { get; set; } = new RangeNode<int>(-30, -200, 200);
        public RangeNode<int> TaniwhaTail { get; set; } = new RangeNode<int>(-40, -200, 200);
        public RangeNode<int> DiesAfterTime { get; set; } = new RangeNode<int>(-50, -200, 200);
    }
}
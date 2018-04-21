using System.Reflection;
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

            NormalRarityWeight = new RangeNode<int>(5, 0, 50);
            MagicRarityWeight = new RangeNode<int>(10, 0, 50);
            RareRarityWeight = new RangeNode<int>(15, 0, 50);
            UniqueRarityWeight = new RangeNode<int>(20, 0, 50);
        }


        public HotkeyNode AimKey { get; set; }
        public RangeNode<int> AimRange { get; set; }
        public RangeNode<int> AimLoopDelay { get; set; }
        public ToggleNode AimUniqueFirst { get; set; }
        public ToggleNode RMousePos { get; set; }

        public ToggleNode AimPlayers { get; set; } = true;

        public ToggleNode DebugMonsterWeight { get; set; } = false;


        [Menu("Rarity Weight Settings", 1001)]
        public EmptyNode EmptyNode1 { get; set; }

        [Menu("Normal", 10011, 1001)]
        public EmptyNode EmptyNodeNormal { get; set; }
        [Menu("Rarity Weight", 100111, 10011)]
        public RangeNode<int> NormalRarityWeight { get; set; }

        [Menu("Magic", 10012, 1001)]
        public EmptyNode EmptyNodeMagic { get; set; }
        [Menu("Rarity Weight", 100121, 10012)]
        public RangeNode<int> MagicRarityWeight { get; set; }

        [Menu("Rare", 10013, 1001)]
        public EmptyNode EmptyNodeRare { get; set; }
        [Menu("Rarity Weight", 100131, 10013)]
        public RangeNode<int> RareRarityWeight { get; set; }

        [Menu("Unique", 10014, 1001)]
        public EmptyNode EmptyNodeUnique { get; set; }
        [Menu("Rarity Weight", 100141, 10014)]
        public RangeNode<int> UniqueRarityWeight { get; set; }
    }
}
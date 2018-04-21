#region Header

/*
 * Idea/Code from Qvin's auto pickup
 * Reworked into a monster aimer
*/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using AimBot.Utilities;
using ImGuiNET;
using PoeHUD.Framework.Helpers;
using PoeHUD.Models;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using SharpDX.Direct3D9;
using Player = AimBot.Utilities.Player;

namespace Aimbot.Core
{
    public class Main : BaseSettingsPlugin<Settings>
    {
        private const int PixelBorder = 3;
        private readonly Stopwatch _aimTimer = Stopwatch.StartNew();
        private readonly List<EntityWrapper> _entities = new List<EntityWrapper>();
        private bool _aiming;
        private Vector2 _clickWindowOffset;
        private bool _mouseWasHeldDown;
        private Vector2 _oldMousePos;
        public HashSet<string> IgnoredMonsters;

        //https://stackoverflow.com/questions/826777/how-to-have-an-auto-incrementing-version-number-visual-studio
        public Version version = Assembly.GetExecutingAssembly().GetName().Version;
        public string PluginVersion;
        public DateTime buildDate;

        public Main() => PluginName = "Aim Bot";

        public override void Initialise()
        {
            buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
            PluginVersion = $"{version}";
            IgnoredMonsters = LoadFile("Ignored Monsters");
        }

        public override void Render()
        {
            base.Render();
            WeightDebug();
            try
            {
                if (Keyboard.IsKeyDown((int)Settings.AimKey.Value)
                 && !GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible
                 && !GameController.Game.IngameState.IngameUi.OpenLeftPanel.IsVisible)
                {
                    if (_aiming)
                        return;
                    _aiming = true;
                    Aimbot();
                }
                else
                {
                    if (!_mouseWasHeldDown)
                        return;
                    _mouseWasHeldDown = false;
                    if (Settings.RMousePos)
                        Mouse.SetCursorPos(_oldMousePos);
                }
            }
            catch (Exception e)
            {
                LogError("Something went wrong? " + e, 5);
            }
        }

        private void WeightDebug()
        {
            if (!Settings.DebugMonsterWeight) return;
            foreach (var entity in GameController.Entities)
            {
                if (entity.DistanceFromPlayer < Settings.AimRange && entity.HasComponent<Monster>() && entity.IsAlive)
                {
                    var camera = GameController.Game.IngameState.Camera;
                    var chestScreenCoords = camera.WorldToScreen(entity.Pos.Translate(0, 0, -170), entity);
                    if (chestScreenCoords == new Vector2())
                    {
                        continue;
                    }

                    var iconRect = new Vector2(chestScreenCoords.X, chestScreenCoords.Y);
                    float maxWidth = 0;
                    float maxheight = 0;
                    var size = Graphics.DrawText(AimWeightEB(entity).ToString(), 20, iconRect, Color.White, FontDrawFlags.Center);
                    chestScreenCoords.Y += size.Height;
                    maxheight += size.Height;
                    maxWidth = Math.Max(maxWidth, size.Width);
                    var background = new RectangleF(chestScreenCoords.X - maxWidth / 2 - 3, chestScreenCoords.Y - maxheight, maxWidth + 6, maxheight);
                    Graphics.DrawBox(background, Color.Black);
                }
            }
        }

        public override void DrawSettingsMenu()
        {
            ImGui.BulletText($"v{PluginVersion}");
            ImGui.BulletText($"Last Updated: {buildDate}");
            Settings.AimRange.Value = ImGuiExtension.IntSlider("Target Distance", Settings.AimRange);
            Settings.AimLoopDelay.Value = ImGuiExtension.IntSlider("Target Delay ms", Settings.AimLoopDelay);
            //Settings.AimUniqueFirst.Value = ImGuiExtension.Checkbox("Aim Highest Rarity First", Settings.AimUniqueFirst.Value);
            Settings.DebugMonsterWeight.Value = ImGuiExtension.Checkbox("Draw Weight Results On Monsters", Settings.DebugMonsterWeight.Value);
            Settings.RMousePos.Value = ImGuiExtension.Checkbox("Restore Mouse Position After Letting Go Of Auto Aim Hotkey", Settings.RMousePos.Value);
            Settings.AimKey.Value = ImGuiExtension.HotkeySelector("Auto Aim Hotkey", "Auto Aim Popup", Settings.AimKey.Value);
            Settings.AimPlayers.Value = ImGuiExtension.Checkbox("Aim Players Instead?", Settings.AimPlayers.Value);

            base.DrawSettingsMenu();
        }

        public override void EntityAdded(EntityWrapper entityWrapper) { _entities.Add(entityWrapper); }

        public override void EntityRemoved(EntityWrapper entityWrapper) { _entities.Remove(entityWrapper); }

        public HashSet<string> LoadFile(string fileName)
        {
            var file = $@"{PluginDirectory}\{fileName}.txt";
            if (!File.Exists(file))
            {
                LogError($@"Failed to find {file}", 10);
                return null;
            }

            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lines = File.ReadAllLines(file);
            lines.Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")).ForEach(x => hashSet.Add(x.Trim()));
            return hashSet;
        }

        private bool IsIgnoredMonster(string path) { return IgnoredMonsters.Any(ignoreString => path.ToLower().Contains(ignoreString.ToLower())); }

        private void Aimbot()
        {
            if (_aimTimer.ElapsedMilliseconds < Settings.AimLoopDelay)
            {
                _aiming = false;
                return;
            }

            if (Settings.AimPlayers)
            {
                PlayerAim();
            }
            else
            {
                MonsterAim();
            }

            _aimTimer.Restart();

            _aiming = false;
        }

        private int TryGetStat(GameStat stat) => GameController.EntityListWrapper.PlayerStats.TryGetValue(stat, out var statInt) ? statInt : 0;

        private int TryGetStat(GameStat stat, EntityWrapper entity)
        {
            return entity.GetComponent<Stats>().StatDictionary.TryGetValue(stat, out var statInt) ? statInt : 0;
        }

        private void PlayerAim()
        {
            var AlivePlayers = _entities
                              .Where(x => x.HasComponent<PoeHUD.Poe.Components.Player>()
                                       && x.IsAlive
                                       && x.Address != Player.Entity.Address
                                       && TryGetStat(GameStat.IgnoredByEnemyTargetSelection, x) != 1
                                       && TryGetStat(GameStat.CannotDie, x) != 1)
                              .Select(x => new Tuple<float, EntityWrapper>(Misc.EntityDistance(x), x))
                              .OrderBy(x => x.Item1)
                              .ToList();
            var closestMonster = AlivePlayers.FirstOrDefault(x => x.Item1 < Settings.AimRange);
            if (closestMonster != null)
            {
                if (!_mouseWasHeldDown)
                {
                    _oldMousePos = Mouse.GetCursorPositionVector();
                    _mouseWasHeldDown = true;
                }

                if (closestMonster.Item1 >= Settings.AimRange)
                {
                    _aiming = false;
                    return;
                }

                var camera = GameController.Game.IngameState.Camera;
                var entityPosToScreen = camera.WorldToScreen(closestMonster.Item2.Pos.Translate(0, 0, 0), closestMonster.Item2);
                var vectWindow = GameController.Window.GetWindowRectangle();
                if (entityPosToScreen.Y + PixelBorder > vectWindow.Bottom || entityPosToScreen.Y - PixelBorder < vectWindow.Top)
                {
                    _aiming = false;
                    return;
                }

                if (entityPosToScreen.X + PixelBorder > vectWindow.Right || entityPosToScreen.X - PixelBorder < vectWindow.Left)
                {
                    _aiming = false;
                    return;
                }

                _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                Mouse.SetCursorPos(entityPosToScreen + _clickWindowOffset);
            }
        }

        public bool HasAnyBuff(EntityWrapper entity, string[] buffList)
        {
            foreach (var buff in entity.GetComponent<Life>().Buffs)
            {
                if (buffList.Any(searchedBuff => searchedBuff == buff.Name))
                {
                    return true;
                }
            }
            return false;
        }
        private void MonsterAim()
        {
            var aliveAndHostile = _entities.Where(x => x.HasComponent<Monster>()
                                                    && x.IsAlive
                                                    && x.IsHostile
                                                    && !IsIgnoredMonster(x.Path)
                                                    && !HasAnyBuff(x, new[]
                                                       {
                                                               "capture_monster_captured",
                                                               "capture_monster_disappearing"
                                                       }))
                                           .Select(x => new Tuple<float, EntityWrapper>(AimWeightEB(x), x))
                                           .OrderByDescending(x => x.Item1)
                                           .ToList();

            //var aliveAndHostile = _entities.Where(x => x.HasComponent<PoeHUD.Poe.Components.Player>() && x.IsAlive && x.GetComponent<PoeHUD.Poe.Components.Player>().PlayerName != Player.Name)
            //                                         .Select(x => new Tuple<int, EntityWrapper>(Misc.EntityDistance(x), x))
            //                                         .OrderBy(x => x.Item1)
            //                                         .ToList();
            if (aliveAndHostile.FirstOrDefault(x => x.Item1 < Settings.AimRange) != null)
            {
                var HeightestWeightedTarget = aliveAndHostile.FirstOrDefault(x => x.Item1 < Settings.AimRange);
                if (!_mouseWasHeldDown)
                {
                    _oldMousePos = Mouse.GetCursorPositionVector();
                    _mouseWasHeldDown = true;
                }

                if (HeightestWeightedTarget.Item1 >= Settings.AimRange)
                {
                    _aiming = false;
                    return;
                }

                var camera = GameController.Game.IngameState.Camera;
                var entityPosToScreen = camera.WorldToScreen(HeightestWeightedTarget.Item2.Pos.Translate(0, 0, 0), HeightestWeightedTarget.Item2);
                var vectWindow = GameController.Window.GetWindowRectangle();
                if (entityPosToScreen.Y + PixelBorder > vectWindow.Bottom || entityPosToScreen.Y - PixelBorder < vectWindow.Top)
                {
                    _aiming = false;
                    return;
                }

                if (entityPosToScreen.X + PixelBorder > vectWindow.Right || entityPosToScreen.X - PixelBorder < vectWindow.Left)
                {
                    _aiming = false;
                    return;
                }

                _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                Mouse.SetCursorPos(entityPosToScreen + _clickWindowOffset);
            }
        }

        public string[] SummonedSkeleton = {
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonStandard",
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonStatue",
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonMannequin",
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonStatueMale",
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonStatueGold",
                "Metadata/Monsters/RaisedSkeletons/RaisedSkeletonStatueGoldMale",
                "Metadata/Monsters/RaisedSkeletons/NecromancerRaisedSkeletonStandard",
                "Metadata/Monsters/RaisedSkeletons/TalismanRaisedSkeletonStandard"
        };

        public string[] RaisedZombie = {
                "Metadata/Monsters/RaisedZombies/RaisedZombieStandard",
                "Metadata/Monsters/RaisedZombies/RaisedZombieMummy",
                "Metadata/Monsters/RaisedZombies/NecromancerRaisedZombieStandard"
        };

        public string[] LightlessGrub = {
                "Metadata/Monsters/HuhuGrub/AbyssGrubMobile",
                "Metadata/Monsters/HuhuGrub/AbyssGrubMobileMinion",
        };

        public float AimWeightEB(EntityWrapper entity)
        {
            var m = entity;
            var weight = 0;
            weight -= m.DistanceFromPlayer / 10;
            var rarity = m.GetComponent<ObjectMagicProperties>().Rarity;
            if (m.GetComponent<Life>().HasBuff("monster_aura_cannot_die")) weight += 100;
            if (m.GetComponent<Life>().HasBuff("capture_monster_trapped")) weight += 200;
            if (m.GetComponent<Life>().HasBuff("capture_monster_enraged")) weight -= 50;
            if (m.Path.Contains("/BeastHeart")) weight += 80;
            if (m.Path == "Metadata/Monsters/Tukohama/TukohamaShieldTotem") weight += 70;
            if (m.HasComponent<ObjectMagicProperties>())
                foreach (var mod in m.GetComponent<ObjectMagicProperties>().Mods)
                {
                    if (mod == "MonsterRaisesUndeadText") weight += 30;
                }

            // Experimental, seems like a buff only strongbox monsters get
            if (HasAnyBuff(m, new[]
            {
                    "summoned_monster_epk_buff"
            }))
            {
                weight += 25;
            }

            switch (rarity)
            {
                case MonsterRarity.Unique:
                    weight += Settings.UniqueRarityWeight;
                    break;
                case MonsterRarity.Rare:
                    weight += Settings.RareRarityWeight;
                    break;
                case MonsterRarity.Magic:
                    weight += Settings.MagicRarityWeight;
                    break;
                case MonsterRarity.White:
                    weight += Settings.NormalRarityWeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (SummonedSkeleton.Any(path => m.Path == path)) weight -= 30;
            if (RaisedZombie.Any(path => m.Path == path)) weight -= 30;
            if (LightlessGrub.Any(path => m.Path == path)) weight -= 30;
            if (m.Path.Contains("TaniwhaTail")) weight -= 40;
            if (m.HasComponent<DiesAfterTime>()) weight -= 50;
            return weight;
        }
    }
}
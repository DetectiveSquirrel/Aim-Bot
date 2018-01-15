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
using AimBot.Utilities;
using PoeHUD.Framework.Helpers;
using PoeHUD.Models;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using SharpDX;

namespace Aimbot.Core
{
    public class Main : BaseSettingsPlugin<Settings>
    {
        private const    int                 PixelBorder = 3;
        private readonly Stopwatch           _aimTimer   = Stopwatch.StartNew();
        private readonly List<EntityWrapper> _entities   = new List<EntityWrapper>();
        private          bool                _aiming;
        private          Vector2             _clickWindowOffset;
        private          bool                _mouseWasHeldDown;
        private          Vector2             _oldMousePos;
        public           HashSet<string>     IgnoredMonsters;

        public Main() => PluginName = "Aim Bot";

        public override void Initialise() { IgnoredMonsters = LoadFile("Ignored Monsters"); }

        public override void Render()
        {
            base.Render();
            try
            {
                if (Keyboard.IsKeyDown((int) Settings.AimKey.Value)
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
            catch
            {
                LogError("Something went wrong?", 5);
            }
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
            var lines   = File.ReadAllLines(file);
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

            _aimTimer.Restart();
            var aliveAndHostile = Settings.AimUniqueFirst
                                          ? _entities.Where(x => x.HasComponent<Monster>() && x.IsAlive && x.IsHostile && !IsIgnoredMonster(x.Path))
                                                     .Select(x => new Tuple<int, EntityWrapper, MonsterRarity>(Misc.EntityDistance(x), x, x.GetComponent<ObjectMagicProperties>().Rarity))
                                                     .OrderByDescending(x => x.Item3)
                                                     .ThenBy(x => x.Item1)
                                                     .ToList()
                                          : _entities.Where(x => x.HasComponent<Monster>() && x.IsAlive && x.IsHostile && !IsIgnoredMonster(x.Path))
                                                     .Select(x => new Tuple<int, EntityWrapper, MonsterRarity>(Misc.EntityDistance(x), x, x.GetComponent<ObjectMagicProperties>().Rarity))
                                                     .OrderBy(x => x.Item1)
                                                     .ToList();
            var closestMonster = aliveAndHostile.FirstOrDefault(x => x.Item1 < Settings.AimRange);
            if (closestMonster != null)
            {
                if (!_mouseWasHeldDown)
                {
                    _oldMousePos      = Mouse.GetCursorPositionVector();
                    _mouseWasHeldDown = true;
                }

                if (closestMonster.Item1 >= Settings.AimRange)
                {
                    _aiming = false;
                    return;
                }

                var camera            = GameController.Game.IngameState.Camera;
                var entityPosToScreen = camera.WorldToScreen(closestMonster.Item2.Pos.Translate(0, 0, 0), closestMonster.Item2);
                var vectWindow        = GameController.Window.GetWindowRectangle();
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

            _aiming = false;
        }
    }
}
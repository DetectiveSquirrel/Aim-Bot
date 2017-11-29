using System;
using PoeHUD.Models;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace AimBot.Utilities
{
    public class Misc
    {
        public static int EntityDistance(EntityWrapper entity)
        {
            var Object = entity.GetComponent<Positioned>();
            return (int) Math.Sqrt(Math.Pow(Player.X - Object.X, 2) + Math.Pow(Player.Y - Object.Y, 2));
        }

        public static int EntityDistance(Entity entity)
        {
            var Object = entity.GetComponent<Positioned>();
            return (int) Math.Sqrt(Math.Pow(Player.X - Object.X, 2) + Math.Pow(Player.Y - Object.Y, 2));
        }
    }
}
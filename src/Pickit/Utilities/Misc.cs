using System;
using PoeHUD.Models;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using SharpDX;

namespace AimBot.Utilities
{
    public class Misc
    {
        //public static int EntityDistance(EntityWrapper entity)
        //{
        //    var Object = entity.GetComponent<Render>();
        //    return (int) Math.Sqrt(Math.Pow(Player.X - Object.X, 2) + Math.Pow(Player.Y - Object.Y, 2));
        //}

        public static float EntityDistance(EntityWrapper entity)
        {
            var Object = entity.GetComponent<Render>();
            return Vector2.Distance(new Vector2(Player.X, Player.Y), new Vector2(Object.X, Object.Y));
        }

        //public static int EntityDistance(Entity entity)
        //{
        //    var Object = entity.GetComponent<Render>();
        //    return (int)Math.Sqrt(Math.Pow(Player.X - Object.X, 2) + Math.Pow(Player.Y - Object.Y, 2));
        //}

        public static float EntityDistance(Entity entity)
        {
            var Object = entity.GetComponent<Render>();
            return Vector2.Distance(new Vector2(Player.X, Player.Y), new Vector2(Object.X, Object.Y));
        }

        public static int GetEntityDistance(Vector2 firstPos, Vector2 secondPos)
        {
            var distanceToEntity = Math.Sqrt(Math.Pow(firstPos.X - secondPos.X, 2) + Math.Pow(firstPos.Y - secondPos.Y, 2));
            return (int) distanceToEntity;
        }
    }
}
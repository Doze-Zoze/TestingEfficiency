using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TestingEfficiency.Helpers
{
    internal static class Helpers
    {
        static Player player => Main.LocalPlayer;
        public static int playerLifeTotal
        {
            get
            {
                return 100 + player.ConsumedLifeCrystals * 20 + player.ConsumedLifeFruit * 5 + calLifeUsed * 25;
            }
            set
            {
                if (value <= 100)
                {
                    player.ConsumedLifeCrystals = 0;
                    player.ConsumedLifeFruit = 0;
                    calLifeUsed = 0;
                }
                else if (value <= 400)
                {
                    player.ConsumedLifeCrystals = (value - 100) / 20;
                    player.ConsumedLifeFruit = 0;
                    calLifeUsed = 0;
                }
                else if (value <= 500)
                {

                    player.ConsumedLifeCrystals = 15;
                    player.ConsumedLifeFruit = (value - 400) / 5;
                    calLifeUsed = 0;
                }
                else
                {

                    player.ConsumedLifeCrystals = 15;
                    player.ConsumedLifeFruit = 20;
                    calLifeUsed = (value - 500) / 25;
                }
            }
        }

        public static int playerManaTotal
        {
            get
            {

                return player.ConsumedManaCrystals * 20 + 20 + 50 * calManaUsed;
            }
            set
            {

                if (value <= 20)
                {
                    player.ConsumedManaCrystals = 0;
                    calManaUsed = 0;
                }
                else if (value <= 200)
                {

                    player.ConsumedManaCrystals = (value - 20) / 20;
                    calManaUsed = 0;
                }
                else
                {

                    player.ConsumedManaCrystals = 9;
                    calManaUsed = (value - 200) / 50;
                }
            }
        }
        #region Calamity Support
        public static int calLifeUsed
        {
            get
            {
                int count = 0;
                if (CalHpOne) //bOrange - sTangerine
                    count++;
                if (CalHp2) //mFruit
                    count++;
                if (CalHp3) //bOrange - eBerry
                    count++;
                if (CalHp4) //bOrange - dFruit
                    count++;
                return count;
            }
            set
            {

                CalHpOne = value > 0;
                CalHp2 = value > 1;
                CalHp3 = value > 2;
                CalHp4 = value > 3;
            }
        }

        public static int calManaUsed
        {
            get
            {
                int count = 0;
                if (CalMana1)
                    count++;
                if (CalMana2)
                    count++;
                if (CalMana3)
                    count++;
                return count;
            }
            set
            {
                CalMana1 = value > 0;
                CalMana2 = value > 1;
                CalMana3 = value > 2;
            }
        }

        public static int rageBoostUsed
        {
            get
            {

                int count = 0;
                if (CalRage1)
                    count++;
                if (CalRage2)
                    count++;
                if (CalRage3)
                    count++;
                return count;
            }
            set
            {

                CalRage1 = value > 0;
                CalRage2 = value > 1;
                CalRage3 = value > 2;
            }
        }
        public static int adrenBoostUsed
        {
            get
            {

                int count = 0;
                if (CalAdren1)
                    count++;
                if (CalAdren2)
                    count++;
                if (CalAdren3)
                    count++;
                return count;
            }
            set
            {
                CalAdren1 = value > 0;
                CalAdren2 = value > 1;
                CalAdren3 = value > 2;
            }
        }
        internal static bool CalHpOne
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "SanguineTangerine");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "SanguineTangerine", value); }
        }
        internal static bool CalHp2
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "MiracleFruit");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "MiracleFruit", value); }
        }
        internal static bool CalHp3
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "TaintedCloudberry");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "TaintedCloudberry", value); }
        }
        internal static bool CalHp4
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "SacredStrawberry");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "SacredStrawberry", value); }
        }

        internal static bool CalMana1
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "CometShard");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "CometShard", value); }
        }

        internal static bool CalMana2
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "EtherealCore");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "EtherealCore", value); }
        }

        internal static bool CalMana3
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "PhantomHeart");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "PhantomHeart", value); }
        }

        internal static bool CalRage1
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "MushroomPlasmaRoot");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "MushroomPlasmaRoot", value); }
        }
        internal static bool CalRage2
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "InfernalBlood");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "InfernalBlood", value); }
        }
        internal static bool CalRage3
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "RedLightningContainer");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "RedLightningContainer", value); }
        }
        internal static bool CalAdren1
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "ElectrolyteGelPack");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "ElectrolyteGelPack", value); }
        }
        internal static bool CalAdren2
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "StarlightFuelCell");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "StarlightFuelCell", value); }
        }
        internal static bool CalAdren3
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "Ectoheart");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "Ectoheart", value); }
        }
        internal static bool CalAccessory
        {
            get => !TestingEfficiency.CalamityLoaded ? false : (bool)TestingEfficiency.CalamityMod.Call("GetPowerup", player, "CelestialOnion");
            set { if (TestingEfficiency.CalamityLoaded) TestingEfficiency.CalamityMod.Call("SetPowerup", player, "CelestialOnion", value); }
        }

        #endregion
        public static void SetTexture(this UIImageButton e, string texture)
        {
            if (e == null)
                e = new UIImageButton(ModContent.Request<Texture2D>(texture));
        }
        public static void SetTexture(this UIImageButton e, Asset<Texture2D> texture)
        {
            if (e == null)
                e = new UIImageButton(texture);
        }

        public static void SetTexture(this UIImage e, string texture)
        {
            if (e == null)
                e = new UIImage(ModContent.Request<Texture2D>(texture));
        }
        public static void SetTexture(this UIImage e, Asset<Texture2D> texture)
        {
            if (e == null)
                e = new UIImage(texture);
        }

        public static void SetImageAndSize(this UIImage e, Asset<Texture2D> texture)
        {
            e.SetImage(texture);
            e.Width.Set(texture.Width(), 0);
            e.Height.Set(texture.Height(), 0);
        }

        public static void Invert(this ref bool b) => b = !b;

        public static void ToggleChild(this UIElement parent, UIElement child)
        {
            if (parent.Children.Contains(child))
                parent.RemoveChild(child);
            else
                parent.Append(child);
        }
    }
}

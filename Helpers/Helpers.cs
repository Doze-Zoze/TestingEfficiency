using CalamityMod;
using CalamityMod.NPCs.AcidRain;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
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
                return 100 + player.ConsumedLifeCrystals*20 + player.ConsumedLifeFruit*5 + calLifeUsed*25;
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
                    player.ConsumedLifeCrystals = (value-100)/20;
                    player.ConsumedLifeFruit = 0;
                    calLifeUsed = 0;
                }
                else if (value <= 500)
                {

                    player.ConsumedLifeCrystals = 15;
                    player.ConsumedLifeFruit = (value-400)/5;
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
        public static int calLifeUsed
        {
            get
            {
                int count = 0;
                if (player.Calamity().sTangerine) //bOrange - sTangerine
                    count++;
                if (player.Calamity().mFruit) //mFruit
                    count++;
                if (player.Calamity().tCloudberry) //bOrange - eBerry
                    count++;
                if (player.Calamity().sStrawberry) //bOrange - dFruit
                    count++;
                return count;
            }
            set
            {

                player.Calamity().sTangerine = value > 0;
                player.Calamity().mFruit = value > 1;
                player.Calamity().tCloudberry = value > 2;
                player.Calamity().sStrawberry = value > 3;
            }
        }

        public static int playerManaTotal
        {
            get
            {

                return player.ConsumedManaCrystals * 20 + 20 + 50*calManaUsed;
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

        public static int calManaUsed
        {
            get
            {
                int count = 0;
                if (player.Calamity().cShard)
                    count++;
                if (player.Calamity().eCore)
                    count++;
                if (player.Calamity().pHeart)
                    count++;
                return count;
            }
            set
            {
                player.Calamity().cShard = value > 0;
                player.Calamity().eCore = value > 1;
                player.Calamity().pHeart = value > 2;
            }
        }

        public static int rageBoostUsed
        {
            get
            {

                int count = 0;
                if (player.Calamity().rageBoostOne)
                    count++;
                if (player.Calamity().rageBoostTwo)
                    count++;
                if (player.Calamity().rageBoostThree)
                    count++;
                return count;
            }
            set
            {

                player.Calamity().rageBoostOne = value > 0;
                player.Calamity().rageBoostTwo = value > 1;
                player.Calamity().rageBoostThree = value > 2;
            }
        }
        public static int adrenBoostUsed
        {
            get
            {

                int count = 0;
                if (player.Calamity().adrenalineBoostOne)
                    count++;
                if (player.Calamity().adrenalineBoostTwo)
                    count++;
                if (player.Calamity().adrenalineBoostThree)
                    count++;
                return count;
            }
            set
            {
                player.Calamity().adrenalineBoostOne = value > 0;
                player.Calamity().adrenalineBoostTwo = value > 1;
                player.Calamity().adrenalineBoostThree = value > 2;
            }
        }
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

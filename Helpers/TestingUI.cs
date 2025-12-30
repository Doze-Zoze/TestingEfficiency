using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Tools;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TestingEfficiency.Commands;

namespace TestingEfficiency.Helpers
{
    public class TestingGUIManager : ModSystem
    {
        private UserInterface _testingGUI;

        internal TestingUI testingUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _testingGUI = new UserInterface();
                testingUI = new TestingUI();
                testingUI.Activate();
            }
        }

        public override void Unload()
        {
            testingUI = null;
            _testingGUI = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.playerInventory)
                _testingGUI.SetState(testingUI);
            else
                _testingGUI.SetState(null);

            if (_testingGUI?.CurrentState != null)
            {
                _testingGUI.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "TestingEfficiency:TestingUI",
                    delegate
                    {
                        _testingGUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                    );
            }
        }
    }

    public class TestingUI : UIState
    {
        UIElement activeChild;

        UIImageButtonBorder OpenMenuButton = new();

        #region Main Mennu
        UIPanel MainMenuPanel = new UIPanel();
        UIImageButtonBorder PermanentUpgradeMenuButton = new();
        UIImageButtonBorder BossToggleMenuButton = new();
        UIImageButtonBorder LoadoutMenuButton = new();
        UIImageButtonBorder DPSMenuButton = new();
        #endregion


        PermanentUpgrades PermanentUpgradePanel = new PermanentUpgrades();
        BossToggles BossTogglePanel = new BossToggles();
        Loadouts LoadoutPanel = new Loadouts();
        DPSDisplay DPSPaned = new();

        #region Toggle Bosses
        #endregion
        public override void OnInitialize()
        {
            OpenMenuButton.image = (TextureAssets.Item[ItemID.GoblinTech]);
            OpenMenuButton.Left.Set(0, 0.75f);
            OpenMenuButton.Top.Set(16, 0);
            OpenMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                this.ToggleChild(MainMenuPanel);
                if (activeChild != null)
                {
                    RemoveChild(activeChild);
                    activeChild = null;
                }

            };
            //Main Menu
            //Permanent Upgrades
            MainMenuPanel.Width = new(48 + MainMenuPanel.PaddingLeft + MainMenuPanel.PaddingRight, 0);
            MainMenuPanel.Height = new(52 * 4 - 4 + MainMenuPanel.PaddingTop + MainMenuPanel.PaddingBottom, 0);
            MainMenuPanel.Top.Set(16, 0);
            MainMenuPanel.Left.Set(-(MainMenuPanel.Width.Pixels + 4), 0.75f);


            PermanentUpgradeMenuButton.image = (TextureAssets.Item[ItemID.LifeCrystal]);
            PermanentUpgradeMenuButton.activePredicate = () => Children.Contains(PermanentUpgradePanel);
            PermanentUpgradeMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(PermanentUpgradePanel);
            };
            MainMenuPanel.Append(PermanentUpgradeMenuButton);

            BossToggleMenuButton.image = (TextureAssets.Item[ItemID.SuspiciousLookingEye]);
            BossToggleMenuButton.Top = new(48 + MainMenuPanel.MarginTop + 4, 0);
            BossToggleMenuButton.activePredicate = () => Children.Contains(BossTogglePanel);
            BossToggleMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(BossTogglePanel);
            };
            MainMenuPanel.Append(BossToggleMenuButton);

            LoadoutMenuButton.image = (TextureAssets.Item[ItemID.CopperShortsword]);
            LoadoutMenuButton.Top = new(52 * 2 + MainMenuPanel.MarginTop, 0);
            LoadoutMenuButton.activePredicate = () => Children.Contains(LoadoutPanel);
            LoadoutMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(LoadoutPanel);
            };
            MainMenuPanel.Append(LoadoutMenuButton);

            DPSMenuButton.image = (TextureAssets.Item[ItemID.DPSMeter]);
            DPSMenuButton.Top = new(52 * 3 + MainMenuPanel.MarginTop, 0);
            DPSMenuButton.activePredicate = () => Children.Contains(DPSPaned);
            DPSMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(DPSPaned);
            };
            MainMenuPanel.Append(DPSMenuButton);

            Append(OpenMenuButton);

        }

        public override void Update(GameTime gameTime)
        {
            foreach (var item in Children)
            {
                item.Update(gameTime);
            }
            SetPadding(4);
        }

        void ToggleGrandchild(UIElement child)
        {
            if (!Children.Contains(child))
            {
                if (activeChild != null)
                    RemoveChild(activeChild);
                Append(child);
                activeChild = child;
            }
            else if (activeChild != null)
            {
                RemoveChild(activeChild);
                activeChild = null;
            }
        }
    }
    public class DPSDisplay : UIPanel
    {

        bool initialized = false;

        public BossHealthGraph graph = new();

        public UIBasicTextbox time = new();

        public override void OnInitialize()
        {
            Width.Set(52 * 5 + 16, 0);
            Height.Set(52 * 2 + 45 * 5 + 24, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);
            Append(graph);

            time.textToUse = () => dpsSystem.lastIGT ?? "";
            time.Top.Set(120, 0);
            time.Width.Set(0, 1);
            time.Height.Set(24, 0);
            initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!initialized)
            {
                Initialize();

                foreach (var item in Children)
                {
                    item.Initialize();
                }
            }

            foreach (var item in Children)
            {
                item.Update(gameTime);
            }

            if (!Children.Contains(time))
                Append(time);

            Width.Set(400,0);
            time.Height.Set(170, 0);
            time.Width.Set(170, 0);
            time.HAlign = 0;
            time.text.VAlign = 0;
            time.Top.Set(100+12, 0);
            Height.Set(320,0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);
            graph.SetPadding(PaddingTop);
            graph.BackgroundColor = new Color(52, 66, 119);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
    public class BossHealthGraph : UIPanel
    {

        bool initialized = false;

        public BossHealthGraph()
        {
            Width.Set(0,1);
            Height.Set(100, 0);
            BackgroundColor = new Color(52, 66, 119);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var dim = GetInnerDimensions().ToRectangle();
            if (dpsSystem.lastPrint is not null)
            {
                spriteBatch.Draw(dpsSystem.lastPrint, dim, Color.White);
            }
        }
    }

    public class UIBasicTextbox : UIPanel
    {
        public UIText text = new("");

        public Func<string> textToUse = null;
        public UIBasicTextbox(string Text = "")
        {
            text.VAlign = 0.5f;
            text.HAlign = 0.5f;
            text.TextOriginX = 0.5f;
            text.TextOriginY = 0.5f;
            text.IgnoresMouseInteraction = true;
            text.SetText(Text);
            Append(text);
            BackgroundColor = Color.Lerp(BackgroundColor, Color.Black, 0.25f);
        }

        public override void Update(GameTime gameTime)
        {
            if (textToUse is not null)
            {
                text.SetText(textToUse.Invoke());
            }
        }
    }
    public class Loadouts : UIPanel
    {

        bool initialized = false;

        UIImageButtonBorder melee = new();
        UIImageButtonBorder ranged = new();
        UIImageButtonBorder magic = new();
        UIImageButtonBorder summon = new();
        UIImageButtonBorder rogue = new();

        UIImageButtonBorder addNew = new();
        UITextInput addInput = new();
        UIList loadouts = new();
        UIScrollbar scrollbar = new();

        int active = -1;

        public override void OnInitialize()
        {
            melee.image = TextureAssets.Item[ItemID.WarriorEmblem];
            melee.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                loadouts.Clear();
                foreach (var item in TestingLoadouts.Instance.meleeLoadouts)
                {
                    loadouts.Add(new LoadoutToPick(item));
                }
                active = 0;
            };
            melee.activePredicate = () => active == 0;

            ranged.Left.Set(52 * (1), 0);
            ranged.image = TextureAssets.Item[ItemID.RangerEmblem];
            ranged.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                loadouts.Clear();
                foreach (var item in TestingLoadouts.Instance.rangerLoadouts)
                {
                    loadouts.Add(new LoadoutToPick(item));
                }
                active = 1;
            };
            ranged.activePredicate = () => active == 1;
            magic.Left.Set(52 * (2), 0);
            magic.image = TextureAssets.Item[ItemID.SorcererEmblem];
            magic.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                loadouts.Clear();
                foreach (var item in TestingLoadouts.Instance.mageLoadouts)
                {
                    loadouts.Add(new LoadoutToPick(item));
                }
                active = 2;
            };
            magic.activePredicate = () => active == 2;
            summon.Left.Set(52 * (3), 0);
            summon.image = TextureAssets.Item[ItemID.SummonerEmblem];
            summon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                loadouts.Clear();
                foreach (var item in TestingLoadouts.Instance.summonerLoadouts)
                {
                    loadouts.Add(new LoadoutToPick(item));
                }
                active = 3;
            };
            summon.activePredicate = () => active == 3;
            rogue.Left.Set(52 * (4), 0);
            rogue.image = TextureAssets.Item[ModContent.ItemType<RogueEmblem>()];
            rogue.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                loadouts.Clear();
                foreach (var item in TestingLoadouts.Instance.rogueLoadouts)
                {
                    loadouts.Add(new LoadoutToPick(item));
                }
                active = 4;
            };
            rogue.activePredicate = () => active == 4;


            loadouts.Top.Set(52, 0);
            loadouts.Width.Set(0, 1);
            loadouts.Height.Set(-52, 1);
            loadouts.SetScrollbar(scrollbar);

            Append(melee);
            Append(ranged);
            Append(summon);
            Append(magic);
            Append(rogue);
            Append(loadouts);

            addInput.Height.Set(48, 0);
            addInput.Width.Set(-52, 1);
            addInput.Top.Set(52 + 45 * 5, 0);
            addInput.Left.Set(52, 0);
            Append(addInput);

            addNew.Top.Set(52 + 45 * 5, 0);
            addNew.image = TextureAssets.Camera[1];
            addNew.activePredicate = () =>
            {
                return active != -1 && addInput.searchBarText != "";
            };
            addNew.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                if (addNew.activePredicate.Invoke())
                {
                    switch (active)
                    {
                        case 0:
                            configCmd.Save(Main.LocalPlayer, ["melee", addInput.searchBarText]);
                            melee.LeftClick(evt);
                            break;
                        case 1:
                            configCmd.Save(Main.LocalPlayer, ["ranger", addInput.searchBarText]);
                            ranged.LeftClick(evt);
                            break;
                        case 2:
                            configCmd.Save(Main.LocalPlayer, ["mage", addInput.searchBarText]);
                            magic.LeftClick(evt);
                            break;
                        case 3:
                            configCmd.Save(Main.LocalPlayer, ["summoner", addInput.searchBarText]);
                            summon.LeftClick(evt);
                            break;
                        case 4:
                            configCmd.Save(Main.LocalPlayer, ["rogue", addInput.searchBarText]);
                            rogue.LeftClick(evt);
                            break;
                    }

                }
            };
            Append(addNew);

            Width.Set(52 * 5 + 16, 0);
            Height.Set(52 * 2 + 45 * 5 + 24, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);

            initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!initialized)
            {
                Initialize();

                foreach (var item in Children)
                {
                    item.Initialize();
                }
            }

            addInput.Height.Set(48, 0);

            foreach (var item in Children)
            {
                item.Update(gameTime);
            }
        }

    }

    public class LoadoutToPick : UIPanel
    {
        public string Name = "";
        public List<string> Items = new();
        public UIText text = new("");
        public LoadoutToPick(KeyValuePair<string, List<string>> loadout)
        {
            Name = loadout.Key;
            Items = loadout.Value;
            text = new(Name);
            text.VAlign = 0.5f;
            text.HAlign = 0.5f;
            text.TextOriginX = 0.5f;
            text.TextOriginY = 0.5f;
            text.IgnoresMouseInteraction = true;
            Width.Set(0, 1);
            Height.Set(40, 0);
            OnLeftClick += LoadoutToPick_OnLeftClick;
            Append(text);
        }

        private void LoadoutToPick_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {

            Player player = Main.LocalPlayer;
            for (int i = 0; i < Items.Count; i++)
            {
                Item item2 = new Item();
                if (Items[i].Split(":")[0] != "Terraria")
                {
                    item2 = new Item(ItemID.Search.GetId(Items[i].Split(":")[0] + "/" + Items[i].Split(":")[1]));
                }
                else if (Items[i].Split(":")[1] != "None")
                {
                    item2 = new Item(ItemID.Search.GetId(Items[i].Split(":")[1]));
                }
                player.armor[i] = item2;
            }
            ReforgeCmd.ApplyReforge(player, ["menacing"]);
        }
    }
    public class PermanentUpgrades : UIPanel
    {
        bool initialized = false;

        #region Permanent Upgrades
        UIImageButtonBorder LifeIcon = new();
        UIColoredSliderSimple LifeSlider = new();

        UIImageButtonBorder ManaIcon = new();
        UIColoredSliderSimple ManaSlider = new();

        UIImageButtonBorder RageIcon = new();
        UIColoredSliderSimple RageSlider = new();

        UIImageButtonBorder AdrenIcon = new();
        UIColoredSliderSimple AdrenSlider = new();

        UIImageButtonBorder VitalCrystalIcon = new();
        UIImageButtonBorder AegisFruitIcon = new();
        UIImageButtonBorder ArcaneCrystalIcon = new();
        UIImageButtonBorder GalaxyPearlIcon = new();

        UIImageButtonBorder AmbrosiaIcon = new();
        UIImageButtonBorder GummyWormIcon = new();
        UIImageButtonBorder PeddlersSatchelIcon = new();
        UIImageButtonBorder ArtisanLoafIcon = new();

        UIImageButtonBorder DemonHeartIcon = new();
        UIImageButtonBorder CelestialOnionIcon = new();
        UIImageButtonBorder AdvancedCombatIcon = new();
        UIImageButtonBorder AdvancedCombat2Icon = new();

        List<UIImageButtonBorder> miscUpgrades => [
            VitalCrystalIcon, AegisFruitIcon, ArcaneCrystalIcon, GalaxyPearlIcon,
                AmbrosiaIcon, GummyWormIcon, PeddlersSatchelIcon, ArtisanLoafIcon,
                DemonHeartIcon, CelestialOnionIcon, AdvancedCombatIcon, AdvancedCombat2Icon,
                ];
        #endregion

        public override void OnInitialize()
        {
            Width.Set(228, 0);
            Height.Set(308, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);

            #region sliders
            LifeIcon.Width = LifeIcon.Height = new(32, 0);
            LifeIcon.Top.Set(0, 0);
            LifeIcon.SetPadding(0);
            LifeIcon.drawBG = false;
            LifeIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.playerLifeTotal += Helpers.playerLifeTotal >= 600 ? -500 : Helpers.playerLifeTotal >= 500 ? 25 : Helpers.playerLifeTotal >= 400 ? 100 : 300;
            LifeIcon.image = TextureAssets.Item[ItemID.LifeCrystal];

            LifeSlider.Left.Set(32, 0f);
            LifeSlider.Width.Set(-32, 1f);
            LifeSlider.Height.Set(16, 0);
            LifeSlider.Top.Set(8, 0);
            LifeSlider.FilledColor = Color.IndianRed;
            LifeSlider.EmptyColor = Color.LightGray;
            LifeSlider.OnLeftClick += AdjustLifeSlider;

            ManaIcon.Width = ManaIcon.Height = new(32, 0);
            ManaIcon.Top.Set(32, 0);
            ManaIcon.SetPadding(0);
            ManaIcon.drawBG = false;
            ManaIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.playerManaTotal += Helpers.playerManaTotal >= 350 ? -330 : Helpers.playerManaTotal >= 200 ? 50 : 180;
            ManaIcon.image = TextureAssets.Item[ItemID.ManaCrystal];


            ManaSlider.Left.Set((32), 0f);
            ManaSlider.Width.Set(-32, 1f);
            ManaSlider.Height.Set(16, 0);
            ManaSlider.Top.Set(8 + ManaIcon.Top.Pixels, 0);
            ManaSlider.EmptyColor = Color.LightGray;
            ManaSlider.FilledColor = Color.RoyalBlue;
            ManaSlider.OnLeftClick += AdjustManaSlider;


            RageIcon.Width = RageIcon.Height = new(32, 0);
            RageIcon.Top.Set(64, 0);
            RageIcon.SetPadding(2);
            RageIcon.drawBG = false;
            RageIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.rageBoostUsed += (Helpers.rageBoostUsed < 3 ? 1 : -3);
            RageIcon.image = TextureAssets.Item[ModContent.ItemType<MushroomPlasmaRoot>()];

            RageSlider.Left.Set((32), 0f);
            RageSlider.Width.Set(-32, 1f);
            RageSlider.Height.Set(16, 0);
            RageSlider.Top.Set(8 + RageIcon.Top.Pixels, 0);
            RageSlider.EmptyColor = Color.LightGray;
            RageSlider.FilledColor = Color.Lerp(Color.DarkOrange, Color.IndianRed, 0.25f);
            RageSlider.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                var dim = listeningElement.GetDimensions();
                Helpers.rageBoostUsed = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.1666f, 0, 1) * 3);
            };

            AdrenIcon.Width = AdrenIcon.Height = new(32, 0);
            AdrenIcon.Top.Set(96, 0);
            AdrenIcon.SetPadding(1);
            AdrenIcon.drawBG = false;
            AdrenIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.adrenBoostUsed += (Helpers.adrenBoostUsed < 3 ? 1 : -3);
            AdrenIcon.image = TextureAssets.Item[ModContent.ItemType<ElectrolyteGelPack>()];

            AdrenSlider.Left.Set((32), 0f);
            AdrenSlider.Width.Set(-32, 1f);
            AdrenSlider.Height.Set(16, 0);
            AdrenSlider.Top.Set(8 + AdrenIcon.Top.Pixels, 0);
            AdrenSlider.EmptyColor = Color.LightGray;
            AdrenSlider.FilledColor = Color.Lerp(Color.SpringGreen, Color.Black, 0.4f);
            AdrenSlider.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                var dim = listeningElement.GetDimensions();
                Helpers.adrenBoostUsed = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.1666f, 0, 1) * 3);
            };
            #endregion

            #region toggles
            VitalCrystalIcon.image = TextureAssets.Item[ItemID.AegisCrystal];
            VitalCrystalIcon.activePredicate = () => Main.LocalPlayer.usedAegisCrystal;
            VitalCrystalIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAegisCrystal.Invert();

            AegisFruitIcon.image = TextureAssets.Item[ItemID.AegisFruit];
            AegisFruitIcon.activePredicate = () => Main.LocalPlayer.usedAegisFruit;
            AegisFruitIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAegisFruit.Invert();

            ArcaneCrystalIcon.image = TextureAssets.Item[ItemID.ArcaneCrystal];
            ArcaneCrystalIcon.activePredicate = () => Main.LocalPlayer.usedArcaneCrystal;
            ArcaneCrystalIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedArcaneCrystal.Invert();

            GalaxyPearlIcon.image = TextureAssets.Item[ItemID.GalaxyPearl];
            GalaxyPearlIcon.activePredicate = () => Main.LocalPlayer.usedGalaxyPearl;
            GalaxyPearlIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedGalaxyPearl.Invert();

            AmbrosiaIcon.image = TextureAssets.Item[ItemID.Ambrosia];
            AmbrosiaIcon.activePredicate = () => Main.LocalPlayer.usedAmbrosia;
            AmbrosiaIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAmbrosia.Invert();

            GummyWormIcon.image = TextureAssets.Item[ItemID.GummyWorm];
            GummyWormIcon.activePredicate = () => Main.LocalPlayer.usedGummyWorm;
            GummyWormIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedGummyWorm.Invert();

            PeddlersSatchelIcon.image = TextureAssets.Item[ItemID.PeddlersSatchel];
            PeddlersSatchelIcon.activePredicate = () => NPC.peddlersSatchelWasUsed;
            PeddlersSatchelIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.peddlersSatchelWasUsed.Invert();

            ArtisanLoafIcon.image = TextureAssets.Item[ItemID.ArtisanLoaf];
            ArtisanLoafIcon.activePredicate = () => Main.LocalPlayer.ateArtisanBread;
            ArtisanLoafIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.ateArtisanBread.Invert();

            DemonHeartIcon.image = TextureAssets.Item[ItemID.DemonHeart];
            DemonHeartIcon.activePredicate = () => Main.LocalPlayer.extraAccessory;
            DemonHeartIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.extraAccessory.Invert();

            CelestialOnionIcon.image = TextureAssets.Item[ModContent.ItemType<CelestialOnion>()];
            CelestialOnionIcon.activePredicate = () => Main.LocalPlayer.Calamity().extraAccessoryML;
            CelestialOnionIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.Calamity().extraAccessoryML.Invert();
            CelestialOnionIcon.SetPadding(6);

            AdvancedCombatIcon.image = TextureAssets.Item[ItemID.CombatBook];
            AdvancedCombatIcon.activePredicate = () => NPC.combatBookWasUsed;
            AdvancedCombatIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.combatBookWasUsed.Invert();

            AdvancedCombat2Icon.image = TextureAssets.Item[ItemID.CombatBookVolumeTwo];
            AdvancedCombat2Icon.activePredicate = () => NPC.combatBookVolumeTwoWasUsed;
            AdvancedCombat2Icon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.combatBookVolumeTwoWasUsed.Invert();
            #endregion

            Append(LifeIcon);
            Append(LifeSlider);
            Append(ManaIcon);
            Append(ManaSlider);
            Append(RageIcon);
            Append(RageSlider);
            Append(AdrenIcon);
            Append(AdrenSlider);

            for (int i = 0; i < miscUpgrades.Count; i++)
            {
                var item = miscUpgrades[i];
                item.Top.Set(52 * (i / 4) + 36 + AdrenIcon.Top.Pixels, 0);
                item.Left.Set(52 * (i % 4), 0);
                item.SetPadding(0);
                Append(item);
            }
            initialized = true;
        }
        private void AdjustLifeSlider(UIMouseEvent evt, UIElement listeningElement)
        {
            var dim = listeningElement.GetDimensions();
            Helpers.playerLifeTotal = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.05f, 0, 1) * 600);
        }

        private void AdjustManaSlider(UIMouseEvent evt, UIElement listeningElement)
        {
            var dim = listeningElement.GetDimensions();
            Helpers.playerManaTotal = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.05f, 0, 1) * 350);
        }

        public override void Update(GameTime gameTime)
        {

            if (!initialized)
            {
                Initialize();

                foreach (var item in Children)
                {
                    item.Initialize();
                }
            }
            foreach (var item in Children)
            {
                item.Update(gameTime);
            }
            ManaSlider.FillPercent = Helpers.playerManaTotal / 350f;
            LifeSlider.FillPercent = Helpers.playerLifeTotal / 600f;
            RageSlider.FillPercent = Helpers.rageBoostUsed / 3f;
            AdrenSlider.FillPercent = Helpers.adrenBoostUsed / 3f;
            CelestialOnionIcon.SetPadding(6);

        }


    }
    public class BossToggles : UIPanel
    {
        UIPanel Aftermath = new();
        UIText AftermathText = new("Aftermath");

        UIPanel ToggleAll = new();
        UIText ToggleText = new("Toggle All");

        UIImageButtonBorder KS = new();
        UIImageButtonBorder DS = new();
        UIImageButtonBorder EoC = new();
        UIImageButtonBorder Crab = new();
        UIImageButtonBorder Evil1 = new();
        UIImageButtonBorder Evil2 = new();
        UIImageButtonBorder QB = new();
        UIImageButtonBorder Deer = new();
        UIImageButtonBorder Skeletron = new();
        UIImageButtonBorder SG = new();
        UIImageButtonBorder WoF = new();


        UIImageButtonBorder Clam = new();
        UIImageButtonBorder QS = new();
        UIImageButtonBorder Cryo = new();
        UIImageButtonBorder Twin = new();
        UIImageButtonBorder AS = new();
        UIImageButtonBorder Dest = new();
        UIImageButtonBorder Brim = new();
        UIImageButtonBorder Prime = new();
        UIImageButtonBorder Clone = new();
        UIImageButtonBorder Plant = new();

        UIImageButtonBorder Levi = new();
        UIImageButtonBorder Aureus = new();
        UIImageButtonBorder Golem = new();
        UIImageButtonBorder Duke = new();
        UIImageButtonBorder PBG = new();
        UIImageButtonBorder EoL = new();
        UIImageButtonBorder Ravager = new();
        UIImageButtonBorder Cultist = new();
        UIImageButtonBorder Deus = new();
        UIImageButtonBorder ML = new();

        UIImageButtonBorder Folly = new();
        UIImageButtonBorder PG = new();
        UIImageButtonBorder Prov = new();
        UIImageButtonBorder CV = new();
        UIImageButtonBorder SW = new();
        UIImageButtonBorder Signus = new();
        UIImageButtonBorder Polter = new();
        UIImageButtonBorder OD = new();
        UIImageButtonBorder DoG = new();
        UIImageButtonBorder Yharon = new();
        UIImageButtonBorder Exo = new();
        UIImageButtonBorder Scal = new();
        UIImageButtonBorder Wyrm = new();

        List<List<UIImageButtonBorder>> allBosses => [
            prehardmode, hardmode, postplant, postml
            ];

        List<UIImageButtonBorder> prehardmode => [
            KS, DS, EoC, Crab, Evil1, Evil2, QB, Deer, Skeletron, SG, WoF
            ];
        List<UIImageButtonBorder> hardmode => [
            QS, Cryo, Twin, AS, Dest, Brim, Prime, Clone, Plant
            ];
        List<UIImageButtonBorder> postplant => [
            Levi, Aureus, Golem, Duke, PBG, EoL, Ravager, Cultist, Deus, ML
            ];
        List<UIImageButtonBorder> postml => [
            Folly, PG, Prov, CV, SW, Signus, Polter, OD, DoG, Yharon, Scal, Exo, Wyrm
            ];

        List<UIImageButtonBorder> all
        {
            get
            {
                var all = new List<UIImageButtonBorder>();
                all.AddRange(prehardmode);
                all.AddRange(hardmode);
                all.AddRange(postplant);
                all.AddRange(postml);
                return all;
            }
        }

        bool initialized = false;

        public override void OnInitialize()
        {
            #region prehardmode
            KS.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.KingSlime]]);
            KS.activePredicate = () => NPC.downedSlimeKing;
            KS.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedSlimeKing.Invert();

            DS.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<DesertScourgeHead>()]]);
            DS.activePredicate = () => DownedBossSystem.downedDesertScourge;
            DS.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedDesertScourge = !DownedBossSystem.downedDesertScourge;

            EoC.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.EyeofCthulhu]]);
            EoC.activePredicate = () => NPC.downedBoss1;
            EoC.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedBoss1.Invert();

            Crab.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<Crabulon>()]]);
            Crab.activePredicate = () => DownedBossSystem.downedCrabulon;
            Crab.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedCrabulon = !DownedBossSystem.downedCrabulon;

            Evil1.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.EaterofWorldsHead]]);
            Evil1.secondaryImage = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.BrainofCthulhu]], () => WorldGen.crimson);
            Evil1.activePredicate = () => NPC.downedBoss2;
            Evil1.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedBoss2.Invert();

            Evil2.image = (TextureAssets.NpcHeadBoss[HiveMind.phase2IconIndex]);
            Evil2.secondaryImage = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<PerforatorHive>()]], () => WorldGen.crimson);
            Evil2.activePredicate = () => (DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator);
            Evil2.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedHiveMind = DownedBossSystem.downedPerforator = !Evil2.activePredicate();

            QB.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.QueenBee]]);
            QB.activePredicate = () => NPC.downedQueenBee;
            QB.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedQueenBee.Invert();

            Deer.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Deerclops]]);
            Deer.activePredicate = () => NPC.downedDeerclops;
            Deer.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedDeerclops.Invert();

            Skeletron.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.SkeletronHead]]);
            Skeletron.activePredicate = () => NPC.downedBoss3;
            Skeletron.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedBoss3.Invert();

            SG.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<SlimeGodCore>()]]);
            SG.activePredicate = () => DownedBossSystem.downedSlimeGod;
            SG.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedSlimeGod = !DownedBossSystem.downedSlimeGod;

            WoF.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.WallofFlesh]]);
            WoF.activePredicate = () => Main.hardMode;
            WoF.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.hardMode.Invert();
            #endregion

            #region hm
            QS.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.QueenSlimeBoss]]);
            QS.activePredicate = () => NPC.downedQueenSlime;
            QS.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedQueenSlime.Invert();

            Cryo.image = (TextureAssets.NpcHeadBoss[Cryogen.cryoIconIndex]);
            Cryo.activePredicate = () => DownedBossSystem.downedCryogen;
            Cryo.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedCryogen = !DownedBossSystem.downedCryogen;

            Twin.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Spazmatism]]);
            Twin.activePredicate = () => NPC.downedMechBoss2;
            Twin.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                NPC.downedMechBoss2.Invert();
                NPC.downedMechBossAny = (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3);
            };

            AS.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<AquaticScourgeHead>()]]);
            AS.activePredicate = () => DownedBossSystem.downedAquaticScourge;
            AS.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedAquaticScourge = !DownedBossSystem.downedAquaticScourge;

            Dest.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.TheDestroyer]]);
            Dest.activePredicate = () => NPC.downedMechBoss1;
            Dest.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                NPC.downedMechBoss1.Invert();
                NPC.downedMechBossAny = (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3);
            };

            Brim.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<BrimstoneElemental>()]]);
            Brim.activePredicate = () => DownedBossSystem.downedBrimstoneElemental;
            Brim.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedBrimstoneElemental = !DownedBossSystem.downedBrimstoneElemental;

            Prime.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.SkeletronPrime]]);
            Prime.activePredicate = () => NPC.downedMechBoss3;
            Prime.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                NPC.downedMechBoss3.Invert();
                NPC.downedMechBossAny = (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3);
            };

            Clone.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<CalamitasClone>()]]);
            Clone.activePredicate = () => DownedBossSystem.downedCalamitasClone;
            Clone.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedCalamitasClone = !DownedBossSystem.downedCalamitasClone;

            Plant.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Plantera]]);
            Plant.activePredicate = () => NPC.downedPlantBoss;
            Plant.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedPlantBoss.Invert();

            #endregion

            #region postplant
            Levi.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<Leviathan>()]]);
            Levi.activePredicate = () => DownedBossSystem.downedLeviathan;
            Levi.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedLeviathan = !DownedBossSystem.downedLeviathan;

            Aureus.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<AstrumAureus>()]]);
            Aureus.activePredicate = () => DownedBossSystem.downedAstrumAureus;
            Aureus.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedAstrumAureus = !DownedBossSystem.downedAstrumAureus;

            Golem.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.GolemHead]]);
            Golem.activePredicate = () => NPC.downedGolemBoss;
            Golem.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedGolemBoss.Invert();

            Duke.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.DukeFishron]]);
            Duke.activePredicate = () => NPC.downedFishron;
            Duke.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedFishron.Invert();

            PBG.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<PlaguebringerGoliath>()]]);
            PBG.activePredicate = () => DownedBossSystem.downedPlaguebringer;
            PBG.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedPlaguebringer = !DownedBossSystem.downedPlaguebringer;

            EoL.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.HallowBoss]]);
            EoL.activePredicate = () => NPC.downedEmpressOfLight;
            EoL.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedEmpressOfLight.Invert();

            Ravager.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<RavagerBody>()]]);
            Ravager.activePredicate = () => DownedBossSystem.downedRavager;
            Ravager.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedRavager = !DownedBossSystem.downedRavager;

            Cultist.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.CultistBoss]]);
            Cultist.activePredicate = () => NPC.downedAncientCultist;
            Cultist.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedAncientCultist.Invert();

            Deus.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<AstrumDeusHead>()]]);
            Deus.activePredicate = () => DownedBossSystem.downedAstrumDeus;
            Deus.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedAstrumDeus = !DownedBossSystem.downedAstrumDeus;

            ML.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.MoonLordHead]]);
            ML.activePredicate = () => NPC.downedMoonlord;
            ML.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.downedMoonlord.Invert();
            #endregion

            #region post-ml

            Folly.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<Dragonfolly>()]]);
            Folly.activePredicate = () => DownedBossSystem.downedDragonfolly;
            Folly.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedDragonfolly = !DownedBossSystem.downedDragonfolly;

            PG.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<ProfanedGuardianHealer>()]]);
            PG.activePredicate = () => DownedBossSystem.downedGuardians;
            PG.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedGuardians = !DownedBossSystem.downedGuardians;

            Prov.image = (TextureAssets.Item[ModContent.ItemType<ProfanedCore>()]);
            Prov.activePredicate = () => DownedBossSystem.downedProvidence;
            Prov.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedProvidence = !DownedBossSystem.downedProvidence;

            CV.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<CeaselessVoid>()]]);
            CV.activePredicate = () => DownedBossSystem.downedCeaselessVoid;
            CV.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedCeaselessVoid = !DownedBossSystem.downedCeaselessVoid;

            SW.image = (TextureAssets.NpcHeadBoss[StormWeaverHead.normalIconIndex]);
            SW.activePredicate = () => DownedBossSystem.downedStormWeaver;
            SW.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedStormWeaver = !DownedBossSystem.downedStormWeaver;

            Signus.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<Signus>()]]);
            Signus.activePredicate = () => DownedBossSystem.downedSignus;
            Signus.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedSignus = !DownedBossSystem.downedSignus;

            Polter.image = (TextureAssets.NpcHeadBoss[Polterghast.phase1IconIndex]);
            Polter.activePredicate = () => DownedBossSystem.downedPolterghast;
            Polter.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedPolterghast = !DownedBossSystem.downedPolterghast;

            OD.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<OldDuke>()]]);
            OD.activePredicate = () => DownedBossSystem.downedBoomerDuke;
            OD.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedBoomerDuke = !DownedBossSystem.downedBoomerDuke;

            DoG.image = (TextureAssets.NpcHeadBoss[DevourerofGodsHead.phase1IconIndex]);
            DoG.activePredicate = () => DownedBossSystem.downedDoG;
            DoG.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedDoG = !DownedBossSystem.downedDoG;

            Yharon.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<Yharon>()]]);
            Yharon.activePredicate = () => DownedBossSystem.downedYharon;
            Yharon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedYharon = !DownedBossSystem.downedYharon;

            Exo.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<AresBody>()]]);
            Exo.activePredicate = () => DownedBossSystem.downedExoMechs;
            Exo.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedExoMechs = !DownedBossSystem.downedExoMechs;

            Scal.image = (TextureAssets.NpcHeadBoss[SupremeCalamitas.hoodedHeadIconIndex]);
            Scal.activePredicate = () => DownedBossSystem.downedCalamitas;
            Scal.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedCalamitas = !DownedBossSystem.downedCalamitas;

            Wyrm.image = (TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[ModContent.NPCType<PrimordialWyrmHead>()]]);
            Wyrm.activePredicate = () => DownedBossSystem.downedPrimordialWyrm;
            Wyrm.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => DownedBossSystem.downedPrimordialWyrm = !DownedBossSystem.downedPrimordialWyrm;
            #endregion

            Aftermath.Width = new(-4, 0.5f);
            Aftermath.OnLeftClick += (UIMouseEvent evt, UIElement listener) =>
            {
                bossLockCmd.Toggle();
            };
            Aftermath.Left = new(0, 0.5f);

            ToggleAll.Width = new(-4, 0.5f);
            ToggleAll.OnLeftClick += (UIMouseEvent evt, UIElement listener) =>
            {
                var all = this.all;
                bool enabled = false;
                foreach (var item in all)
                {
                    if (item.activePredicate.Invoke())
                        enabled = true;
                }
                foreach (var item in all)
                {
                    if (item.activePredicate.Invoke() == enabled)
                        item.LeftClick(evt);
                }
            };
            ToggleAll.Append(ToggleText);
            ToggleText.IgnoresMouseInteraction = true;
            ToggleText.TextOriginX = 0.5f;
            ToggleText.TextOriginY = 0.5f;
            ToggleText.VAlign = 0.5f;
            ToggleText.HAlign = 0.5f;

            Aftermath.Append(AftermathText);
            AftermathText.IgnoresMouseInteraction = true;
            AftermathText.TextOriginX = 0.5f;
            AftermathText.TextOriginY = 0.5f;
            AftermathText.VAlign = 0.5f;
            AftermathText.HAlign = 0.5f;
            Append(ToggleAll);
            Append(Aftermath);

            for (int i = 0; i < allBosses.Count; i++)
            {
                for (int i2 = 0; i2 < allBosses[i].Count; i2++)
                {
                    allBosses[i][i2].OnRightClick += AddUntilThisBoss;
                    allBosses[i][i2].Left.Set(52 * (i), 0);
                    allBosses[i][i2].Top.Set(52 * (i2 + 1), 0);
                    Append(allBosses[i][i2]);
                }
            }


            Width.Set(52 * allBosses.Count() + 16, 0);
            Height.Set(52 * (postml.Count() + 1) + 16, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);
            initialized = true;
        }

        private void AddUntilThisBoss(UIMouseEvent evt, UIElement listeningElement)
        {
            var allList = all;
            var clicked = (listeningElement as UIImageButtonBorder);
            var active = true;
            foreach (var item in allList)
            {
                if (item.activePredicate.Invoke() != active)
                    item.LeftClick(evt);

                if (item == clicked)
                    active = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!initialized)
            {
                Initialize();

                foreach (var item in Children)
                {
                    item.Initialize();
                }
            }
            foreach (var item in Children)
            {
                item.Update(gameTime);
            }
            Aftermath.Height.Set(48, 0);
            ToggleAll.Height.Set(48, 0);
            Aftermath.Left = new(4, 0.5f);
            AftermathText.TextColor = bossLockNPC.bossLock ? Color.Pink : Color.LightGreen;
        }


    }
    public class UIImageButtonBorder : UIPanel
    {
        public Asset<Texture2D> image;

        public (Asset<Texture2D>, Func<bool>) secondaryImage;
        public Rectangle? frame = null;
        public bool active = true;
        public Func<bool> activePredicate = null;
        public bool drawBG = true;

        public UIImageButtonBorder()
        {
            SetPadding(8);
            Width = Height = new(48, 0);
            drawBG = true;
        }
        public override void OnInitialize()
        {
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (drawBG)
                base.DrawSelf(spriteBatch);
            var dim = GetDimensions();
            var inDim = GetInnerDimensions();
            if (image != null)
            {
                var tex = secondaryImage.Item2 != null && secondaryImage.Item2.Invoke() ? secondaryImage.Item1.Value : image.Value;
                spriteBatch.Draw(tex, dim.Center() - new Vector2(1, 0), frame, Color.White * (active ? 1 : 0.33f), 0, tex.Size() * 0.5f, MathHelper.Min(MathHelper.Min(inDim.Width / tex.Width, inDim.Height / tex.Height), 1), 0, 0);
            }
        }


        public override void Update(GameTime gameTime)
        {
            if (activePredicate != null)
            {
                active = activePredicate.Invoke();
            }
        }
    }

    public class UITextInput : UIPanel
    {

        UISearchBar addInput = new(Language.GetText("TestingEfficiency.GUI.Loadouts.SearchText"), 1);

        public string searchBarText = "";
        public override void OnInitialize()
        {
            SetPadding(0);
            addInput.Height.Set(0, 1);
            addInput.Width.Set(0, 1);
            addInput.OnContentsChanged += OnSearchContentsChanged;
            addInput.OnStartTakingInput += OnStartTakingInput;
            addInput.OnEndTakingInput += OnEndTakingInput;
            addInput.OnNeedingVirtualKeyboard += OpenVirtualKeyboardWhenNeeded;
            addInput.OnCanceledTakingInput += OnCanceledInput;
        }
        private void OnSearchContentsChanged(string contents)
        {
            searchBarText = contents;
        }

        private void OnStartTakingInput()
        {
            BorderColor = Main.OurFavoriteColor;
        }

        private void OnEndTakingInput()
        {
            BorderColor = new Color(35, 40, 83);
        }
        private void OnCanceledInput()
        {
            Main.LocalPlayer.ToggleInv();
        }

        private void OpenVirtualKeyboardWhenNeeded()
        {
            int maxInputLength = 40;
            UIVirtualKeyboard uIVirtualKeyboard = new UIVirtualKeyboard(Language.GetText("UI.PlayerNameSlot").Value, searchBarText, OnFinishedSettingName, () => addInput.ToggleTakingText(), 3, allowEmpty: true);
            uIVirtualKeyboard.SetMaxInputLength(maxInputLength);
            uIVirtualKeyboard.CustomEscapeAttempt = EscapeVirtualKeyboard;
            IngameFancyUI.OpenUIState(uIVirtualKeyboard);
        }
        private void OnFinishedSettingName(string name)
        {
            string contents = name.Trim();
            addInput.SetContents(contents);
            addInput.ToggleTakingText();
        }
        private bool EscapeVirtualKeyboard()
        {
            Main.playerInventory = true;
            if (addInput.IsWritingText)
            {
                addInput.ToggleTakingText();
            }
            return true;
        }
    }
}

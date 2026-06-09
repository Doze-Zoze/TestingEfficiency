using Microsoft.CodeAnalysis;
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
using Terraria.UI.Chat;
using TestingEfficiency.Commands;
using TestingEfficiency.DamageStats;
using static TestingEfficiency.DataStructures;

namespace TestingEfficiency.Helpers
{
    public class TestingGUIManager : ModSystem
    {
        private UserInterface _testingGUI;

        public static TestingUI testingUI;

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
        UIImageButtonBorder ExportMenuButton = new();
        #endregion


        PermanentUpgrades PermanentUpgradePanel = new PermanentUpgrades();
        BossToggles BossTogglePanel = new BossToggles();
        Loadouts LoadoutPanel = new Loadouts();
        public DPSDisplay DPSPanel = new();
        public DataExport exportPanel = new();

        #region Toggle Bosses
        #endregion
        public override void OnInitialize()
        {
            Main.instance.LoadItem(ItemID.GoblinTech);
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
            MainMenuPanel.Height = new(52 * 5 - 4 + MainMenuPanel.PaddingTop + MainMenuPanel.PaddingBottom, 0);
            MainMenuPanel.Top.Set(16, 0);
            MainMenuPanel.Left.Set(-(MainMenuPanel.Width.Pixels + 4), 0.75f);

            Main.instance.LoadItem(ItemID.LifeCrystal);
            PermanentUpgradeMenuButton.image = (TextureAssets.Item[ItemID.LifeCrystal]);
            PermanentUpgradeMenuButton.activePredicate = () => Children.Contains(PermanentUpgradePanel);
            PermanentUpgradeMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(PermanentUpgradePanel);
            };
            MainMenuPanel.Append(PermanentUpgradeMenuButton);

            Main.instance.LoadItem(ItemID.SuspiciousLookingEye);
            BossToggleMenuButton.image = (TextureAssets.Item[ItemID.SuspiciousLookingEye]);
            BossToggleMenuButton.Top = new(48 + MainMenuPanel.MarginTop + 4, 0);
            BossToggleMenuButton.activePredicate = () => Children.Contains(BossTogglePanel);
            BossToggleMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(BossTogglePanel);
            };
            MainMenuPanel.Append(BossToggleMenuButton);

            Main.instance.LoadItem(ItemID.CopperShortsword);
            LoadoutMenuButton.image = (TextureAssets.Item[ItemID.CopperShortsword]);
            LoadoutMenuButton.Top = new(52 * 2 + MainMenuPanel.MarginTop, 0);
            LoadoutMenuButton.activePredicate = () => Children.Contains(LoadoutPanel);
            LoadoutMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(LoadoutPanel);
            };
            MainMenuPanel.Append(LoadoutMenuButton);

            Main.instance.LoadItem(ItemID.DPSMeter);
            DPSMenuButton.image = (TextureAssets.Item[ItemID.DPSMeter]);
            DPSMenuButton.Top = new(52 * 3 + MainMenuPanel.MarginTop, 0);
            DPSMenuButton.activePredicate = () => Children.Contains(DPSPanel);
            DPSMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(DPSPanel);
            };
            MainMenuPanel.Append(DPSMenuButton);


            Main.instance.LoadItem(ItemID.Book);
            ExportMenuButton.image = (TextureAssets.Item[ItemID.Book]);
            ExportMenuButton.Top = new(52 * 4 + MainMenuPanel.MarginTop, 0);
            ExportMenuButton.activePredicate = () => Children.Contains(exportPanel);
            ExportMenuButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                ToggleGrandchild(exportPanel);
            };
            //Disabled until finished
            //MainMenuPanel.Append(ExportMenuButton);

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
        public PlayerHealthGraph playerGraph = new();

        public UIBasicTextbox time = new();
        public UIBasicTextbox splits = new();

        public static SaveTestBox saveTextBox = new();


        UIList Outputs = new();
        UIScrollbar scrollbar = new();

        public override void OnInitialize()
        {
            Width.Set(52 * 5 + 16, 0);
            Height.Set(52 * 2 + 45 * 5 + 24, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);
            //Append(graph);
            //Append(playerGraph);
            graph.SetPadding(PaddingTop);
            graph.BackgroundColor = new Color(52, 66, 119);

            playerGraph.SetPadding(PaddingTop);
            playerGraph.BackgroundColor = new Color(52, 66, 119);

            time.textToUse = () => DamageStatsSystem.lastIGT ?? "";
            time.Width.Set(0, 1);

            splits.Width.Set(0, 1);
            splits.textToUse = () => DamageStatsSystem.lastSplits ?? "";

            Outputs.Width.Set(0, 1);
            Outputs.Height.Set(0, 1);
            Outputs.SetScrollbar(scrollbar);

            Append(Outputs);
            Outputs.Add(graph);
            Outputs.Add(playerGraph);
            Outputs.Add(time);
            Outputs.Add(splits);
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
            bool hasTestData = Outputs.Contains(saveTextBox);
            Outputs.Clear();

            Outputs.Add(graph);
            Outputs.Add(playerGraph);
            if (time.textToUse.Invoke() != string.Empty)
                Outputs.Add(time);
            if (splits.textToUse.Invoke() != string.Empty)
                Outputs.Add(splits);

            var goalWidth = 200f;
            var goalHeight = 0f;
            foreach (var item in Outputs)
            {
                if (item is UIBasicTextbox)
                {
                    var bt = (item as UIBasicTextbox);
                    goalWidth = Math.Max(goalWidth, FontAssets.MouseText.Value.MeasureString(bt.textToUse.Invoke()).X - 140);
                }
                goalHeight += item.GetOuterDimensions().Height + 12;
            }
            Width.Set(goalWidth, 0);



            Height.Set(goalHeight, 0);
            MaxWidth.Set(0, 0.5f);
            MaxHeight.Set(-32, 1f);
            Top.Set(16, 0);
            var calcWidth = GetOuterDimensions();
            Left.Set(-(calcWidth.Width + 48 + PaddingLeft * 2 + 8), 0.75f);
            return;
            //if (!Children.Contains(time))
            //    Append(time);

            if (!Children.Contains(Outputs))
            {
                Outputs.SetPadding(0);
            }
            return;
            Width.Set(400, 0);
            //time.Height.Set(170, 0);
            time.Width.Set(170, 0);
            time.HAlign = 0;
            time.text.VAlign = 0;
            time.Top.Set(264, 0);
            Height.Set(458, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);

            playerGraph.Top.Set(132, 0);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
    public class BossHealthGraph : UIPanel
    {

        bool initialized = false;
        UIText BossText = new("Boss HP Graph", 0.8f);

        public BossHealthGraph()
        {
            Append(BossText);
            Width.Set(0, 1);
            Height.Set(120, 0);
            BackgroundColor = new Color(52, 66, 119);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            var dim = GetInnerDimensions().ToRectangle();
            dim.Height -= 20;
            dim.Y += 20;

            spriteBatch.End();
            spriteBatch.Begin(default, null, SamplerState.PointClamp, null, null, null, Main.UIScaleMatrix);
            if (DamageStatsSystem.LastBossHPGraph is not null)
            {
                spriteBatch.Draw(DamageStatsSystem.LastBossHPGraph, dim, Color.White);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, null, default, null, null, null, Main.UIScaleMatrix);
        }
    }
    public class PlayerHealthGraph : UIPanel
    {

        bool initialized = false;
        UIText text = new("Player HP Graph", 0.8f);

        public PlayerHealthGraph()
        {
            Width.Set(0, 1);
            Height.Set(120, 0);
            BackgroundColor = new Color(52, 66, 119);
            Append(text);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            var dim = GetInnerDimensions().ToRectangle();
            dim.Height -= 20;
            dim.Y += 20;

            spriteBatch.End();
            spriteBatch.Begin(default, null, SamplerState.PointClamp, null, null, null, Main.UIScaleMatrix);
            if (DamageStatsSystem.LastPlayerHPGraph is not null)
            {
                spriteBatch.Draw(DamageStatsSystem.LastPlayerHPGraph, dim, Color.White);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, null, default, null, null, null, Main.UIScaleMatrix);

        }
    }

    public class DataExport : UIPanel
    {

        bool initialized = false;

        public static SaveTestBox saveTextBox = new();

        public static Dictionary<BossTestData, SaveTestBox> testBoxes = new();

        public static UIList Outputs = new();
        UIScrollbar scrollbar = new();

        int childcount = 0;


        public override void OnInitialize()
        {
            Width.Set(52 * 5 + 16, 0);
            Height.Set(52 * 2 + 45 * 5 + 24, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);

            Outputs.Width.Set(0, 1);
            Outputs.Height.Set(0, 1);
            Outputs.SetScrollbar(scrollbar);

            Append(Outputs);
            initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (saveTextBox.testData != DamageStatsSystem.lastTestData)
            {
                saveTextBox = new(DamageStatsSystem.lastTestData);
            }

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
            Outputs.Clear();
            Outputs.Add(saveTextBox);

            foreach (var test in DamageStatsSystem.CurrentTestSession.bosses)
            {
                if (test == DamageStatsSystem.lastTestData)
                    continue;
                if (!testBoxes.Keys.Contains(test))
                {
                    var t = new SaveTestBox(test);
                    testBoxes.Add(test, t);
                }
            }

            foreach (var item in testBoxes)
            {
                Outputs.Add(item.Value);
            }

            if (Children.Count() != childcount)
            {
                childcount = Children.Count();
                foreach (var item in Children)
                {
                    if (item is SaveTestBox)
                        (item as SaveTestBox).UpdateText();
                }
            }


            var goalWidth = 600f;
            var goalHeight = 0f;
            foreach (var item in Outputs)
            {
                if (item is UIBasicTextbox)
                {
                    var bt = (item as UIBasicTextbox);
                    goalWidth = Math.Max(goalWidth, FontAssets.MouseText.Value.MeasureString(bt.textToUse.Invoke()).X - 140);
                }
                goalHeight += item.GetOuterDimensions().Height + 8;
            }
            Width.Set(goalWidth, 0);



            Height.Set(goalHeight + 16, 0);
            MaxWidth.Set(0, 0.5f);
            MaxHeight.Set(-32, 1f);
            Top.Set(16, 0);
            var calcWidth = GetOuterDimensions();
            Left.Set(-(calcWidth.Width + 48 + PaddingLeft * 2 + 8), 0.75f);
            return;
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
    public class SaveTestBox : UIPanel
    {
        public BossTestData testData;

        public UIImageButtonBorder saved = new();
        public UIImageButtonBorder delete = new();
        public UIText text = new("Test List Data", 0.8f);

        public UIText TimeLabel = new("Time", 0.8f);
        public UIText BossLabel = new("Boss", 0.8f);
        public UIText NoteLabel = new("Note", 0.8f);
        public UIText GearLabel = new("Gear", 0.8f);

        public UITextInput BossName = new();
        public UITextInput Time = new();
        public UITextInput Note = new();
        public UITextInput Gear = new();

        public SaveTestBox(BossTestData data = null)
        {
            testData = data;
            Width.Set(0, 1);
            Height.Set(56 * 4, 0);
            BackgroundColor = new Color(52, 66, 119);
            Append(text);

            Main.instance.LoadItem(ItemID.FallenStar);
            saved.image = (TextureAssets.Item[ItemID.FallenStar]);
            saved.activePredicate = () => DamageStatsSystem.CurrentTestSession.bosses.Contains(testData);
            saved.OnLeftClick += (evt, el) =>
            {
                if (saved.activePredicate.Invoke())
                    DamageStatsSystem.CurrentTestSession.bosses.Remove(testData);
                else
                    DamageStatsSystem.CurrentTestSession.bosses.Add(testData);

                BossName.addInput.SetContents(testData.name);
                Time.addInput.SetContents(testData.timeString);
                Note.addInput.SetContents(testData.note);
                Gear.addInput.SetContents(testData.gear);

            };
            saved.Top.Set(0, 0);
            saved.Left.Set(-48, 1);
            Append(saved);

            Main.instance.LoadItem(ItemID.TrashCan);
            delete.image = (TextureAssets.Item[ItemID.TrashCan]);
            delete.OnLeftClick += (evt, el) =>
            {
                DataExport.testBoxes.Remove(testData);
            };
            delete.Top.Set(0, 0);
            delete.Left.Set(-48 - 56, 1);
            Append(delete);

            Append(BossLabel ??= new("Boss", 0.8f));
            Append(NoteLabel ??= new("Note", 0.8f));
            Append(TimeLabel ??= new("Time", 0.8f));
            Append(GearLabel ??= new("Gear", 0.8f));

            BossName.addInput.OnContentsChanged += (_) =>
            {
                testData?.name = _;
            };
            BossName.Height.Set(48, 0);
            Append(BossName);

            Time.addInput.OnContentsChanged += (_) =>
            {
                testData?.timeString = _;
            };

            Time.Height.Set(48, 0);
            Time.Top.Set(56, 0);
            Append(Time);

            Note.addInput.OnContentsChanged += (_) =>
            {
                testData?.note = _;
            };

            Note.Height.Set(48, 0);
            Note.Top.Set(56 * 3, 0);
            Append(Note);

            Gear.addInput.OnContentsChanged += (_) =>
            {
                testData?.gear = _;
            };
            Gear.Height.Set(48, 0);
            Gear.Top.Set(56 * 2, 0);
            Append(Gear);

            UpdateText();
        }

        public override void Update(GameTime gameTime)
        {
            testData ??= DamageStatsSystem.lastTestData;
            bool isLatest = DamageStatsSystem.lastTestData == testData;

            if (!isLatest && !Children.Contains(delete) && !saved.activePredicate.Invoke())
                Append(delete);
            else if ((isLatest && Children.Contains(delete)) || saved.activePredicate.Invoke())
                RemoveChild(delete);

            Main.instance.LoadItem(ItemID.FallenStar);
            saved.image = (TextureAssets.Item[ItemID.FallenStar]);
            saved.frame = saved.image.Frame(1, 9);
            saved.Top.Set(0, 0);

            BossName.Height.Set(48, 0);
            BossName.Left.Set(40, 0);
            BossName.Width.Set(-40, 1);
            BossName.Top.Set(56, 0);
            BossLabel.Top = BossName.Top;
            BossLabel.Height = BossName.Height;
            BossLabel.TextOriginY = 0.5f;

            Time.Left.Set(40, 0);
            Time.Width.Set(-40, 1);
            Time.Height.Set(48, 0);
            Time.Top.Set(56 * 2, 0);
            TimeLabel.Top = Time.Top;
            TimeLabel.Height = Time.Height;
            TimeLabel.TextOriginY = 0.5f;


            Note.Left.Set(40, 0);
            Note.Width.Set(-40, 1);
            Note.Height.Set(48, 0);
            Note.Top.Set(56 * 4, 0);
            NoteLabel.Top = Note.Top;
            NoteLabel.Height = Note.Height;
            NoteLabel.TextOriginY = 0.5f;

            Gear.Left.Set(40, 0);
            Gear.Width.Set(-40, 1);
            Gear.Height.Set(48, 0);
            Gear.Top.Set(56 * 3, 0);
            GearLabel.Top = Gear.Top;
            GearLabel.Height = Gear.Height;
            GearLabel.TextOriginY = 0.5f;


            Height.Set(56 * 5 + 16, 0);
            text.Top.Set(10, 0);
            text.SetText(isLatest ? "Last Test Export Details" : "", 0.75f, true);



            base.Update(gameTime);
        }

        public void UpdateText()
        {
            BossName.addInput.SetContents(testData?.name ?? "a");
            Time.addInput.SetContents(testData?.timeString ?? "0:00");
            Note.addInput.SetContents(testData?.note ?? "");
            Gear.addInput.SetContents(testData?.gear ?? "");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
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
                var t = ChatManager.GetStringSize(FontAssets.MouseText.Value, textToUse.Invoke(), Vector2.One);
                Height.Set(t.Y + 12, 0);

            }
            SetPadding(12);
            text.Width.Set(0, 1);
            text.Height.Set(0, 1);
            text.HAlign = 0;
            text.VAlign = 0;
            text.TextOriginY = 0;
            text.TextOriginX = 0;
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
        Dictionary<string, List<string>> currentClassLoadouts = new();
        public override void OnInitialize()
        {
            Main.instance.LoadItem(ItemID.WarriorEmblem);
            melee.image = TextureAssets.Item[ItemID.WarriorEmblem];
            melee.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                currentClassLoadouts = TestingLoadouts.Instance.meleeLoadouts;
                refreshSearch();
                active = 0;
            };
            melee.activePredicate = () => active == 0;

            ranged.Left.Set(52 * (1), 0);
            Main.instance.LoadItem(ItemID.RangerEmblem);
            ranged.image = TextureAssets.Item[ItemID.RangerEmblem];
            ranged.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                currentClassLoadouts = TestingLoadouts.Instance.rangerLoadouts;
                refreshSearch();
                active = 1;
            };
            ranged.activePredicate = () => active == 1;
            magic.Left.Set(52 * (2), 0);
            Main.instance.LoadItem(ItemID.SorcererEmblem);
            magic.image = TextureAssets.Item[ItemID.SorcererEmblem];
            magic.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                currentClassLoadouts = TestingLoadouts.Instance.mageLoadouts;
                refreshSearch();
                active = 2;
            };
            magic.activePredicate = () => active == 2;
            summon.Left.Set(52 * (3), 0);
            Main.instance.LoadItem(ItemID.SummonerEmblem);
            summon.image = TextureAssets.Item[ItemID.SummonerEmblem];
            summon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                currentClassLoadouts = TestingLoadouts.Instance.summonerLoadouts;
                refreshSearch();
                active = 3;
            };
            summon.activePredicate = () => active == 3;
            rogue.Left.Set(52 * (4), 0);
            if (TestingEfficiency.CalamityLoaded)
            {
                int id = TestingEfficiency.CalamityMod.Find<ModItem>("RogueEmblem").Type;
                Main.instance.LoadItem(id);
                rogue.image = TextureAssets.Item[id];
                rogue.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
                {
                    currentClassLoadouts = TestingLoadouts.Instance.rogueLoadouts;
                    refreshSearch();
                    active = 4;
                };
                rogue.activePredicate = () => active == 4;
            }

            loadouts.Top.Set(52, 0);
            loadouts.Width.Set(0, 1);
            loadouts.Height.Set(-52 * 2, 1);
            loadouts.SetScrollbar(scrollbar);

            Append(melee);
            Append(ranged);
            Append(summon);
            Append(magic);
            if (TestingEfficiency.CalamityLoaded)
                Append(rogue);
            Append(loadouts);

            addInput.Height.Set(48, 0);
            addInput.Width.Set(-52, 1);
            addInput.Top.Set(52 + 45 * 5, 0);
            addInput.Left.Set(52, 0);
            addInput.addInput.OnContentsChanged += (_) => refreshSearch();
            Append(addInput);

            addNew.Top.Set(52 + 45 * 5, 0);
            addNew.image = TextureAssets.Cursors[16];
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
                            currentClassLoadouts[addInput.searchBarText] = configCmd.Save(Main.LocalPlayer, ["melee", addInput.searchBarText]);
                            refreshSearch();
                            break;
                        case 1:
                            currentClassLoadouts[addInput.searchBarText] = configCmd.Save(Main.LocalPlayer, ["ranger", addInput.searchBarText]);
                            refreshSearch();
                            break;
                        case 2:
                            currentClassLoadouts[addInput.searchBarText] = configCmd.Save(Main.LocalPlayer, ["mage", addInput.searchBarText]);
                            refreshSearch();
                            break;
                        case 3:
                            currentClassLoadouts[addInput.searchBarText] = configCmd.Save(Main.LocalPlayer, ["summoner", addInput.searchBarText]);
                            refreshSearch();
                            break;
                        case 4:
                            currentClassLoadouts[addInput.searchBarText] = configCmd.Save(Main.LocalPlayer, ["rogue", addInput.searchBarText]);
                            refreshSearch();
                            break;
                    }
                    refreshSearch();
                }
            };
            Append(addNew);

            Width.Set(52 * 5 + 16, 0);
            Height.Set(52 * 2 + 45 * 5 + 24, 0);
            Top.Set(16, 0);
            Left.Set(-(Width.Pixels + 48 + PaddingLeft * 2 + 8), 0.75f);

            initialized = true;
        }

        void refreshSearch()
        {
            loadouts.Clear();
            foreach (var item in currentClassLoadouts)
            {
                if (addInput.searchBarText == "" || item.Value.Any(x => x.ToLower().Contains(addInput.searchBarText.ToLower())))
                    loadouts.Add(new LoadoutToPick(item));
            }
        }
        bool fals = false;
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
            LifeIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => {
                if (TestingEfficiency.CalamityLoaded)
                    Helpers.playerLifeTotal += Helpers.playerLifeTotal >= 600 ? -500 : Helpers.playerLifeTotal >= 500 ? 25 : Helpers.playerLifeTotal >= 400 ? 100 : 300;
                else
                    Helpers.playerLifeTotal += Helpers.playerLifeTotal >= 500 ? -400 : Helpers.playerLifeTotal >= 400 ? 100 : 300;
                    };
            LifeIcon.image = TextureAssets.Item[ItemID.LifeCrystal];
            Main.instance.LoadItem(ItemID.LifeCrystal);

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
            ManaIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                if (TestingEfficiency.CalamityLoaded)
                    Helpers.playerManaTotal += Helpers.playerManaTotal >= 350 ? -330 : Helpers.playerManaTotal >= 200 ? 50 : 180;
                else
                    Helpers.playerManaTotal += Helpers.playerManaTotal >= 200 ? -180 : 60;
            };
            ManaIcon.image = TextureAssets.Item[ItemID.ManaCrystal];
            Main.instance.LoadItem(ItemID.ManaCrystal);


            ManaSlider.Left.Set((32), 0f);
            ManaSlider.Width.Set(-32, 1f);
            ManaSlider.Height.Set(16, 0);
            ManaSlider.Top.Set(8 + ManaIcon.Top.Pixels, 0);
            ManaSlider.EmptyColor = Color.LightGray;
            ManaSlider.FilledColor = Color.RoyalBlue;
            ManaSlider.OnLeftClick += AdjustManaSlider;

            if (TestingEfficiency.CalamityLoaded)
            {
                int id = TestingEfficiency.CalamityMod.Find<ModItem>("MushroomPlasmaRoot").Type;
                Main.instance.LoadItem(id);
                CelestialOnionIcon.image = TextureAssets.Item[id];
                CelestialOnionIcon.activePredicate = () => Helpers.CalAccessory;
                CelestialOnionIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Helpers.CalAccessory = !Helpers.CalAccessory;
                CelestialOnionIcon.SetPadding(6);


                RageIcon.Width = RageIcon.Height = new(32, 0);
                RageIcon.Top.Set(64, 0);
                RageIcon.SetPadding(2);
                RageIcon.drawBG = false;
                RageIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.rageBoostUsed += (Helpers.rageBoostUsed < 3 ? 1 : -3);
                RageIcon.image = TextureAssets.Item[id];
                Main.instance.LoadItem(id);

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

                id = TestingEfficiency.CalamityMod.Find<ModItem>("ElectrolyteGelPack").Type;
                AdrenIcon.Width = AdrenIcon.Height = new(32, 0);
                AdrenIcon.Top.Set(96, 0);
                AdrenIcon.SetPadding(1);
                AdrenIcon.drawBG = false;
                AdrenIcon.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) => Helpers.adrenBoostUsed += (Helpers.adrenBoostUsed < 3 ? 1 : -3);
                AdrenIcon.image = TextureAssets.Item[id];
                Main.instance.LoadItem(id);

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
            }
            #endregion

            #region toggles
            Main.instance.LoadItem(ItemID.AegisCrystal);
            VitalCrystalIcon.image = TextureAssets.Item[ItemID.AegisCrystal];
            VitalCrystalIcon.activePredicate = () => Main.LocalPlayer.usedAegisCrystal;
            VitalCrystalIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAegisCrystal.Invert();

            Main.instance.LoadItem(ItemID.AegisFruit);
            AegisFruitIcon.image = TextureAssets.Item[ItemID.AegisFruit];
            AegisFruitIcon.activePredicate = () => Main.LocalPlayer.usedAegisFruit;
            AegisFruitIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAegisFruit.Invert();

            Main.instance.LoadItem(ItemID.ArcaneCrystal);
            ArcaneCrystalIcon.image = TextureAssets.Item[ItemID.ArcaneCrystal];
            ArcaneCrystalIcon.activePredicate = () => Main.LocalPlayer.usedArcaneCrystal;
            ArcaneCrystalIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedArcaneCrystal.Invert();

            Main.instance.LoadItem(ItemID.GalaxyPearl);
            GalaxyPearlIcon.image = TextureAssets.Item[ItemID.GalaxyPearl];
            GalaxyPearlIcon.activePredicate = () => Main.LocalPlayer.usedGalaxyPearl;
            GalaxyPearlIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedGalaxyPearl.Invert();

            Main.instance.LoadItem(ItemID.Ambrosia);
            AmbrosiaIcon.image = TextureAssets.Item[ItemID.Ambrosia];
            AmbrosiaIcon.activePredicate = () => Main.LocalPlayer.usedAmbrosia;
            AmbrosiaIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedAmbrosia.Invert();

            Main.instance.LoadItem(ItemID.GummyWorm);
            GummyWormIcon.image = TextureAssets.Item[ItemID.GummyWorm];
            GummyWormIcon.activePredicate = () => Main.LocalPlayer.usedGummyWorm;
            GummyWormIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.usedGummyWorm.Invert();

            Main.instance.LoadItem(ItemID.PeddlersSatchel);
            PeddlersSatchelIcon.image = TextureAssets.Item[ItemID.PeddlersSatchel];
            PeddlersSatchelIcon.activePredicate = () => NPC.peddlersSatchelWasUsed;
            PeddlersSatchelIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.peddlersSatchelWasUsed.Invert();

            Main.instance.LoadItem(ItemID.ArtisanLoaf);
            ArtisanLoafIcon.image = TextureAssets.Item[ItemID.ArtisanLoaf];
            ArtisanLoafIcon.activePredicate = () => Main.LocalPlayer.ateArtisanBread;
            ArtisanLoafIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.ateArtisanBread.Invert();

            Main.instance.LoadItem(ItemID.DemonHeart);
            DemonHeartIcon.image = TextureAssets.Item[ItemID.DemonHeart];
            DemonHeartIcon.activePredicate = () => Main.LocalPlayer.extraAccessory;
            DemonHeartIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Main.LocalPlayer.extraAccessory.Invert();

            if (TestingEfficiency.CalamityLoaded)
            {
                int id = TestingEfficiency.CalamityMod.Find<ModItem>("CelestialOnion").Type;
                Main.instance.LoadItem(id);
                CelestialOnionIcon.image = TextureAssets.Item[id];
                CelestialOnionIcon.activePredicate = () => Helpers.CalAccessory;
                CelestialOnionIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => Helpers.CalAccessory = !Helpers.CalAccessory;
                CelestialOnionIcon.SetPadding(6);
            }

            Main.instance.LoadItem(ItemID.CombatBook);
            AdvancedCombatIcon.image = TextureAssets.Item[ItemID.CombatBook];
            AdvancedCombatIcon.activePredicate = () => NPC.combatBookWasUsed;
            AdvancedCombatIcon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.combatBookWasUsed.Invert();

            Main.instance.LoadItem(ItemID.CombatBookVolumeTwo);
            AdvancedCombat2Icon.image = TextureAssets.Item[ItemID.CombatBookVolumeTwo];
            AdvancedCombat2Icon.activePredicate = () => NPC.combatBookVolumeTwoWasUsed;
            AdvancedCombat2Icon.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => NPC.combatBookVolumeTwoWasUsed.Invert();
            #endregion

            Append(LifeIcon);
            Append(LifeSlider);
            Append(ManaIcon);
            Append(ManaSlider);
            if (TestingEfficiency.CalamityLoaded)
            {
                Append(RageIcon);
                Append(RageSlider);
                Append(AdrenIcon);
                Append(AdrenSlider);
            }
            else
                miscUpgrades.Remove(CelestialOnionIcon);

            for (int i = 0; i < miscUpgrades.Count; i++)
            {
                var item = miscUpgrades[i];
                item.Top.Set(52 * (i / 4) + 36 + (TestingEfficiency.CalamityLoaded ? AdrenIcon.Top.Pixels : ManaIcon.Top.Pixels), 0);
                item.Left.Set(52 * (i % 4), 0);
                item.SetPadding(0);
                Append(item);
            }
            initialized = true;
        }
        private void AdjustLifeSlider(UIMouseEvent evt, UIElement listeningElement)
        {
            var dim = listeningElement.GetDimensions();
            Helpers.playerLifeTotal = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.05f, 0, 1) * (TestingEfficiency.CalamityLoaded ? 600 : 500));
        }

        private void AdjustManaSlider(UIMouseEvent evt, UIElement listeningElement)
        {
            var dim = listeningElement.GetDimensions();
            Helpers.playerManaTotal = (int)(MathHelper.Clamp((evt.MousePosition.X - dim.Position().X) / (float)(dim.Width) + 0.05f, 0, 1) * (TestingEfficiency.CalamityLoaded ? 350 : 200));
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

        List<List<UIImageButtonBorder>> allBosses => [
            prehardmode, hardmode, postplant, postml
            ];

        List<UIImageButtonBorder> prehardmode = [

            ];
        List<UIImageButtonBorder> hardmode = [
            ];
        List<UIImageButtonBorder> postplant = [
            ];
        List<UIImageButtonBorder> postml = [
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
            foreach (var item in allBosses)
            {
                item.Clear();   
            }
            foreach (var boss in TestingEfficiency.BossTogles.OrderBy(x => x.tier))
            {
                UIImageButtonBorder button = new();

                button.image = boss.texture();
               button.activePredicate = boss.getter;
                button.OnLeftClick += (_, _) => boss.setter(!boss.getter());

                if (boss.tier > 17)
                    postml.Add(button);

                else if (boss.tier > 12)
                    postplant.Add(button);

                else if (boss.tier > 7)
                    hardmode.Add(button);
                else
                    prehardmode.Add(button);
            }

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

            var longest = 0;
            foreach (var item in allBosses)
            {
                if (item.Count() > longest)
                    longest = item.Count();
            }

            Width.Set(52 * allBosses.Count() + 16, 0);
            Height.Set(52 * (longest + 1) + 16, 0);
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
                if (item.activePredicate?.Invoke() != active)
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
                spriteBatch.Draw(tex, dim.Center() - new Vector2(1, 0), frame, Color.White * (active ? 1 : 0.33f), 0, (frame?.Size() ?? tex.Size()) * 0.5f, MathHelper.Min(MathHelper.Min(inDim.Width / (frame?.Width ?? tex.Width), inDim.Height / (frame?.Height ?? tex.Height)), 1), 0, 0);
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

        public UISearchBar addInput = new(Language.GetText("Mods.TestingEfficiency.GUI.Loadouts.SearchText"), 1);

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

            addInput.IgnoresMouseInteraction = true;
            OnLeftClick += (evt, listen) =>
            {
                addInput.ToggleTakingText();
            };
            Append(addInput);
        }
        private void OnSearchContentsChanged(string contents)
        {
            searchBarText = contents;
        }

        private void OnStartTakingInput()
        {
            addInput.SetContents(searchBarText);
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

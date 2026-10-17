using System;
using System.Linq;
using ChaosFramework.Collections;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Core;
using ChaosFramework.Graphics;
using ChaosFramework.IO;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosFramework.Menu.OpenGl.Controls.Layout;
using ChaosFramework.Platform;
using ChaosUtil.Reflection;
using ChaosUtil.Serialization.Text;
using static ChaosFramework.Math.Clamping;

using Disabler = LD60.Settings.GuiDisabledAttribute;
using BindingFlags = System.Reflection.BindingFlags;
using Gui = LD60.Settings.GuiAttribute;
using Ref = System.Reflection;
using SysCol = System.Collections.Generic;

namespace LD60.Menu
{
    public class Settings : DialogScene
    {
        class Setting
        {
            public readonly Ref.FieldInfo field;
            public readonly Gui gui;
            public readonly Ini.FieldAttribute ini;
            public readonly ImmutableArray<Disabler> disablers;

            public readonly Control.EnableFunction enabledFunc;

            public Setting(Ref.FieldInfo field)
            {
                this.field = field;
                Type declType = field.DeclaringType;

                gui = field.GetAttributes<Gui>().Single();
                ini = Ini.GetIniAttribute(field);

                Disabler[] disablersAttrs = field.GetAttributes<Disabler>();
                LinkedList<Func<bool>> disablers = [];
                if (disablersAttrs != null && disablersAttrs.Length > 0)
                {
                    this.disablers = disablersAttrs;
                    foreach (Disabler it_disabler in disablersAttrs)
                    {
                        Disabler disabler = it_disabler;
                        Ref.FieldInfo disablerField = declType.GetField(disabler.settingName, BindingFlags.Public | BindingFlags.Instance)
                            ?? throw new InvalidOperationException($"{disabler.settingName} is not a setting.");

                        disablers.Add(() => disablerField.GetValue(Confix.instance).Equals(disabler.value));
                    }
                }

                enabledFunc = () =>
                {
                    foreach (Func<bool> disabler in disablers)
                        if (disabler())
                            return false;

                    return true;
                };
            }
        }

        readonly DialogScene owner;
        TabPageContainer settingsContainer;

        public Settings(DialogScene owner)
            : base(owner.context, new Vector2f(0.9f * owner.context.window.Ratio(), 0.9f), false)
        {
            this.owner = owner;
            panel.texture = null;
            Build();
        }

        void Build()
        {
            panel.DisposeChildren();
            BuildMenu("settings_layout", "Root/Dialog");

            Control btnCancel = panel.GetControlById("btnCancel");
            if (btnCancel != null)
                btnCancel.mouseDownLeft = (_) => CloseMenu(false);

            Control btnApply = panel.GetControlById("btnApply");
            if (btnApply != null)
                btnApply.mouseDownLeft = (_) => CloseMenu(true);

            Control btnReset = panel.GetControlById("btnReset");
            if (btnReset != null)
                btnReset.mouseDownLeft = _ =>
                {
                    string currentTab = settingsContainer.currentlyOpened.text;
                    KindlyRequestSuicide();
                    Dispose();
                    Confix.RestoreDefaults();
                    Settings dlg = new(owner);
                    dlg.ShowDialog(null, null);
                    dlg.settingsContainer[currentTab].Open();
                };

            CreatePage_Licenses();

            settingsContainer = panel.GetControlById("tabMain") as TabPageContainer;
            if (settingsContainer != null)
            {
                LinkedList<Setting> settings = [];
                foreach (Ref.FieldInfo field in typeof(LD60.Settings.ConfixClass).GetFields(BindingFlags.Instance | BindingFlags.Public))
                    if (!field.IsLiteral)
                    {
                        Setting setting = new Setting(field);
                        if (setting.gui != null)
                            settings.Add(setting);
                    }

                System.Xml.XmlNode settingsPageTemplate = ((Game)game).language.GetNode("settings_layout", "Root/SettingsPage");
                SysCol.Dictionary<string, SettingsGrid> grids = [];

                int numArbitraryTabs = settingsContainer.GetPages(0).length;
                Func<string, SettingsGrid> generatePage = tabName =>
                {
                    TabPage page = settingsContainer.CreateControl<TabPage>(0, 1, tabName);
                    Box box = page.CreateControl<Box>(0, 1);
                    box.anchor = Align.BottomTopLeftRight;
                    LayoutContainer.CreateMenuInControl(box, settingsPageTemplate, false);
                    SettingsGrid grid = page.GetControlById("settingsGrid") as SettingsGrid;
                    grids[tabName] = grid;
                    if (grid == null)
                        throw new Exception("Please provide a settingsGrid");
                    settingsContainer.InsertPage(0, settingsContainer.GetPages(0).length - 1 - numArbitraryTabs, page, false);
                    return grid;
                };

                foreach (Ref.FieldInfo tabField in typeof(LD60.Settings.Cat).GetFields(BindingFlags.Public | BindingFlags.Static))
                    generatePage((string)tabField.GetValue(null));

                foreach (Setting setting in settings)
                {
                    SettingsGrid grid;
                    if (!grids.TryGetValue(setting.gui.page, out grid))
                        grid = generatePage(setting.gui.page);

                    Type settingType = setting.field.FieldType;
                    Type gridSettingType = typeof(SettingsGrid.Setting<>).MakeGenericType(settingType);
                    Type[] gridSettingCtorArgTypes = [
                        typeof(string),
                        typeof(string),
                        typeof(Ref.FieldInfo),
                        typeof(object),
                        typeof(SettingsGrid.ControlProvider<>).MakeGenericType(settingType),
                        typeof(Control.EnableFunction)
                    ];

                    Type controlProviderType = setting.gui.controlProviderType ?? typeof(SettingsGrid.DefaultControlProvider<>).MakeGenericType(settingType);
                    Ref.ConstructorInfo controlProviderCtor = controlProviderType.GetConstructor([typeof(object[])]);
                    object controlProvider = controlProviderCtor.Invoke([setting.gui.controlProviderArgs]);

                    Ref.ConstructorInfo gridSettingCtor = gridSettingType.GetConstructor(gridSettingCtorArgTypes);
                    SettingsGrid.Setting settingsGridSetting = (SettingsGrid.Setting)gridSettingCtor.Invoke([
                        setting.field.Name,
                        setting.ini.comment,
                        setting.field,
                        Confix.instance,
                        controlProvider,
                        setting.enabledFunc
                    ]);

                    grid.settings.Add(settingsGridSetting);
                }
                using (new AccessScope<ChaosArchive>(((Game)game).archive))
                    foreach (SettingsGrid grid in grids.Values)
                        grid.Build();
            }

            panel.Layout();
        }

        public void CreatePage_Licenses()
        {
            FlowPanel flowLicenses = panel.GetControlById("flowLicenses") as FlowPanel;
            RichLabel lblLicenseText = panel.GetControlById("lblLicenseText") as RichLabel;
            ScrollBox scrollLicenses = panel.GetControlById("scrollLicenses") as ScrollBox;

            if (flowLicenses != null && lblLicenseText != null)
            {
                Vector2f originalLicenseTextSize = lblLicenseText.size;
                LinkedList<Button> btns = [];
                foreach (string itLicenseFile in System.IO.Directory.GetFiles("license").OrderBy(_ => _))
                {
                    string licenseFile = itLicenseFile;
                    Button licenseButton = flowLicenses.CreateControl<Button>(0, new Vector2f(flowLicenses.size.x, 0.05f), System.IO.Path.GetFileNameWithoutExtension(licenseFile));
                    licenseButton.mouseDown[0] += _ =>
                    {
                        if (System.IO.File.Exists(licenseFile))
                            lblLicenseText.text = '\n' + System.IO.File.ReadAllText(licenseFile);
                        else
                            lblLicenseText.text = "\n\n\tCannot find license...\n\tMaybe it was moved or deleted.";

                        lblLicenseText.GetParent<LayoutContainer>().Layout();

                        var licenseTextFlow = lblLicenseText.GetParent<LayoutContainer>();
                        float textWidth = lblLicenseText.textGeometry.geo.geometryBounds.width * originalLicenseTextSize.x;
                        float flowWidth = (licenseTextFlow.width - licenseTextFlow.marginRight - licenseTextFlow.marginLeft);

                        var licenseTextScroll = lblLicenseText.GetParent<ScrollBox>();
                        if (licenseTextScroll != null)
                            licenseTextScroll.enableScrollX = false;

                        float scale = Min(1, flowWidth / textWidth);
                        lblLicenseText.size = originalLicenseTextSize * scale;
                    };
                    btns.Add(licenseButton);
                }
                flowLicenses.Layout();
                scrollLicenses.scrollMargin = new Vector2f(float.NaN, scrollLicenses.scrollMargin.y);
                if (scrollLicenses.hasScrollY)
                    foreach (Button btn in btns)
                        btn.size = new Vector2f(btn.size.x - ScrollBox.SCROLL_BAR_HEIGHT, btn.size.y);

                if (btns.length > 0)
                    if (btns.first != null)
                        btns.first.mouseDown[0](btns.first);
            }
        }

        public void CloseMenu(bool saveChanges)
        {
            if (saveChanges)
                Confix.Save();
            else
                Confix.Load();

            KindlyRequestSuicide();
        }
    }
}

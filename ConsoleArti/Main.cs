using System;
using System.Collections.Generic;
using RoR2;
using EntityStates.Mage;
using EntityStates;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using InLobbyConfig.Fields;
using InLobbyConfig;
using ConsoleArti;

namespace ConsoleArti
{
    [BepInPlugin("com.FMRadio11.ENVToggle", "ENV_Toggle", "1.0.2")]
    [BepInDependency("com.KingEnderBrine.InLobbyConfig")]
    class Main : BaseUnityPlugin
    {
        public void Awake()
        {
            Debug.LogWarning("Note that this mod's config is located in GamepadHover.cfg! I forgot to rename it before pushing 1.0.0, so I've left it as is for now.");
            On.RoR2.BodyCatalog.Init += orig =>
            {
                orig();
                artiPrefab = BodyCatalog.FindBodyPrefab("MageBody");
                artiMachine = EntityStateMachine.FindByCustomName(artiPrefab, "Body");
            };
            On.EntityStates.Mage.FlyUpState.OnExit += (orig, self) =>
            {
                if (ToggleOption.Value && AutoSurge.Value) MageToggleCharcterMain.delayHover = true;
                orig(self);
            };
            Run.onRunStartGlobal += GHCheck;
        }
        public void Start()
        {
            GHConfig = new ConfigFile(Paths.ConfigPath + "\\GamepadHover.cfg", true);
            ToggleOption = GHConfig.Bind<bool>("Mechanics", "Enable Toggle?", true, "Enables the mod's changes.");
            DelayJet = GHConfig.Bind<bool>("Mechanics", "Delay Hover", true, "Toggles whether Artificer will automatically begin hovering on fall after jumping while hovering.");
            TapLength = GHConfig.Bind<float>("Mechanics", "Tap Jump Duration", 0.12f, "Determines how long jump must be held when Artificer falls while still having jumps remaining to automatically hover. If jump is released before this duration is up, she will jump instead.");
            AutoSurge = GHConfig.Bind<bool>("Mechanics", "Enable Auto Surge?", true, "If active, Artificer will always hover after using Ion Surge. If not, Artificer must be holding jump at the end of the move to hover.");
            var configEntry = ConfigFieldUtilities.CreateFromBepInExConfigFile(GHConfig, "ENV_Toggle Config");
            InLobbyConfig.ModConfigCatalog.Add(configEntry);
        }
        public void GHCheck(Run obj)
        {
            var ArtiMain = ToggleOption.Value ? new SerializableEntityStateType(typeof(MageToggleCharcterMain)) : new SerializableEntityStateType(typeof(MageCharacterMain));
            artiMachine.initialStateType = ArtiMain;
            artiMachine.mainStateType = ArtiMain;
        }
        GameObject artiPrefab;
        EntityStateMachine artiMachine;
        public static ConfigEntry<bool> ToggleOption { get; set; }
        public static ConfigEntry<bool> DelayJet { get; set; }
        public static ConfigEntry<float> TapLength { get; set; }
        public static ConfigEntry<bool> AutoSurge { get; set; }
        ConfigFile GHConfig;
    }
}

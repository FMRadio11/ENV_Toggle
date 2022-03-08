using System;
using System.Collections.Generic;
using RoR2;
using EntityStates.Mage;
using EntityStates;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
//using InLobbyConfig.Fields;
//using InLobbyConfig;
using ConsoleArti;

namespace ConsoleArti
{
    [BepInPlugin("com.FMRadio11.ENVToggle", "ENV_Toggle", "1.0.6")]
    //[BepInDependency("com.KingEnderBrine.InLobbyConfig")]
    class Main : BaseUnityPlugin
    {
        public void Awake()
        {
            DelayJet = base.Config.Bind<bool>("Mechanics", "Delay Hover", true, "Toggles whether Artificer will automatically begin hovering on fall after jumping while hovering.");
            TapLength = base.Config.Bind<float>("Mechanics", "Tap Jump Duration", 0.12f, "Determines how long jump must be held when Artificer falls while still having jumps remaining to automatically hover. If jump is released before this duration is up, she will jump instead.");
            AutoSurge = base.Config.Bind<bool>("Mechanics", "Enable Auto Surge?", true, "If active, Artificer will always hover after using Ion Surge. If not, Artificer must be holding jump at the end of the move to hover.");
            HoldJump = base.Config.Bind<bool>("Mechanics", "Hold Jump to hover?", true, "If active, Artificer will automatically hover when she begins falling after a grounded jump if the button/key isn't released.");
            Grace = base.Config.Bind<float>("Mechanics", "Tap Jump delay", 0.15f, "If greater than 0, inputting jump within this time period (in seconds) will make Artificer hover automatically. If set to (or below) 0, this function is disabled.");
            On.RoR2.BodyCatalog.Init += orig =>
            {
                orig();
                artiPrefab = BodyCatalog.FindBodyPrefab("MageBody");
                artiMachine = EntityStateMachine.FindByCustomName(artiPrefab, "Body");
            };
            On.EntityStates.Mage.FlyUpState.OnExit += (orig, self) =>
            {
                if (/*ToggleOption.Value &&*/ AutoSurge.Value) MageToggleCharcterMain.delayHover = true;
                orig(self);
            };
            Run.onRunStartGlobal += GHCheck;
        }
        public void Start()
        {
            //GHConfig = new ConfigFile(Paths.ConfigPath + "\\ENV_Toggle.cfg", true);
            //ToggleOption = GHConfig.Bind<bool>("Mechanics", "Enable Toggle?", true, "Enables the mod's changes.");
            /*var configEntry = ConfigFieldUtilities.CreateFromBepInExConfigFile(GHConfig, "ENV_Toggle Config");
            InLobbyConfig.ModConfigCatalog.Add(configEntry);*/
        }
        public void GHCheck(Run obj)
        {
            var ArtiMain = /*ToggleOption.Value ? */new SerializableEntityStateType(typeof(MageToggleCharcterMain))/* : new SerializableEntityStateType(typeof(MageCharacterMain))*/;
            artiMachine.initialStateType = ArtiMain;
            artiMachine.mainStateType = ArtiMain;
        }
        GameObject artiPrefab;
        EntityStateMachine artiMachine;
        //public static ConfigEntry<bool> ToggleOption { get; set; }
        public static ConfigEntry<bool> DelayJet { get; set; }
        public static ConfigEntry<float> TapLength { get; set; }
        public static ConfigEntry<bool> AutoSurge { get; set; }
        public static ConfigEntry<bool> HoldJump { get; set; }
        public static ConfigEntry<float> Grace { get; set; }
        //ConfigFile GHConfig;
    }
}

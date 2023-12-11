using RoR2;
using EntityStates;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Bootstrap;

namespace ConsoleArti
{
    [BepInPlugin("com.FMRadio11.ENVToggle", "ENV_Toggle", "1.1.0")]
    [BepInDependency("com.Borbo.ArtificerExtended", BepInDependency.DependencyFlags.SoftDependency)]
    class Main : BaseUnityPlugin
    {
        public void Awake()
        {
            DelayJet = base.Config.Bind<bool>("Mechanics", "Delay Hover", true, "Toggles whether Artificer will automatically begin hovering on fall after jumping while hovering.");
            TapLength = base.Config.Bind<float>("Mechanics", "Tap Jump Duration", 0.12f, "Determines how long jump must be held when Artificer falls while still having jumps remaining to automatically hover. If jump is released before this duration is up, she will jump instead.");
            AutoSurge = base.Config.Bind<bool>("Mechanics", "Enable Auto Surge?", true, "If active, Artificer will always hover after using Ion Surge. If not, Artificer must be holding jump at the end of the move to hover.");
            HoldJump = base.Config.Bind<bool>("Mechanics", "Hold Jump to hover?", true, "If active, Artificer will automatically hover when she begins falling after a grounded jump if the button/key isn't released.");
            Grace = base.Config.Bind<float>("Mechanics", "Tap Jump delay", 0.15f, "If greater than 0, inputting jump within this time period (in seconds) while grounded will make Artificer hover automatically. If set to (or below) 0, this function is disabled.");
            UpDelay = base.Config.Bind<bool>("Mechanics", "Upward jump toggle?", false, "If active, Artificer will not execute Hopoo Feather jumps while rising until the button is released, and will toggle hover on/off if jump is input and held mid-jump. This is disabled by default due to not being vanilla behavior, but is recommended if you find the original execution clunky like I did.");
            AECompat = Chainloader.PluginInfos.ContainsKey("com.Borbo.ArtificerExtended");
            if (AECompat)
                AECom.AECompatibility();
            On.RoR2.BodyCatalog.Init += orig =>
            {
                orig();
                artiPrefab = BodyCatalog.FindBodyPrefab("MageBody");
                artiPrefab.AddComponent<ETHelper>();
                artiMachine = EntityStateMachine.FindByCustomName(artiPrefab, "Body");
            };
            On.EntityStates.Mage.FlyUpState.OnExit += (orig, self) =>
            {
                if (AutoSurge.Value) self.gameObject.GetComponent<ETHelper>().delayHover = true;
                orig(self);
            };
            // This hook does nothing if ArtificerExtended is active, see AECom for the code that's run instead
            if (!AECompat) Run.onRunStartGlobal += GHCheck;
        }
        public void GHCheck(Run obj)
        {
            var ArtiMain = new SerializableEntityStateType(typeof(MageToggleCharcterMain));
            artiMachine.initialStateType = ArtiMain;
            artiMachine.mainStateType = ArtiMain;
        }
        GameObject artiPrefab;
        internal static SkillLocator artiSkill;
        EntityStateMachine artiMachine;
        public static ConfigEntry<bool> DelayJet { get; set; }
        public static ConfigEntry<float> TapLength { get; set; }
        public static ConfigEntry<bool> AutoSurge { get; set; }
        public static ConfigEntry<bool> HoldJump { get; set; }
        public static ConfigEntry<float> Grace { get; set; }
        public static ConfigEntry<bool> UpDelay { get; set; }
        public static bool AECompat;
    }
}

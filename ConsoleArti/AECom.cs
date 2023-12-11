using ArtificerExtended;
using ArtificerExtended.Passive;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using EntityStates;

namespace ConsoleArti
{
    public static class AECom
    {
        public static void AECompatibility()
        {
            Main.artiSkill = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageBody.prefab").WaitForCompletion().GetComponent<SkillLocator>();
            PassiveSkillDef jetPassive = ScriptableObject.CreateInstance<PassiveSkillDef>();
            jetPassive.skillNameToken = Main.artiSkill.passiveSkill.skillNameToken;
            jetPassive.skillDescriptionToken = Main.artiSkill.passiveSkill.skillDescriptionToken;
            jetPassive.icon = Main.artiSkill.passiveSkill.icon;
            jetPassive.canceledFromSprinting = false;
            jetPassive.cancelSprintingOnActivation = false;
            jetPassive.stateMachineDefaults = new PassiveSkillDef.StateMachineDefaults[]
            {
                    new PassiveSkillDef.StateMachineDefaults()
                    {
                        machineName = "Body",
                        mainState = new SerializableEntityStateType(typeof(MageToggleCharcterMain)),
                        initalState = new SerializableEntityStateType(typeof(MageToggleCharcterMain)),
                        defaultMainState = new SerializableEntityStateType(typeof(GenericCharacterMain)),
                        defaultInitalState = new SerializableEntityStateType(typeof(GenericCharacterMain))
                    }
            };
            ArtificerExtendedPlugin.magePassiveFamily.variants[0].skillDef = jetPassive;
        }
    }
}

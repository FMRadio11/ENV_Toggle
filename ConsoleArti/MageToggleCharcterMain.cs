using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.Mage;
using RoR2;
using UnityEngine;

namespace ConsoleArti
{
    public class MageToggleCharcterMain : GenericCharacterMain
    {
        // Importing OnEnter/OnExit from MageCharacterMain
        public override void OnEnter()
        {
            base.OnEnter();
            this.jetpackMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jet");
        }
        public override void OnExit()
        {
            if (base.isAuthority && this.jetpackMachine)
            {
                this.jetpackMachine.SetNextState(new Idle());
            }
            base.OnExit();
        }
        public override void ProcessJump()
        {
            // Getting the needed bools out of the way here
            featherCheck = false;
            quailCheck = false;
            if (this.jumpInputReceived && base.characterBody)
            {
                // First, tracking if Artificer hits jump while still under the grace period
                if (graceTimer > 0 && !delayHover) delayHover = true;
                // Next, tracking if Artificer hits jump while being able to - either grounded or if she has Hopoo Feathers
                else if (this.jumpInputReceived && base.characterBody && base.characterMotor.jumpCount < base.characterBody.maxJumpCount)
                {
                    // First check is to see if she's either grounded or currently rising. If so, she jumps normally
                    if (base.characterMotor.velocity.y >= 0f || base.characterMotor.isGrounded)
                    {
                        // Jump has been put into its own void instead of being part of ProcessJump. This allows both this and FixedUpdate to call it
                        Jump();
                    }
                    // This covers her pressing jump while falling, which sets up a section in FixedUpdate
                    else
                    {
                        delayTimer = 0;
                        tapJump = true;
                    }
                    // Enables the bool that tracks holding jump if its config is enabled.
                    if (Main.HoldJump.Value && base.characterMotor.isGrounded) autoJump = true;
                }
                // This next part covers Artificer pressing jump when she has no jumps remaining, but is not grounded
                else if (base.characterMotor.jumpCount == base.characterBody.maxJumpCount && !base.characterMotor.isGrounded)
                {
                    // Setting up a bool for this in advance since three other things call it
                    bool jetCheck = this.jetpackMachine.state.GetType() == typeof(JetpackOn);
                    // First up, this tracks if she's falling with the jetpack off, and turns it on if so
                    if (base.characterMotor.velocity.y < 0f && !jetCheck) this.jetpackMachine.SetNextState(new JetpackOn());
                    // This instead tracks if she is currently rising from her last jump without having the delayed hover component that's explained in FixedUpdate
                    else if (!jetCheck && !delayHover && Main.DelayJet.Value) delayHover = true;
                    else
                    {
                        // This covers if she has the delayed hover component described below
                        if (delayHover == true) delayHover = false;
                        // Finally, this turns off the jetpack if she had it on
                        if (jetCheck) this.jetpackMachine.SetNextState(new Idle());
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            if (graceTimer > 0) graceTimer -= Time.fixedDeltaTime;
            // Tracks if jump is held through when Artificer starts falling; if so, she'll hover automatically.
            if (outer.commonComponents.inputBank.jump.justReleased && autoJump) autoJump = false;
            else if (delayHover && !base.characterMotor.isGrounded) autoJump = false;
            if (!base.characterMotor.isGrounded && autoJump && !tapJump && base.characterMotor.velocity.y < 0f)
            {
                autoJump = false;
                this.jetpackMachine.SetNextState(new JetpackOn());
            }
            // The delayed hover component
            if (Main.DelayJet.Value)
            {
                // Disables the delayed hover if she somehow touches the ground without falling; this could happen if she's hugging a wall
                if (delayHover && base.characterMotor.isGrounded) delayHover = false;
                // If the delayed hover component was set before, this tracks to see if she begins falling
                if (delayHover && base.characterMotor.velocity.y < 0f)
                {
                    // Turning it off so it doesn't continuously loop, then turning on jetpack
                    delayHover = false;
                    this.jetpackMachine.SetNextState(new JetpackOn());
                }
            }
            // This covers the situation where Artificer has additional jumps and is falling while inputting jump
            if (tapJump)
            {
                // First up, a timer is set to track how long jump is held in
                delayTimer += Time.fixedDeltaTime;
                // If jump is released before the duration is up, this runs the normal jump void
                if (this.outer.commonComponents.inputBank.jump.justReleased)
                {
                    Jump();
                    tapJump = false;
                }
                // If it's held for the entire duration, this sets her to turn the jetpack either off or on
                else if (delayTimer >= Main.TapLength.Value)
                {
                    EntityState jetState = this.jetpackMachine.state;
                    if (jetState != null && jetState.GetType() == typeof(JetpackOn)) this.jetpackMachine.SetNextState(new Idle());
                    else if (jetState != null) this.jetpackMachine.SetNextState(new JetpackOn());
                    tapJump = false;
                    delayTimer = 0;
                }
            }
            // This turns off the jetpack if she has it active when touching the ground; otherwise, it'd just stay on
            if (base.characterMotor.isGrounded && jetpackMachine.state.GetType() == typeof(JetpackOn))
            {
                this.jetpackMachine.SetNextState(new Idle());
                delayHover = false;
            }
            base.FixedUpdate();
        }
        private void Jump()
        {
            // Almost entirely identical to vanilla jump behavior, so I won't make comments here if unneeded
            int itemCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
            float HB = 1;
            float VB = 1;
            if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
            {
                featherCheck = true;
                HB = 1.5f;
                VB = 1.5f;
            }
            else if ((float)itemCount > 0f && base.characterBody.isSprinting)
            {
                float num = base.characterBody.acceleration * base.characterMotor.airControl;
                if (base.characterBody.moveSpeed > 0f && num > 0f)
                {
                    quailCheck = true;
                    float num2 = Mathf.Sqrt(10f * (float)itemCount / num);
                    float num3 = base.characterBody.moveSpeed / num;
                    HB = (num2 + num3) / num3;
                }
            }
            // This bit tracks if she already had jetpack on before jumping, and sets up the delayed hover component if she did
            if (this.jetpackMachine.state.GetType() == typeof(JetpackOn))
            {
                if (Main.DelayJet.Value) delayHover = true;
                this.jetpackMachine.SetNextState(new Idle());
            }
            ApplyJumpVelocity(base.characterMotor, base.characterBody, HB, VB, false);
            if (this.hasModelAnimator)
            {
                int layerIndex = base.modelAnimator.GetLayerIndex("Body");
                if (layerIndex >= 0)
                {
                    if (base.characterMotor.jumpCount == 0 || base.characterBody.baseJumpCount == 1)
                    {
                        base.modelAnimator.CrossFadeInFixedTime("Jump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                    }
                    else
                    {
                        base.modelAnimator.CrossFadeInFixedTime("BonusJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                    }
                }
            }
            if (featherCheck)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                {
                    origin = base.characterBody.footPosition
                }, true);
            }
            else if (base.characterMotor.jumpCount > 0)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
                {
                    origin = base.characterBody.footPosition,
                    scale = base.characterBody.radius
                }, true);
            }
            if (quailCheck)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                {
                    origin = base.characterBody.footPosition,
                    rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                }, true);
            }
            base.characterMotor.jumpCount++;
            // A grace period is added to jump code; this allows for double taps to be read as their own code instead of just being two rapidfire jumps
            if (Main.Grace.Value > 0) graceTimer = Main.Grace.Value;
        }
        private EntityStateMachine jetpackMachine;
        private bool featherCheck;
        private bool quailCheck;
        public static bool delayHover;
        private bool tapJump;
        private float delayTimer;
        private bool autoJump;
        private float graceTimer = 0;
    }
}

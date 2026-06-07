using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;
using ReLogic.Content;

namespace IAmLostInASea.Content.NPCs.Bosses.Test
{
    public class Test : ModNPC
    {
        //States
        enum EnemyState
        {
            Resting,
            Dashing
        }

        enum Frame
        {
            observing,
            biting,
            smiling,
        }

        //AI values
        Vector2 savePlayerPosition;

        public int saveDirection;

        public float roarTimer = 120f;
        public float restingStateSwitch = 150f;
        public float playerPositionTimer = 55f;
        public float dashTimer = 60f;
        public float slowDownTimer = 90f;
        public float loopTimer = 120f;

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float DashCount => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            //ints
            writer.Write(saveDirection);

            //Vectors
            writer.WriteVector2(savePlayerPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            //ints
            saveDirection = reader.ReadInt32();

            //Vectors
            savePlayerPosition = reader.ReadVector2();
        }

        public override void SetDefaults()
        {
            NPC.width = 118;
            NPC.height = 180;
            NPC.lifeMax = 2800;
            NPC.damage = 15;
            NPC.defense = 12;
            NPC.npcSlots = 8f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath17;
            NPC.value = 500f;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);

            switch (AIState)
            {
                case (float)EnemyState.Resting:
                    Resting();
                    break;
                
                case (float)EnemyState.Dashing:
                    Dashing();
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            switch (AIState)
            {
                case (float)EnemyState.Resting:
                    if (AITimer >= roarTimer && AITimer <= restingStateSwitch)
                    {
                        NPC.frame.Y = (int)Frame.biting * frameHeight;
                    }
                    else
                    {
                        NPC.frame.Y = (int)Frame.observing * frameHeight;
                    }
                    break;
                
                case (float)EnemyState.Dashing:
                    if (AITimer >= playerPositionTimer && AITimer <= dashTimer)
                    {
                        NPC.frame.Y = (int)Frame.smiling * frameHeight;
                    }
                    else if (AITimer >= dashTimer && AITimer <= loopTimer)
                    {
                        NPC.frame.Y = (int)Frame.biting * frameHeight;
                    }
                    else
                    {
                        NPC.frame.Y = (int)Frame.observing * frameHeight;
                    }
                    break;
            }
        }

        public void Resting()
        {
            LookAtPlayer();

            AITimer++;

            //Slow down
            if (NPC.velocity != Vector2.Zero)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 1f / 10);
            }
            
            //Roar
            if (AITimer == roarTimer)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath30, NPC.Center);
            }

            //State change
            if (AITimer == restingStateSwitch)
            {
                AIState = (float)EnemyState.Dashing;
                AITimer = dashTimer / 2; //The enemy will dash shortly after dashing
            }
        }

        public void Dashing()
        {
            AITimer++;

            //Dash 3 times
            if (DashCount < 3)
            {
                //Look at the player while the boss is preparing to charge
                if (AITimer >= 0 && AITimer <= dashTimer)
                {
                    LookAtPlayer();
                }
                
                //Save player position
                if (AITimer == playerPositionTimer)
                {
                    savePlayerPosition = Main.player[NPC.target].Center;
                }

                //Dash
                if (AITimer == dashTimer)
                {
                    saveDirection = NPC.spriteDirection;

                    Vector2 ChargeDirection = savePlayerPosition - NPC.Center;
                    ChargeDirection.Normalize();

                    NPC.velocity = ChargeDirection * 15;
                }

                //Slow down after a bit
                if (AITimer >= slowDownTimer)
                {
                    NPC.direction = saveDirection;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 1f / 10);
                }

                //Loop
                if (AITimer >= 105)
                {
                    DashCount++;
                    AITimer = 0;
                }
            }
            else
            {
                AIState = (float)EnemyState.Resting;
                DashCount = 0;
            }
        }

        public void LookAtPlayer()
        {
            //EoC rotation
            Vector2 vector = new(NPC.Center.X, NPC.Center.Y);
            float rotX = Main.player[NPC.target].Center.X - vector.X;
            float rotY = Main.player[NPC.target].Center.Y - vector.Y;
            NPC.rotation = (float)Math.Atan2(rotY, rotX) + 4.71f;

            //Sprite direction
            NPC.spriteDirection = NPC.direction;
        }
    }
}
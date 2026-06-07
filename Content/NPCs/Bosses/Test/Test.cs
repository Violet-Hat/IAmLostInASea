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
        private enum EnemyState
        {
            Resting,
            Dashing
        }

        //AI values
        Vector2 savePlayerPosition;

        public int saveDirection;

        public bool dontFacePlayer = false;

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

            //bools
            writer.Write(dontFacePlayer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            //ints
            saveDirection = reader.ReadInt32();

            //bools
            dontFacePlayer = reader.ReadBoolean();
        }

        public override void SetDefaults()
        {
            NPC.width = 59;
            NPC.height = 90;
            NPC.lifeMax = 2800;
            NPC.damage = 15;
            NPC.defense = 12;
            NPC.npcSlots = 8f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
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

        public void Resting()
        {
            LookAtPlayer();

            AITimer++;

            if (NPC.velocity != Vector2.Zero)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 1f / 15);
            }
            
            if (AITimer == 120)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath30, NPC.Center);
            }

            if (AITimer == 130)
            {
                AIState = (float)EnemyState.Dashing;
                AITimer = 0;
            }
        }

        public void Dashing()
        {
            AITimer++;

            if (DashCount < 3)
            {
                if (AITimer >= 0 && AITimer <= 60)
                {
                    LookAtPlayer();
                }
                
                if (AITimer == 30)
                {
                    savePlayerPosition = Main.player[NPC.target].Center;
                }

                if (AITimer == 60)
                {
                    saveDirection = NPC.spriteDirection;

                    Vector2 ChargeDirection = savePlayerPosition - NPC.Center;
                    ChargeDirection.Normalize();

                    NPC.velocity = ChargeDirection * 10;
                }

                if (AITimer >= 90)
                {
                    NPC.direction = saveDirection;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 1f / 15);
                }

                if (AITimer >= 120)
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
            Vector2 vector = new(NPC.Center.X, NPC.Center.Y);
            float rotX = Main.player[NPC.target].Center.X - vector.X;
            float rotY = Main.player[NPC.target].Center.Y - vector.Y;
            NPC.rotation = (float)Math.Atan2(rotY, rotX) + 4.71f;

            NPC.spriteDirection = NPC.direction;
        }
    }
}
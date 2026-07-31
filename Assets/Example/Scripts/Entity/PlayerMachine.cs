using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine machine = new StateMachine();

        protected override void Enqueue()
        {
            machine.Clear();
        }

        public void Tick()
        {
            machine.Update();
        }

        public void Update(int value)
        {
            machine.Update(value);
        }

        public void Create<T>(int value)
        {
            machine.Create<T>(owner, value);
        }

        public void Switch(int value)
        {
            machine.Switch(value);
        }
    }

    public static class MachineExtensions
    {
        private static readonly string[] Tags = new string[4];

        static MachineExtensions()
        {
            Tags[0] = TagConst.Ground;
            Tags[1] = TagConst.DashQuad;
            Tags[2] = TagConst.MoveQuad;
            Tags[3] = TagConst.Collision;
        }

        private static bool CompareTag(this Collider2D collider, params string[] tags)
        {
            foreach (var tag in tags)
            {
                if (collider.CompareTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static void MoveX(this Rigidbody machine, PlayerFeature feature, float velocityX)
        {
            var distance = Math.Abs(velocityX);
            var direction = Math.Sign(velocityX);
            if (direction != 0)
            {
                foreach (var hit in machine.collision.Boxcast(new Vector2(direction, 0), distance, LayerConst.GroundAndCollision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag(Tags))
                    {
                        if (direction > 0)
                        {
                            feature.State |= State.右墙;
                        }
                        else
                        {
                            feature.State |= State.左墙;
                        }

                        feature.JumpCount = 1;
                        feature.WallInput = -direction;
                        feature.WallTimer = Time.fixedTime + 0.1F;
                        velocityX = direction * hit.distance;
                        machine.velocityX = 0;

                    }
                }
            }

            machine.positionX += velocityX;
        }

        public static void MoveY(this Rigidbody machine, PlayerFeature feature, float velocityY)
        {
            var distance = Math.Abs(velocityY);
            var direction = Math.Sign(velocityY);
            if (direction != 0)
            {
                foreach (var hit in machine.collision.Boxcast(new Vector2(0, direction), distance, LayerConst.GroundAndCollision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag(Tags))
                    {
                        if (direction > 0)
                        {
                            feature.State |= State.头顶;
                        }
                        else
                        {
                            feature.JumpCount = 1;
                            feature.DashCount = 1;
                            feature.State |= State.地面;
                        }

                        velocityY = direction * hit.distance;
                        machine.velocityY = 0;
    
                    }
                }
            }

            if (direction < 0 && feature.Platform < Time.fixedTime)
            {
                foreach (var hit in machine.collision.Boxcast(distance, LayerConst.Collision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag(TagConst.Platform))
                    {
                        feature.JumpCount = 1;
                        feature.DashCount = 1;
                        feature.State |= State.平台;
                        velocityY = direction * hit.distance;
                        machine.velocityY = 0;
                    }
                }
            }

            machine.positionY += velocityY;
        }
    }
}
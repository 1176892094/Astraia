using System;
using System.Linq;
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
        private static readonly string[] Tags = new string[3];

        static MachineExtensions()
        {
            Tags[0] = "Ground";
            Tags[1] = "DashQuad";
            Tags[2] = "Collision";
        }

        private static bool CompareTag(this Collider2D collider, params string[] tags)
        {
            return tags.Any(collider.CompareTag);
        }

        public static void MoveX(this Rigidbody machine, PlayerFeature feature, float value)
        {
            var moveX = Math.Abs(value);
            var signX = Math.Sign(value);
            if (signX != 0)
            {
                foreach (var hit in machine.collision.Boxcast(new Vector2(signX, 0), moveX, LayerConst.GroundAndCollision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag(Tags))
                    {
                        if (signX > 0)
                        {
                            feature.State |= State.右墙;
                        }
                        else
                        {
                            feature.State |= State.左墙;
                        }

                        feature.JumpCount = 1;
                        feature.WallInput = -signX;
                        feature.WallTimer = Time.fixedTime + 0.1F;
                        value = signX * hit.distance;
                        machine.velocityX = value;
                    }
                }
            }

            machine.positionX += value;
        }

        public static void MoveY(this Rigidbody machine, PlayerFeature feature, float value)
        {
            var moveY = Math.Abs(value);
            var signY = Math.Sign(value);
            if (signY != 0)
            {
                foreach (var hit in machine.collision.Boxcast(new Vector2(0, signY), moveY, LayerConst.GroundAndCollision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag(Tags))
                    {
                        if (signY > 0)
                        {
                            feature.State |= State.头顶;
                        }
                        else
                        {
                            feature.JumpCount = 1;
                            feature.DashCount = 1;
                            feature.State |= State.地面;
                        }

                        value = signY * hit.distance;
                        machine.velocityY = value;
                    }
                }
            }

            if (signY < 0 && feature.Platform < Time.fixedTime)
            {
                foreach (var hit in machine.collision.Boxcast(moveY, LayerConst.Collision))
                {
                    if (hit.distance >= 0 && hit.collider.CompareTag("Platform"))
                    {
                        feature.JumpCount = 1;
                        feature.DashCount = 1;
                        feature.State |= State.平台;
                        value = signY * hit.distance;
                        machine.velocityY = value;
                    }
                }
            }

            machine.positionY += value;
        }
    }
}
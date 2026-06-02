using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Example.Scripts.Physics
{
    [Flags]
    public enum State
    {
        上墙 = 1 << 0,
        下墙 = 1 << 1,
        左墙 = 1 << 2,
        右墙 = 1 << 3,
        碰撞 = 上墙 | 下墙 | 左墙 | 右墙,
    }

    [Serializable]
    public class Character : MonoBehaviour
    {
        public State state;
        public int moveX;
        public int moveY;
        public int moveSpeed = 1;

        public Tilemap composite;
        public Rigidbody movement;
        public List<Rigidbody> rigidbodies;

        private void Update()
        {
            moveX = Math.Sign(Input.GetAxisRaw("Horizontal"));
            moveY = Math.Sign(Input.GetAxisRaw("Vertical"));
        }

        private void FixedUpdate()
        {
            state &= ~State.碰撞;
            var direction = new Vector2(moveX, moveY).normalized;
            movement.VelocityX = Mathf.RoundToInt(direction.x * moveSpeed);
            movement.VelocityY = Mathf.RoundToInt(direction.y * moveSpeed);
            var velocityX = Mathf.Abs(movement.VelocityX);
            var velocityY = Mathf.Abs(movement.VelocityY);
            var collisions = movement.GetContacts(composite);
            foreach (var rigid in rigidbodies)
            {
                if (movement != rigid)
                {
                    collisions.Add(rigid);
                }
            }

            foreach (var collision in collisions)
            {
                if (moveX != 0)
                {
                    var stepX = movement.BoxCast(collision, new Vector2(moveX, 0), velocityX);
                    if (stepX >= 0 && stepX <= velocityX)
                    {
                        state |= moveX > 0 ? State.右墙 : State.左墙;
                        movement.VelocityX = moveX * stepX;
                    }
                }
            }

            movement.PositionX += movement.VelocityX;
            foreach (var collision in collisions)
            {
                if (moveY != 0)
                {
                    var stepY = movement.BoxCast(collision, new Vector2(0, moveY), velocityY);
                    if (stepY >= 0 && stepY <= velocityY)
                    {
                        state |= moveY > 0 ? State.上墙 : State.下墙;
                        movement.VelocityY = moveY * stepY;
                    }
                }
            }

            movement.PositionY += movement.VelocityY;
            movement.MovePosition();
        }
    }
}
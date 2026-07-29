using System;
using UnityEngine;

namespace Runtime
{
    public class Test : MonoBehaviour
    {
        public BoxCollider2D box1;
        public BoxCollider2D box2;

        private void FixedUpdate()
        {
            if (box1 && box2)
            {
                var result = Physics2D.Distance(box1, box2);
                var reason = string.Empty;
                reason += box1.transform.position + "\t";
                reason += box2.transform.position + "\t";
                reason += result.distance + "\t";
                reason += result.pointA + "\t";
                reason += result.pointB + "\t";
                reason += result.normal + "\t";
                reason += result.isOverlapped + "\t";

                Debug.Log(reason);
            }
        }
    }
}
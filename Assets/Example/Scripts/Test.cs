using System;
using System.Collections.Generic;
using Astraia;
using Astraia.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Example.Scripts
{
    [Serializable]
    public class PlayerModule : Module<Entity>
    {
        public StateMachine<int> machine = new StateMachine<int>();
        public Blackboard<int> Blackboard = new Blackboard<int>();
        public Dictionary<int> Dictionary = new Dictionary<int>();
    }
}
// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-16 20:09:48
// // # Recently: 2025-09-16 20:09:49
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Common
{
    [Serializable]
    public struct NodeData
    {
        public List<NodeData> items;
        public string name;

        public NodeData(string name)
        {
            this.name = name;
            items = new List<NodeData>();
        }
    }
}
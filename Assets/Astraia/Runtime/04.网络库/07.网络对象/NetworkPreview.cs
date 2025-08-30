// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-19 20:08:26
// // # Recently: 2025-08-19 20:08:26
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

#if UNITY_EDITOR
using System.Collections.Generic;
using Astraia.Common;
using UnityEditor;
using UnityEngine;

namespace Astraia.Net
{
    [CustomPreview(typeof(GameObject))]
    internal class NetworkPreview : ObjectPreview
    {
        private struct AgentData
        {
            public readonly GUIContent name;
            public readonly NetworkAgent value;

            public AgentData(GUIContent name, NetworkAgent value)
            {
                this.name = name;
                this.value = value;
            }
        }

        private struct EntityData
        {
            public readonly GUIContent name;
            public readonly GUIContent value;

            public EntityData(string name, string value)
            {
                this.name = new GUIContent(name);
                this.value = new GUIContent(value);
            }
        }

        private class Styles
        {
            public readonly GUIStyle label = new GUIStyle(EditorStyles.label);
            public readonly GUIStyle agentName = new GUIStyle(EditorStyles.boldLabel);

            public Styles()
            {
                var textColor = new Color(0.7f, 0.7f, 0.7f);
                label.padding.right += 20;
                label.normal.textColor = textColor;
                label.active.textColor = textColor;
                label.focused.textColor = textColor;
                label.hover.textColor = textColor;
                label.onNormal.textColor = textColor;
                label.onActive.textColor = textColor;
                label.onFocused.textColor = textColor;
                label.onHover.textColor = textColor;

                agentName.normal.textColor = textColor;
                agentName.active.textColor = textColor;
                agentName.focused.textColor = textColor;
                agentName.hover.textColor = textColor;
                agentName.onNormal.textColor = textColor;
                agentName.onActive.textColor = textColor;
                agentName.onFocused.textColor = textColor;
                agentName.onHover.textColor = textColor;
            }
        }

        private GUIContent preview;

        private Styles _styles;
        private Styles styles => _styles ??= new Styles();

        public override GUIContent GetPreviewTitle()
        {
            return preview ??= new GUIContent("Network Preview");
        }

        public override bool HasPreviewGUI()
        {
            return target != null && target is GameObject source && source.GetComponent<NetworkEntity>();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (target != null && target is GameObject source)
            {
                if (source.TryGetComponent(out NetworkEntity entity))
                {
                    var padding = new RectOffset(-5, -5, -5, -5);
                    var newRect = padding.Add(rect);

                    var width = newRect.x + 10;
                    var height = newRect.y + 10;

                    height = DrawEntityData(entity, width, height);
                    _ = DrawAgentData(entity, width, height);
                }
            }
        }

        private float DrawEntityData(NetworkEntity entity, float width, float height)
        {
            var entityData = GetEntityData(entity);

            var entitySize = new Vector2(140, 20);
            var nameLength = GetMaxNameSize(entityData);

            var entityRect = new Rect(width, height, entitySize.x, entitySize.y);
            var nameRect = new Rect(entitySize.x, height, nameLength.x, nameLength.y + 5.5f);

            foreach (var data in entityData)
            {
                GUI.Label(entityRect, data.name, styles.label);
                GUI.Label(nameRect, data.value, styles.agentName);
                entityRect.y += entityRect.height;
                entityRect.x = width;
                nameRect.y += nameRect.height;
            }

            return entityRect.y;
        }

        private float DrawAgentData(NetworkEntity entity, float width, float height)
        {
            var agentData = GetAgentData(entity);
            var agentSize = GetMaxAgentSize(agentData);
            var agentRect = new Rect(width, height, agentSize.x, agentSize.y);

            GUI.Label(agentRect, new GUIContent("Network Agent :"), styles.label);
            agentRect.x += 20;
            agentRect.y += agentRect.height + 5f;

            foreach (var data in agentData)
            {
                if (data.value != null)
                {
                    GUI.Label(agentRect, data.name, styles.agentName);
                    agentRect.y += agentRect.height + 5f;
                    height = agentRect.y;
                }
            }

            return height;
        }

        private Vector2 GetMaxNameSize(IList<EntityData> copies)
        {
            var rect = Vector2.zero;
            foreach (var data in copies)
            {
                var size = styles.label.CalcSize(data.value);
                if (rect.x < size.x)
                {
                    rect.x = size.x;
                }

                if (rect.y < size.y)
                {
                    rect.y = size.y;
                }
            }

            return rect;
        }

        private Vector2 GetMaxAgentSize(IList<AgentData> copies)
        {
            var rect = Vector2.zero;
            foreach (var data in copies)
            {
                var size = styles.label.CalcSize(data.name);
                if (rect.x < size.x)
                {
                    rect.x = size.x;
                }

                if (rect.y < size.y)
                {
                    rect.y = size.y;
                }
            }

            return rect;
        }

        private static IList<AgentData> GetAgentData(NetworkEntity entity)
        {
            var copies = new List<AgentData>();

            if (GlobalManager.entityData.TryGetValue(entity, out var agentData))
            {
                var agents = agentData.Values;
                foreach (var agent in agents)
                {
                    if (agent is NetworkAgent result)
                    {
                        copies.Add(new AgentData(new GUIContent(result.GetType().FullName), result));
                    }
                }
            }


            return copies;
        }

        private static IList<EntityData> GetEntityData(NetworkEntity entity)
        {
            var copies = new List<EntityData>
            {
                new EntityData("Asset ID :", entity.assetId.ToString()),
                new EntityData("Scene ID :", entity.sceneId.ToString()),
                new EntityData("Object ID :", entity.objectId.ToString()),
                new EntityData("Is Owner :", entity.isOwner ? "Yes" : "No"),
                new EntityData("Is Server :", entity.isServer ? "Yes" : "No"),
                new EntityData("Is Client :", entity.isClient ? "Yes" : "No")
            };

            if (Application.isPlaying)
            {
                if (entity.connection == null)
                {
                    copies.Add(new EntityData("Client :", "Null"));
                    copies.Add(new EntityData("Client ID :", "-1"));
                }
                else
                {
                    copies.Add(new EntityData("Client :", entity.connection.ToString()));
                    copies.Add(new EntityData("Client ID :", entity.connection.clientId.ToString()));
                }
            }

            return copies;
        }
    }
}
#endif
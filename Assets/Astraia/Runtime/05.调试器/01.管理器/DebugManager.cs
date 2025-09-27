// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-16 01:12:26
// # Recently: 2024-12-22 20:12:26
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Net;
using UnityEngine;

namespace Astraia.Common
{
    [DefaultExecutionOrder(-100)]
    public partial class DebugManager : NetworkDebugger
    {
        private Font font;
        private bool maximized;
        private float frameData;
        private double frameTime;

        private Window window;
        private Rect windowRect;
        private Rect screenRect;
        private Color screenColor;
        private Vector2 screenView;
        private Vector2 windowView;
        private Vector2 screenRate;

        private float screenWidth => Screen.width / screenSize;
        private float screenHeight => Screen.height / screenSize;
        private float screenSize => Screen.width / screenRate.x + Screen.height / screenRate.y;

       protected override void Awake()
        {
            base.Awake();
            screenColor = Color.white;
            screenRate = new Vector2(2560, 1440);
            screenRect = new Rect(10, 20, 100, 60);
            font = Resources.Load<Font>("Sarasa Mono SC");
        }

        protected override void Start()
        {
            base.Start();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var sceneTypes = assembly.GetTypes();
                foreach (var sceneType in sceneTypes)
                {
                    if (!sceneType.IsSubclassOf(typeof(Component)))
                    {
                        continue;
                    }

                    if (sceneType.IsAbstract)
                    {
                        continue;
                    }

                    if (sceneType.IsGenericType)
                    {
                        continue;
                    }

                    cachedTypes.Add(sceneType);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Application.logMessageReceived += LogMessageReceived;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Application.logMessageReceived -= LogMessageReceived;
        }

        protected override void Update()
        {
            base.Update();
            if (frameTime > Time.realtimeSinceStartup)
            {
                return;
            }

            frameData = (int)(1.0 / Time.deltaTime);
            frameTime = Time.realtimeSinceStartup + 1;
        }

        private void OnGUI()
        {
            var matrix = GUI.matrix;
            var labelAlignment = GUI.skin.label.alignment;
            var fieldAlignment = GUI.skin.textField.alignment;

            GUI.matrix = Matrix4x4.Scale(new Vector3(screenSize, screenSize, 1));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

            if (font != null)
            {
                GUI.skin.font = font;
            }

            if (maximized)
            {
                var maxRect = new Rect(0, 0, screenWidth, screenHeight);
                GUI.Window(0, maxRect, MaxWindow, "调试器");
            }
            else
            {
                windowRect.size = screenRect.size;
                windowRect = GUI.Window(0, windowRect, MinWindow, "调试器");
            }

            GUI.matrix = matrix;
            GUI.skin.label.alignment = labelAlignment;
            GUI.skin.textField.alignment = fieldAlignment;
        }

        private void MaxWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUI.contentColor = screenColor;
            if (GUILayout.Button("FPS: {0}".Format(frameData), GUILayout.Height(30), GUILayout.Width(80)))
            {
                maximized = false;
            }

            GUI.contentColor = window == Window.Console ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Console.ToString(), GUILayout.Height(30)))
            {
                window = Window.Console;
            }

            GUI.contentColor = window == Window.Scene ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Scene.ToString(), GUILayout.Height(30)))
            {
                UpdateGameObject();
                UpdateComponent();
                window = Window.Scene;
            }

            GUI.contentColor = window == Window.Reference ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Reference.ToString(), GUILayout.Height(30)))
            {
                window = Window.Reference;
            }

            GUI.contentColor = window == Window.Network ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Network.ToString(), GUILayout.Height(30)))
            {
                window = Window.Network;
            }

            GUILayout.EndHorizontal();

            if (window != Window.Console)
            {
                GUILayout.BeginHorizontal();
                GUI.contentColor = window == Window.Memory ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Memory.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Memory;
                }

                GUI.contentColor = window == Window.Time ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Time.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Time;
                }

                GUI.contentColor = window == Window.System ? Color.white : Color.gray;
                if (GUILayout.Button(Window.System.ToString(), GUILayout.Height(30)))
                {
                    window = Window.System;
                }

                GUI.contentColor = window == Window.Screen ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Screen.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Screen;
                }

                GUI.contentColor = window == Window.Project ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Project.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Project;
                }

                GUILayout.EndHorizontal();
            }

            GUI.contentColor = Color.white;
            switch (window)
            {
                case Window.Console:
                    ConsoleWindow();
                    break;
                case Window.Scene:
                    SceneWindow();
                    break;
                case Window.Reference:
                    ReferenceWindow();
                    break;
                case Window.Network:
                    NetworkWindow();
                    break;
                case Window.System:
                    SystemWindow();
                    break;
                case Window.Project:
                    ProjectWindow();
                    break;
                case Window.Memory:
                    MemoryWindow();
                    break;
                case Window.Screen:
                    ScreenWindow();
                    break;
                case Window.Time:
                    TimeWindow();
                    break;
            }
        }

        private void MinWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, screenRect.width, 20f));

            GUILayout.BeginHorizontal();
            GUI.contentColor = screenColor;
            if (GUILayout.Button("FPS: {0}".Format(frameData), GUILayout.Height(30), GUILayout.Width(80)))
            {
                maximized = true;
            }

            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private enum Window
        {
            Console,
            Scene,
            Reference,
            Network,
            System,
            Project,
            Memory,
            Screen,
            Time,
        }
    }
}
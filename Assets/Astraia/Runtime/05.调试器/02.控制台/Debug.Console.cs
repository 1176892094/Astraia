// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-18 20:12:46
// # Recently: 2024-12-22 20:12:41
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private readonly Dictionary<LogType, LogData> logData = new Dictionary<LogType, LogData>
        {
            { LogType.Log, new LogData(Color.white) },
            { LogType.Warning, new LogData(Color.yellow) },
            { LogType.Exception, new LogData(Color.magenta) },
            { LogType.Error, new LogData(Color.red) },
            { LogType.Assert, new LogData(Color.green) }
        };

        private readonly List<LogMessage> messages = new List<LogMessage>();
        private int selectMessage = -1;

        private void ConsoleWindow()
        {
            ConsoleOption();
            ConsoleButton();
            ConsoleScroll();
        }

        private void LogMessageReceived(string message, string stackTrace, LogType logType)
        {
            if (!logData.TryGetValue(logType, out var data))
            {
                return;
            }

            if (messages.Count >= 300)
            {
                if (logData.TryGetValue(messages[0].logType, out var log))
                {
                    log.count--;
                }

                messages.RemoveAt(0);
            }

            messages.Add(new LogMessage(message, stackTrace, logType));

            data.count++;
            var logs = logData.Values.Reverse();
            foreach (var log in logs)
            {
                if (log.count > 0)
                {
                    screenColor = log.color;
                    break;
                }
            }
        }

        private void ConsoleButton()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30)))
            {
                selectMessage = -1;
                foreach (var data in logData.Values)
                {
                    data.count = 0;
                }

                messages.Clear();
                screenColor = Color.white;
            }

            if (GUILayout.Button("Report", GUILayout.Height(30)))
            {
                var mailBody = new StringBuilder(1024);
                foreach (var message in messages)
                {
                    mailBody.Append(message + "\n\n" + message.stackTrace + "\n\n");
                }

                Service.Mail.Send(new MailData
                {
                    smtpServer = GlobalSetting.Instance.smtpServer,
                    smtpPort = GlobalSetting.Instance.smtpPort,
                    senderName = "Astraia",
                    senderAddress = GlobalSetting.Instance.smtpUsername,
                    senderPassword = GlobalSetting.Instance.smtpPassword,
                    targetAddress = GlobalSetting.Instance.smtpUsername,
                    mailName = "来自《Astraia》的调试日志:",
                    mailBody = mailBody.ToString()
                });
            }

            GUILayout.EndHorizontal();
        }

        private void ConsoleOption()
        {
            GUILayout.BeginHorizontal();
            foreach (var type in logData.Keys)
            {
                if (logData.TryGetValue(type, out var data))
                {
                    GUI.contentColor = data.status ? Color.white : Color.gray;
                    if (GUILayout.Button("{0} [{1}]".Format(type, data.count), GUILayout.Height(30)))
                    {
                        data.status = !data.status;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void ConsoleScroll()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box", GUILayout.Height(screenHeight * 0.4f));

            for (var i = 0; i < messages.Count; i++)
            {
                if (logData.TryGetValue(messages[i].logType, out var data) && data.status)
                {
                    GUILayout.BeginHorizontal();
                    GUI.contentColor = data.color;
                    if (GUILayout.Toggle(selectMessage == i, messages[i].ToString(), GUILayout.Height(20)))
                    {
                        selectMessage = i;
                    }

                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            windowView = GUILayout.BeginScrollView(windowView, "Box");
            if (selectMessage != -1)
            {
                GUILayout.Label(messages[selectMessage].message + "\n\n" + messages[selectMessage].stackTrace);
            }

            GUILayout.EndScrollView();
        }

        [Serializable]
        private class LogData
        {
            public int count;
            public bool status;
            public Color color;

            public LogData(Color color)
            {
                this.color = color;
                status = true;
            }
        }

        [Serializable]
        private struct LogMessage
        {
            public string message;
            public string stackTrace;
            public LogType logType;
            public DateTime dateTime;

            public LogMessage(string message, string stackTrace, LogType logType)
            {
                this.logType = logType;
                this.message = message;
                this.stackTrace = stackTrace;
                dateTime = DateTime.Now;
            }

            public override string ToString()
            {
                return "[{0}] [{1}] {2}".Format(dateTime.ToString("HH:mm:ss"), logType, message);
            }
        }
    }
}
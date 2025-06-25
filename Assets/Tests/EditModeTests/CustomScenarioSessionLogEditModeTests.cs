using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class CustomScenarioSessionLogEditModeTests
    {
        [Test]
        public void TestSessionLogFilename()
        {
            var customScenario = "CustomScenario:RealSceneName.json.extra.data";

            var systemManager = SystemManager.GetDefault();

            var filename = SessionLog.GenerateFilename(customScenario, "SessionName", systemManager.SystemConfig.SessionLogsFolder);

            Debug.Log(filename);
        }
    }
}
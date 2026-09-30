using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using FarmingEngine;

namespace FarmingEngine.Tests
{
    /// <summary>
    /// 场景冒烟测试：加载每个场景，运行若干帧，收集错误
    /// </summary>
    public class SceneSmokeTests
    {
        private static readonly List<string> errorLogs = new List<string>();

        private static void CleanSaves()
        {
            PlayerData.Unload();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            foreach (string file in SaveTool.GetAllSave(PlayerData.extension))
                SaveTool.DeleteFile(file);
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                string entry = $"[{type}] {condition}\n{stackTrace}";
                if (!errorLogs.Contains(entry))
                    errorLogs.Add(entry);
            }
        }

        private static IEnumerator RunScene(string sceneName, int frames)
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;

            CleanSaves();

            // 同步加载：同一帧内卸载旧场景并加载新场景，避免旧场景对象在 PlayerData 清空后残留运行
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            yield return null; // Start

            for (int i = 0; i < frames; i++)
            {
                yield return null;
                if (errorLogs.Count > 50) break;
            }

            Application.logMessageReceived -= OnLogMessage;

            if (errorLogs.Count > 0)
            {
                Assert.Fail($"场景 {sceneName} 出现 {errorLogs.Count} 个错误:\n{string.Join("\n---\n", errorLogs)}");
            }
        }

        [UnityTest]
        public IEnumerator FarmScene_RunsWithoutErrors()
        {
            yield return RunScene("Farm", 180);
        }

        [UnityTest]
        public IEnumerator HouseScene_RunsWithoutErrors()
        {
            yield return RunScene("House", 120);
        }

        [UnityTest]
        public IEnumerator MineScene_RunsWithoutErrors()
        {
            yield return RunScene("Mine", 120);
        }

        [UnityTest]
        public IEnumerator TestScene_RunsWithoutErrors()
        {
            yield return RunScene("Test", 120);
        }

        [UnityTest]
        public IEnumerator NewGame_InitializesCorrectly()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;

            CleanSaves();

            SceneManager.LoadScene("Farm", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;

            PlayerData pdata = PlayerData.Get();
            Assert.IsNotNull(pdata, "PlayerData 未初始化");
            Assert.AreEqual(1, pdata.day, "新游戏应从第一天开始");
            Assert.IsTrue(pdata.day_time > 0f, "新游戏应有时刻");

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "场景中应存在玩家角色");
            Assert.IsNotNull(TheGame.Get(), "TheGame 实例不存在");

            Application.logMessageReceived -= OnLogMessage;
            if (errorLogs.Count > 0)
                Assert.Fail($"新游戏初始化出现错误:\n{string.Join("\n---\n", errorLogs)}");
        }

        [UnityTest]
        public IEnumerator SaveAndReload_Works()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;

            CleanSaves();

            SceneManager.LoadScene("Farm", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;

            TheGame game = TheGame.Get();
            Assert.IsNotNull(game, "TheGame 实例不存在");
            game.Save();
            Assert.IsTrue(SaveTool.DoesFileExist(PlayerData.GetLastSave() + PlayerData.extension), "存档文件应存在");

            PlayerData.Unload();

            SceneManager.LoadScene("Farm", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;

            Assert.IsNotNull(PlayerData.Get(), "重新加载后 PlayerData 不应为空");

            Application.logMessageReceived -= OnLogMessage;
            if (errorLogs.Count > 0)
                Assert.Fail($"保存/读取测试出现错误:\n{string.Join("\n---\n", errorLogs)}");
        }
    }
}

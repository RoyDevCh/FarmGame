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
    /// 深度交互测试：模拟玩家真实操作，验证核心游戏逻辑。
    /// 每个测试独立运行（Test Framework 会在测试间重置场景）。
    /// </summary>
    public class GameplayInteractionTests
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

        private static IEnumerator LoadFarm()
        {
            CleanSaves();
            SceneManager.LoadScene("Farm", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;
        }

        private static void Check(string name, bool condition, string failMsg)
        {
            if (!condition)
                Assert.Fail($"检查失败: {name}: {failMsg}");
        }

        private static IEnumerator AssertNoErrors(string context)
        {
            yield return null;
            if (errorLogs.Count > 0)
                Assert.Fail($"{context} 出现运行时错误:\n{string.Join("\n---\n", errorLogs)}");
        }

        [UnityTest]
        public IEnumerator PlayerCanMove()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");
            Vector3 start = player.transform.position;

            player.MoveTo(start + new Vector3(2f, 0f, 2f));
            yield return null;
            yield return null;

            // 移动目标应该被设置
            Check("自动移动已启动", player.IsAutoMove(), "IsAutoMove 为 false");
            player.StopMove();
            yield return null;

            yield return AssertNoErrors("玩家移动");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator GainAndEatItem_Works()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");

            ItemData bread = ItemData.Get("bread");
            Assert.IsNotNull(bread, "Bread 物品数据不存在");

            player.Inventory.GainItem(bread, 5);
            Check("面包数量=5", player.Inventory.CountItem(bread) == 5, $"实际={player.Inventory.CountItem(bread)}");

            int slot = player.InventoryData.GetFirstItemSlot(bread.id, 99);
            Check("面包在库存中", slot >= 0, "未找到槽位");
            player.Inventory.EatItem(slot);
            yield return null;
            Check("吃后面包减少", player.Inventory.CountItem(bread) < 5, $"数量={player.Inventory.CountItem(bread)}");

            yield return AssertNoErrors("物品系统");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator EquipWeapon_Works()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");

            ItemData axe = ItemData.Get("axe");
            Assert.IsNotNull(axe, "Axe 物品数据不存在");

            player.Inventory.GainItem(axe, 1);
            int slot = player.InventoryData.GetFirstItemSlot(axe.id, 99);
            Check("斧头在库存中", slot >= 0, "未找到槽位");
            player.Inventory.EquipItem(slot);
            yield return null;
            Check("已装备斧头", player.Inventory.HasEquippedItem(EquipSlot.Hand), "Hand 槽未装备");

            yield return AssertNoErrors("装备系统");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator PlayerTakesDamage_ReducesHealth()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");

            float hp_before = player.Attributes.GetAttributeValue(AttributeType.Health);
            player.Combat.TakeDamage(10);
            yield return null;
            float hp_after = player.Attributes.GetAttributeValue(AttributeType.Health);
            Check("受伤后HP减少", hp_after < hp_before, $"HP {hp_before}->{hp_after}");

            yield return AssertNoErrors("战斗系统");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator Crafting_SystemInitializes()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");

            List<CraftData> all = CraftData.GetAll();
            Assert.IsNotNull(all, "CraftData 为空");
            Check("有制作数据", all.Count > 0, "无任何制作配方");

            // 找到一个可以实际制作的配方（如果有材料）
            bool crafted = false;
            foreach (CraftData c in all)
            {
                if (c.craftable && player.Crafting.CanCraft(c))
                {
                    player.Crafting.StartCrafting(c);
                    crafted = true;
                    break;
                }
            }
            if (!crafted)
                Debug.Log("[TEST] 初始材料不足，跳过实际制作（仅验证系统初始化）");

            yield return AssertNoErrors("制作系统");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator SpawnDroppedItem_Works()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            ItemData bread = ItemData.Get("bread");
            Assert.IsNotNull(bread, "Bread 物品数据不存在");

            PlayerCharacter player = PlayerCharacter.GetFirst();
            Assert.IsNotNull(player, "玩家不存在");
            Vector3 pos = player.transform.position + new Vector3(1f, 0f, 1f);
            Item.Create(bread, pos, 3);
            yield return null;

            // 掉落物品应在场景中
            Item dropped = Item.GetNearest(pos, 2f);
            Check("掉落物品已生成", dropped != null, "场景中找不到掉落物品");

            yield return AssertNoErrors("掉落物品");
            Application.logMessageReceived -= OnLogMessage;
        }

        [UnityTest]
        public IEnumerator UI_PanelsShowWithoutError()
        {
            errorLogs.Clear();
            Application.logMessageReceived += OnLogMessage;
            yield return LoadFarm();

            InventoryPanel invPanel = InventoryPanel.Get();
            CraftPanel craftPanel = CraftPanel.Get();
            Assert.IsNotNull(invPanel, "背包面板不存在");
            Assert.IsNotNull(craftPanel, "制作面板不存在");

            invPanel.Show();
            yield return null;
            Check("背包面板可见", invPanel.IsVisible(), "Show 后不可见");
            invPanel.Hide();
            yield return null;

            craftPanel.Show();
            yield return null;
            craftPanel.Hide();
            yield return null;

            yield return AssertNoErrors("UI 面板");
            Application.logMessageReceived -= OnLogMessage;
        }
    }
}

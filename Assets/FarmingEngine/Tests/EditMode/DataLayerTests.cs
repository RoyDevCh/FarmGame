using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FarmingEngine;

namespace FarmingEngine.Tests
{
    /// <summary>
    /// 数据层单元测试（无需场景，EditMode 可跑）：验证存档序列化往返、库存操作、物品 ID 唯一性
    /// </summary>
    public class DataLayerTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            PlayerData.Unload();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            foreach (string file in SaveTool.GetAllSave(PlayerData.extension))
                SaveTool.DeleteFile(file);

            // 手动加载数据（EditMode 下无 TheData）
            CraftData.Load();
            ItemData.Load();
            ConstructionData.Load();
            PlantData.Load();
            CharacterData.Load();
            SpawnData.Load();
            LevelData.Load();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            PlayerData.Unload();
        }

        [Test]
        public void SaveTool_RoundTrip_Works()
        {
            PlayerData pdata = PlayerData.NewGame("test_roundtrip");
            pdata.day = 5;
            pdata.day_time = 14.5f;
            pdata.play_time = 1234.5f;
            pdata.world_seed = 42;

            PlayerData.Save("test_roundtrip", pdata);
            Assert.IsTrue(SaveTool.DoesFileExist("test_roundtrip" + PlayerData.extension), "存档文件应存在");

            PlayerData loaded = SaveTool.LoadFile<PlayerData>("test_roundtrip" + PlayerData.extension);
            Assert.IsNotNull(loaded, "存档应能读取");
            Assert.AreEqual(5, loaded.day, "day 应保持一致");
            Assert.AreEqual(14.5f, loaded.day_time, 0.001f, "day_time 应保持一致");
            Assert.AreEqual(1234.5f, loaded.play_time, 0.001f, "play_time 应保持一致");
            Assert.AreEqual(42, loaded.world_seed, "world_seed 应保持一致");

            SaveTool.DeleteFile("test_roundtrip" + PlayerData.extension);
        }

        [Test]
        public void InventoryData_AddRemove_Works()
        {
            PlayerData pdata = PlayerData.NewGame("test_inv");
            InventoryData inv = pdata.GetInventory(InventoryType.Inventory, "test_inv_uid");
            Assert.IsNotNull(inv, "库存应创建");

            ItemData item = ItemData.Get("wood");
            Assert.IsNotNull(item, "wood 物品应存在");

            // 添加
            int slot = inv.AddItem("wood", 5, 1f, UniqueID.GenerateUniqueID());
            Assert.GreaterOrEqual(slot, 0, "应有空槽位");
            Assert.AreEqual(5, inv.CountItem("wood"), "数量应为5");

            // 堆叠
            slot = inv.AddItem("wood", 3, 1f, UniqueID.GenerateUniqueID());
            Assert.AreEqual(8, inv.CountItem("wood"), "堆叠后数量应为8");

            // 移除
            inv.RemoveItem("wood", 2);
            Assert.AreEqual(6, inv.CountItem("wood"), "移除后数量应为6");

            // 移除全部
            inv.RemoveItem("wood", 99);
            Assert.AreEqual(0, inv.CountItem("wood"), "移除全部后应为0");
        }

        [Test]
        public void ItemData_AllIdsUnique()
        {
            List<ItemData> items = ItemData.GetAll();
            Assert.IsNotEmpty(items, "应有物品数据");
            HashSet<string> ids = new HashSet<string>();
            foreach (ItemData item in items)
            {
                Assert.IsFalse(ids.Contains(item.id), $"物品 ID 重复: {item.id}");
                Assert.IsNotEmpty(item.id, "物品 ID 不应为空");
                ids.Add(item.id);
            }
        }

        [Test]
        public void PlantData_AllIdsUnique()
        {
            List<PlantData> plants = PlantData.GetAll();
            Assert.IsNotEmpty(plants, "应有植物数据");
            HashSet<string> ids = new HashSet<string>();
            foreach (PlantData p in plants)
            {
                Assert.IsFalse(ids.Contains(p.id), $"植物 ID 重复: {p.id}");
                Assert.IsNotEmpty(p.id, "植物 ID 不应为空");
                ids.Add(p.id);
            }
        }

        [Test]
        public void ConstructionData_AllIdsUnique()
        {
            List<ConstructionData> cons = ConstructionData.GetAll();
            Assert.IsNotEmpty(cons, "应有建筑数据");
            HashSet<string> ids = new HashSet<string>();
            foreach (ConstructionData c in cons)
            {
                Assert.IsFalse(ids.Contains(c.id), $"建筑 ID 重复: {c.id}");
                ids.Add(c.id);
            }
        }

        [Test]
        public void CraftData_AllHaveGroups()
        {
            List<CraftData> crafts = CraftData.GetAll();
            Assert.IsNotEmpty(crafts, "应有制作数据");
            foreach (CraftData c in crafts)
            {
                // 只有可制作项需要分组（用于 UI 分类）；不可制作项（如动物、纯收集物）无分组是合理的
                if (c.craftable)
                {
                    Assert.IsTrue(c.groups != null && c.groups.Length > 0, $"可制作项 {c.id} 缺少分组");
                }
            }
        }

        [Test]
        public void PlayerData_GetPlayerCharacter_AutoCreates()
        {
            PlayerData pdata = PlayerData.NewGame("test_pc");
            PlayerCharacterData cdata = pdata.GetPlayerCharacter(0);
            Assert.IsNotNull(cdata, "玩家数据应自动创建");
            Assert.IsTrue(pdata.player_characters.ContainsKey(0), "玩家应登记在字典中");
            Assert.AreEqual(0, cdata.player_id, "player_id 应为0");
        }

        [Test]
        public void PlayerData_InventoryPersistence()
        {
            PlayerData pdata = PlayerData.NewGame("test_inv2");
            InventoryData inv = pdata.GetInventory(InventoryType.Inventory, 0);
            inv.AddItem("wood", 10, 1f, UniqueID.GenerateUniqueID());

            PlayerData.Save("test_inv2", pdata);
            PlayerData loaded = SaveTool.LoadFile<PlayerData>("test_inv2" + PlayerData.extension);
            Assert.IsNotNull(loaded, "存档应能读取");

            InventoryData loadedInv = loaded.GetInventory(InventoryType.Inventory, 0);
            Assert.IsNotNull(loadedInv, "库存应恢复");
            Assert.AreEqual(10, loadedInv.CountItem("wood"), "存档后 wood 数量应保持");

            SaveTool.DeleteFile("test_inv2" + PlayerData.extension);
        }

        [Test]
        public void SaveTool_InvalidFilename_Rejected()
        {
            Assert.IsFalse(SaveTool.IsValidFilename(""), "空文件名应无效");
            Assert.IsFalse(SaveTool.IsValidFilename("a/b"), "含路径分隔符应无效");
            Assert.IsFalse(SaveTool.IsValidFilename("a\\b"), "含反斜杠应无效");
            Assert.IsTrue(SaveTool.IsValidFilename("player1"), "正常文件名应有效");
        }

        [Test]
        public void LevelData_GetLevelByXP_Monotonic()
        {
            LevelData[] all = Resources.LoadAll<LevelData>("Levels");
            if (all == null || all.Length == 0)
            {
                Assert.Ignore("无等级数据，跳过");
                return;
            }

            // 验证等级经验需求是递增的（按 level 排序后检查）
            Dictionary<string, List<LevelData>> byType = new Dictionary<string, List<LevelData>>();
            foreach (LevelData lvl in all)
            {
                if (!byType.ContainsKey(lvl.id))
                    byType[lvl.id] = new List<LevelData>();
                byType[lvl.id].Add(lvl);
            }

            foreach (var kv in byType)
            {
                kv.Value.Sort((a, b) => a.level.CompareTo(b.level));
                for (int i = 1; i < kv.Value.Count; i++)
                {
                    Assert.Greater(kv.Value[i].xp_required, kv.Value[i - 1].xp_required,
                        $"等级 {kv.Key} 的经验需求应递增 (Lv{kv.Value[i - 1].level}->Lv{kv.Value[i].level})");
                }
            }
        }
    }
}

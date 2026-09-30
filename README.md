# FarmGame（农场游戏）

基于 **Farming Engine** 框架的 3D 农场经营游戏，使用 **Unity 2021.3.45f2** 开发。

## 游戏内容

- **耕种系统**：锄地、播种、浇水、收获，多种作物随天数生长
- **制作系统**：配方分类制作（工具、建筑、食物等），熔炉冶炼、混合锅合成
- **建造系统**：放置建筑（围栏、熔炉、箱子和装饰等），建筑有耐久度
- **养殖与驯服**：喂养牲畜产出物资、驯服宠物（可协助挖掘/战斗）
- **战斗系统**：武器装备、野生动物（攻击性/逃跑型 AI）、陷阱、塔防
- **多场景探索**：农场（Farm）、房屋（House）、矿洞（Mine）、测试（Test）四个场景，出入口无缝切换
- **昼夜与天气**：游戏内时间流逝、睡觉跳过天数、天气效果
- **存档系统**：多存档位保存/读取，世界状态完整持久化（建筑、作物、掉落物、动物等）
- **完整 UI**：背包、装备栏、制作面板、商店、箱子、对话任务、手柄与移动端支持

## 环境要求

- Unity **2021.3.45f2**（含 Windows Build Support）
- .NET Framework 4.x 脚本后端（Mono）

## 打开与构建

1. 用 Unity Hub 打开本目录（首次导入会生成 `Library/`，耗时几分钟）
2. 打开场景 `Assets/FarmingEngine/Scenes/Farm.unity`，点击 Play 即可试玩
3. 构建整包：菜单或命令行执行 `BuildScript.Build`：

```bash
Unity.exe -batchmode -quit -projectPath <本项目路径> -executeMethod BuildScript.Build -logFile build.log
```

构建产物输出到 `Builds/FarmGame-final-test/FarmGame.exe`（Windows x64）。

## 运行测试

项目包含 EditMode（数据层单元测试）与 PlayMode（场景集成测试）两套测试：

```bash
# EditMode（16 项）
Unity.exe -batchmode -projectPath <本项目路径> -runTests -testPlatform EditMode -testResults results.xml -logFile test.log

# PlayMode（13 项：玩家移动/物品/装备/战斗/制作/掉落/UI/四场景冒烟/新游戏/存读档）
Unity.exe -batchmode -projectPath <本项目路径> -runTests -testPlatform PlayMode -testResults results.xml -logFile test.log
```

当前状态：**29/29 全部通过**。

## 项目结构

```
Assets/FarmingEngine/
├── Scenes/          # Farm / House / Mine / Test 四个场景
├── Scripts/
│   ├── TheGame.cs   # 游戏主管理器（时间、暂停、存读档、场景切换）
│   ├── Gameplay/    # 植物、动物、建筑、熔炉、战斗等玩法逻辑
│   ├── Player/      # 玩家角色、输入控制、背包、战斗、制作
│   ├── Data/        # ScriptableObject 数据与存档数据层
│   ├── UI/          # 背包/制作/商店/箱子等面板
│   ├── Actions/     # 交互动作（熔炼、捕捉、钓鱼等）
│   ├── Tools/       # 通用工具（存档 IO、物理、场景导航等）
│   └── Editor/      # 编辑器脚本（构建脚本等）
├── Prefabs/         # 预制体（物品、动物、建筑、UI 等）
├── Resources/       # 运行时加载的数据资产（配方、植物、天气、等级）
└── Tests/           # NUnit 测试（EditMode + PlayMode）
```

## 近期修复的 Bug（2026-09）

1. 野生动物检测威胁时误将自己设为逃跑目标，导致永久卡在逃跑状态（`AnimalWild.cs`）
2. 熔炉追加燃料时数量计算错误，可凭空复制物品（`Furnace.cs`）
3. 熔炉存档保存的是最后一次追加量而非累计量，读档后产出减少（`Furnace.cs`）
4. 混合锅支付配方成本时销毁整组物品而非所需数量（`MixingPanel.cs`）
5. 宠物挖掘协程每帧重复启动，动画与协程堆积（`Pet.cs`）
6. 图层掩码转换函数在掩码含 Default 层时死循环卡死游戏（`PhysicsTool.cs`）
7. 使用"拾取"动作于未配置掉落物的对象时空引用崩溃（`PlayerCharacterInventory.cs`）
8. 商店面板刷新遇到失效物品数据时崩溃（`ShopPanel.cs`）
9. 制作面板在玩家被销毁后空引用（`CraftPanel.cs`）
10. 场景切换后角色出生朝向计算使用了错误的位置基准（`TheGame.cs`）

## 路线图：LiveOps 活动系统设计（提案，尚未实现）

> 以下为规划中系统的设计说明，用于指导后续开发；当前版本不包含活动系统。

### 目标

在保持**单机可玩、离线可用**的前提下，让运营无需重新出包即可上线限时活动：节日庆典、双倍掉落、限时商店、连续登录奖励等。

### 核心概念

- **EventData（ScriptableObject）**：一条活动配置，存放于 `Resources/Events/`，主要字段：
  - `id`（唯一标识）、`title`、`icon`
  - 调度：`start_day` / `end_day`（游戏内天数）；可选 `real_date_start/end`（真实日历，默认关闭）
  - 类型：`Festival`（节日装饰与 NPC）、`DropBoost`（掉落倍率）、`FlashShop`（限时商店）、`LoginReward`（每日登录奖励表）
  - `effects`：复用现有 `BonusEffectData` 的效果结构，不新造奖励体系
- **EventManager**：常驻组件，与 `TheGame` 同级；订阅 `onNewDay` 在每天开始时结算活动状态，`onSkipTime`（睡觉跳时间）时同步补算，避免跳天漏发/多发
- **远程配置**：活动表 JSON 位于 `StreamingAssets/Events/events.json`；启动时尝试拉取远端版本覆盖默认值，失败则回退到打包默认——保证无网环境完整可玩

### 与现有系统的集成点

| 系统 | 集成方式 |
|---|---|
| 时间 | 复用 `TheGame` 的 `day`/`day_time`，不使用 `DateTime`，避免改档时钟回拨影响活动；倒计时 UI 扩展 `TimeClockUI` |
| 掉落 | `DropBoost` 在 `LootData` 生成入口处乘算倍率 |
| 商店 | `FlashShop` 复用 `ShopPanel` + `ShopNPC`，按活动 id 切换商品表 |
| 奖励 | 领取标记写入 `PlayerData`（如 `SetCustomBool("event_<id>_claimed_<day>")`），保证幂等，刷档也无法重复领取 |
| 天气 | 活动可引用 `WeatherEffect`，组合出"雪季钓鱼大赛"之类的联动活动 |

### 活动表示例（events.json）

```json
{
  "events": [
    {
      "id": "harvest_festival",
      "type": "Festival",
      "start_day": 28,
      "end_day": 30,
      "effects": [{ "bonus": "farming_xp", "value": 2.0 }],
      "shop_override": "festival_shop",
      "reward_table": "festival_daily"
    }
  ]
}
```

### 兼容与边界

- **存档兼容**：存档只记录活动 id 与领取标记，不序列化完整配置；活动表更新后旧档天然兼容
- **单机边界**：以"不破坏存档、奖励幂等"为目标，不做联网校验；后续如需联机，可将领取行为扩展为服务端校验的消息，接口保持不变

## 许可

仅供学习交流使用。

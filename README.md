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

## 许可

仅供学习交流使用。

# Random_FloatingTool 代码审查

## 已修复问题

1. **随机数种子重复导致抽取结果偏斜**
   - 问题：`FlashTimer_Tick` 每 20ms 都 `new Random()`，会频繁使用相同时间种子，导致结果重复。
   - 修复：改为复用同一个 `Random` 实例。

2. **数字模式边界错误和异常风险**
   - 问题：`random.Next(min, max)` 不包含上限；且当最小值大于最大值时会抛异常。
   - 修复：自动交换 `min/max`，并使用 `Next(min, max + 1)` 包含上限。

3. **空列表/越界导致崩溃**
   - 问题：`Window_Loaded`、去重开关、写日志时对 `SelectedIndex` 与列表集合缺少边界检查。
   - 修复：增加 `Items.Count` 与 `SelectedIndex` 的合法性判断。

4. **全局热键事件未解绑**
   - 问题：`ThreadPreprocessMessage` 使用匿名函数订阅，窗口关闭时无法安全解绑，可能造成泄漏和重复触发。
   - 修复：保存处理器引用并在 `Window_Closed` 中注销。

5. **Mutex 生命周期管理缺失**
   - 问题：单实例互斥量未在退出时释放/Dispose。
   - 修复：在 `App.Exit` 事件中释放并销毁 mutex。

## 仍建议优化（未改动）

1. **数据文件路径可移植性**
   - 目前数据库路径写死为 `Documents\\dev\\Random`，建议迁移到 `%LocalAppData%`，并通过配置项管理。

2. **字符串模式标识建议改枚举**
   - `currectmode` 使用字符串（且存在拼写错误），建议改为 `enum Mode { Number, List }`，减少拼写和分支错误。

3. **资源释放**
   - `ToolBox` 持有 `DatabaseService`，建议在窗口关闭时显式调用 `_db.Dispose()`。

4. **重复事件处理器**
   - `nummode_button_Click` 与 `nummode_button_MouseLeftButtonUp`、`listmode_button_Click` 与 `...MouseLeftButtonUp` 逻辑重复，可保留单一路径。

5. **异常可观测性**
   - 目前主要通过 `Debug.WriteLine` 和 `MessageBox`，建议引入结构化日志（文件日志）用于现场诊断。


## 本轮已落实的建议改动

- ✅ 建议 2（模式标识）：已将字符串模式改为 `DrawMode` 枚举，避免拼写导致的分支错误。
- ✅ 建议 3（资源释放）：已在 `ToolBox` 关闭事件中停止计时器并释放数据库连接。
- ✅ 建议 4（重复事件处理器）：已删除重复的 `Click` 模式切换处理器，仅保留实际使用的鼠标事件路径。

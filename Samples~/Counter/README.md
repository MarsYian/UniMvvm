# uGUI Counter

1. 在 Canvas 下创建 Text 和 Button，并添加 `CounterView`，连接两个 Inspector 字段。
2. 在场景对象上添加 `CounterInstaller`，将 CounterView 拖入 `view` 字段。
3. 进入 Play Mode。按钮执行 ViewModel 命令，Count 属性更新 Text。

`CounterInstaller` 创建并持有 ViewModel，`CounterView` 只负责绑定。复用或更换 ViewModel 时调用 `SetViewModel`；回收到对象池前调用 `Unbind`，再次显示时调用 `Bind`。

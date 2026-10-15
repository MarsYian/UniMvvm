# UniMvvm

轻量、显式绑定的 Unity UI MVVM 框架。核心和 Unity UI 适配分别位于独立程序集，业务 ViewModel 不依赖 UnityEngine。

## 安装

将整个 `UniMvvm` 目录复制到目标项目的 `Packages/com.unimvvm.ui`，或作为 Git/本地 UPM 包引用。运行时代码不依赖目标游戏代码；uGUI 适配器声明 `com.unity.ugui` 依赖。TMP 适配器会在项目安装 TextMeshPro 后自动编译。测试需要 Unity Test Framework。

## 程序集与命名空间

| 程序集 | 命名空间 | 职责 |
| --- | --- | --- |
| `UniMvvm` | `UniMvvm.Core`、`UniMvvm.Binding`、`UniMvvm.View`、`UniMvvm.Pooling` | 不引用 UnityEngine 的状态、命令、绑定和对象池。 |
| `UniMvvm.Unity` | `UniMvvm.Unity.Events`、`UniMvvm.Unity.Collections` | UnityEvent 和集合 View 适配。 |
| `UniMvvm.Unity.UGui` | `UniMvvm.Unity.UGui` | Text、Button、Toggle、Slider、InputField、Dropdown。 |
| `UniMvvm.Unity.Tmp` | `UniMvvm.Unity.Tmp` | TMP_Text、TMP_InputField、TMP_Dropdown。 |

## 生命周期

- View 直接实现 `IView<TViewModel>`，由 UI 组合入口创建和持有 ViewModel，并交给 View。
- 一个 View 自己持有一个 `BindingCollection`。`Bind` 新建集合并注册属性和命令；`Unbind` 调用 `Dispose` 后置空。更换 ViewModel、回收到对象池、销毁 View 时需要调用 `Unbind`。隐藏界面是否解绑由具体 UI 决定。
- Runtime 不提供 `UIView`、`BindingHost` 或隐式 GameObject 组件。`UniMvvmExample/Integration/UIViewBase.cs` 是接入项目 UI 调度框架的模板，不属于 Runtime 契约。
- 属性和命令通知应在 Unity 主线程触发。ViewModel 的销毁由创建它的调用方负责。
- 绑定回调异常会向调用方传播。列表适配器发生更新错误后通过 `IsFaulted` 和 `LastError` 暴露状态，先修复数据或回调，再调用 `Recover` 重建。

## 示例与测试

`Samples~/Counter` 是 UPM 包导入示例，包含自身的 `Integration/UIViewBase.cs` 模板。当前开发项目可直接打开 `Assets/UniMvvmExample/Counter/CounterSample.unity`。`Tests/EditMode` 覆盖 ViewModel 切换、销毁解绑、uGUI 双向绑定和命令状态，以及列表更新和失败恢复。

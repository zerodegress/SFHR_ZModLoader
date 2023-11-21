# SFHR_ZModLoader

[English](./README_EN.md)

为《战火英雄：重制版》制作的多用途Mod加载器。

## 注意

3.0.0开始迁移到IL2CPP，如更新请重新安装插件框架

## 特性

- 在游戏原版资源之外加载迷彩。
- 更多特性正在开发中……

## 安装

1. 从[这里](https://builds.bepinex.dev/projects/bepinex_be/674/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.674%2B82077ec.zip)下载BepInEx插件包（IL2CPP win x64）并解压到游戏根目录。
2. 前往本仓库的Release页面下载最新的插件包，同样解压到游戏根目录。
3. 打开游戏享受吧！

### 操作方法

将`Mod`放置在游戏根目录下的`mods`文件夹中即可加载，关于`Mod`的更多信息请参考[Mod开发](#mod开发)。

在游戏中，按下`P`键可以热重载资源，不过如果是对原版的修改（事实上目前也只有原版修改功能），一开始没有修改贴图的话不能重载，这是个技术性问题。

## Mod开发

在`ZModLoader`中，`Mod`是游戏根目录下的`mods`文件夹下的任意文件夹，称之为“**Mod文件夹**”，其中每一个Mod文件夹都包含一个`mod.json`
用来说明Mod基本信息，比如：

```json
{
    "id": "this_is_a_mod_template",
    "displayName": "This is a Mod template",
    "versionCode": 1,
    "version": "1.0.0"
}
```

- `id`:Mod的内部标识符
- `displayName`:Mod显示名称（暂时无用）
- `versionCode`:Mod内部版本号代码
- `version`:Mod显示版本号

在每一个Mod文件夹之下，都有一定的文件结构来表示Mod，比如：

Mod文件夹

|-sfh

|--camos

|---Red1

|----texture.png

|----icon.png

|----redCamo.png

|mod.json

- `sfh`:所有原版数据和资产都在这个文件夹（你也可以定制你自己的文件夹名，比如说xxddc之类的，但sfh被指定为原版的文件夹，又称**命名空间**）下，更改这个命名空间下的文件说明你将覆盖原版的数据和资产。
- `camos`:迷彩文件夹，其下每一个文件夹代表一个与之名称相同的迷彩
- `Red1`:这是原版的“邪恶”迷彩，内部名称为`Red1`
- `texture.png`:这是人物贴图文件
- `icon.png`:这是人物图标文件
- `redCamo.png`:这是迷彩层文件

（更多内容正在开发中……）

## 许可证

GPL-v3
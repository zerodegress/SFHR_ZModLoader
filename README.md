# SFHR_ZModLoader

[English](./README_EN.md)

为《战火英雄：重制版》制作的多用途Mod加载器。

## 注意

3.0.0开始迁移到IL2CPP，如更新请重新安装插件框架

## 特性

- 在游戏原版资源之外加载迷彩。
- 在游戏原版资源之外加载武器。
- 在游戏根目录下生成`DebugEmit`文件夹提供Mod开发所需的一些资料。
- 更多特性正在开发中……

## 安装

1. 从[这里](https://builds.bepinex.dev/projects/bepinex_be/674/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.674%2B82077ec.zip)下载BepInEx插件包（IL2CPP win x64）并解压到游戏根目录。
2. 前往本仓库的Release页面下载最新的插件包，同样解压到游戏根目录。
3. 打开游戏享受吧！

### 操作方法

将`Mod`放置在游戏根目录下的`mods`文件夹中即可加载，关于`Mod`的更多信息请参考[Mod开发](#mod开发)。

在游戏中，按下`P`键可以热重载资源。请注意：目前热重载仅对已经加载的Mod生效，如果你的Mod在热重载时尚未加载那么贴图可能不会被替换。(也就是说不生效的话重启即可)

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
- (Mod根目录)
  - sfh
    - camos
      - Red1
        - texture.png
        - icon.png
        - redCamo.pn
    - weapons
      - equipTexture.png
      - equipTextureAlt.png
      - menuTexture.png
      - unequipTexture.png
  - mymod
    - scripts
      - index.js
  - mod.json

- `sfh`:所有原版数据和资产都在这个文件夹（你也可以定制你自己的文件夹名，比如说`xxddc`之类的，但`sfh`被指定为原版的文件夹，又称**命名空间**）下，更改这个命名空间下的文件说明你将覆盖原版的数据和资产。
- `camos`:迷彩文件夹，其下每一个文件夹代表一个与之名称相同的迷彩
- `Red1`:这是原版的“邪恶”迷彩，内部名称为`Red1`。你可以将其更换为
- `texture.png`:这是人物贴图文件
- `icon.png`:这是人物图标文件
- `redCamo.png`:这是迷彩层文件
- `weapons`:武器文件夹，其下每一个文件夹代表一个与之名称相同的武器
- `weapons`文件夹下每一个贴图代表了武器的某种状态的贴图，目前只能确定`equipTexture.png`是装备在人物身上时显示的贴图，其他贴图请自行尝试。
- `mymod`文件夹是和`sfh`平行的命名空间，其中`mymod`替换为你的mod名称。一般来说，在原版之外新增加的东西应该增加到你专有的mod命名空间当中，比如脚本。
- `scripts`文件夹是存放脚本的文件夹，所有同命名空间的脚本都放置在此文件夹下。
- `index.js`是脚本文件的入口，每个命名空间下的`scripts/index.js`都是各自的脚本执行的入口。其余脚本文件只能被引用，不会被执行。

（更多内容正在开发中……）

## 参与贡献

参考以下步骤搭建插件开发环境：

1. 确保你安装了[BepInEx6的合适版本]((https://builds.bepinex.dev/projects/bepinex_be/674/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.674%2B82077ec.zip))。
2. 在项目根目录下建立一个新文件`.gamepath`，并在其中粘贴你的游戏根目录位置。
3. 运行`./scripts/FetchDependencies.ps1`拉取所有所需依赖
4. 运行`dotnet build`构建你的插件
5. 运行`./scripts/Deploy.ps1`将插件部署到你的游戏（你可能需要先将原来安装的插件本体删除，但是请保留插件本身单个dll之外的依赖）
6. 在你喜欢的开发环境里进行开发，比如`VSCode`（本项目当前开发主要在`VSCode`下进行）。

## 许可证

GPL-v3
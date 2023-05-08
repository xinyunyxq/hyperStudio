# HyperStudio

![Platform support](https://img.shields.io/badge/platform-windows-blue?style=flat-square)
![Built with Unity3D](https://img.shields.io/badge/Built%20with-Unity3D-lightgrey?style=flat-square)

> **Note**: This is only available on Windows.

这是一个使用AR眼镜实现多屏办公的项目，配合iddsampledriver实现多个虚拟扩展屏，并将扩展屏投射到AR眼镜，目前支持雷鸟AR眼镜.

## [Download](https://github.com/xinyunyxq/hyperStudio)
## 功能

- 实现将主屏和虚拟扩展屏桌面投射到uinty的虚拟空间中。
- 编辑模式和桌面模式，编辑模式可调节编辑屏幕面板，桌面模式移动鼠标自动跳转到屏幕中，屏幕中的鼠标操作和windows使用真实扩展屏的操作一样
- 可通过UI编辑画面

## 运行程序
### 一、安装IddSampleDriver虚拟扩展屏驱动
1. 前往https://github.com/ge9/IddSampleDriver/releases下载use option.txt版本IddSampleDriver.zip包
2. 解压缩并复制到C盘中，保证所有inf及其他的驱动文件都在C:\IddSampleDriver\下，并且没有多余的文件夹层级
3. 以管理员身份运行 *.bat 文件，将驱动程序证书添加为受信任的根证书。（建议使用管理员运行cmd进入bat所在文件夹，使用命令运行bat文件，直接运行可能不成功）。
4. 打开设备管理器，单击任何设备，然后单击“操作”菜单并单击“添加旧版硬件”
5. 选择“从列表中添加硬件（高级）”，然后选择显示适配器
6. 单击“从磁盘安装...”，然后单击“浏览...”按钮。导航到提取的文件并选择 inf 文件。
7. 禁用添加的显示适配器，循环4~6步骤添加多个IddSampleDriver驱动
### 二、运行管理软件
1. 在github中下载release
2. 以管理员身份启动iddManager.exe，非管理员无法启动/禁用驱动。
3. 选择需要启动的虚拟屏幕数量和分辨率，虚拟屏幕数量无法超过IddSampleDriver驱动数量。
4. 点击打开虚拟屏幕，屏幕可能会黑屏闪烁一下，之后在桌面右键选择显示设置查看增加的虚拟扩展屏。
5. 此时扩展屏可能处于复制模式，点击切换扩展模式切换到扩展模式，如果不成功，就直接在显示设置里面调节成扩展显示。
6. 拖动软件到AR眼镜所在的扩展屏幕，点击开启AR桌面，系统将自动识别并将AR空间程序显示在扩展屏中



>

### Set Resolution

```bash

```

> **Note**: 

### Run in a Specific Monitor

```bash

```

> **Note**: N starts from 1 instead of 0.

## How to Use



## FAQ

- Sorry this display is unsupported?
  - Go to System Setting > Display > Graphics, find/add this app, and set options to Power Saving to use the on-chip GPU.
  - Or, you can directly disable your discrete GPU.
  - https://github.com/hecomi/uDesktopDuplication/issues/30
- Virtual monitor?
  - https://www.amyuni.com/forum/viewtopic.php?t=3030
  - https://github.com/pavlobu/deskreen/discussions/86

## [CHANGELOG](https://github.com/DiscreteTom/HyperStudio/blob/main/CHANGELOG.md)

## Credit

Thanks for these cool libs:

- [uDesktopDuplication](https://github.com/hecomi/uDesktopDuplication)
- [UnityRawInput](https://github.com/Elringus/UnityRawInput)

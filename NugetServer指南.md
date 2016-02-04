# NugetServer 使用指南

## 为什么要使用Nuget
> 在我们的项目， 存在着一些公共Dll， 这些Dll被大量的项目所引用。同时这些公共dll也同时在进行版本升级， 由于缺乏版本管理，这些Dll会被到处Copy，导致各个项目所应用的版本不一致。
 
> 极端的情况是A项目和B项目都引用了一些Common Dll， 他们引用的Common Dll版本还可能不一致， 随着需求的变化，可能在某一天就会出现让A项目依赖B项目的情况。
这时我们就可能陷入dll版本陷阱中。

> 如果我们有一个好的包管理器， 当管理器中的包升级的时候，依赖这个包的项目可以得到提示，那么我们就可以这简单地让我们的项目始终依赖最新的dll版本， 可以很自然的避免版本陷阱的发生。

> 在.net的世界里， 这个包管理器就是**Nuget**

## Nuget Server搭建
> Nuget Server的搭建十分简单， 微软已经为我们什么都准备好了。

1. 在VS中创建一个Empty Web Application
2. 选择Tools > Library Package Manager > Package Manager Setting，确认Package Manager的Package Sources已经添加nuget官方源：https://www.nuget.org/api/v2/, 
VS2013以上版本应该已经集成。
3. 在步骤1新建的Web Application中的Reference上右击， 选择Manage Nuget Package
4. 在弹出的对话中， 选择Online Tab， 然后搜索NugetServer， 点击安装
5. 修改web.config的requireApiKey=False, 或者设置requireApiKey=true，则必须设置apiKey，否则Push Package会报403错误

至此， 一个NugetServer就搞定了， 很简单吧？赶紧将Server部署起来吧！！！

## 添加Nuget Server Feed
> 记得在**Nuget Server搭建**部分讲的怎么确认nuget官方源是否已添加吗？ 你已经知道怎么添加我们自己的Nuget Server源了吧。
添加好源之后， 只要把我们Nuget Package放到Server根目录的Packages的文件夹下，这个Package就可以被我们使用了

## 如何制作Nuget Package
> 在我们的项目里， 有两种dll我们需要利用Nuget来进行版本管理。
* 第三方Dll， 我们没有源码， 为了保证各个项目中引用的版本能够保持一致， 且能够同时得到更新，我们需要Nuget
* 我们自己产生的公共Dll， 大量的项目都在引用这些Dll， 我们也需要Nuget

> 在制作Nuget Package之前，我们需要下载[Nuget.exe](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe)。
下载好之后将Nuget.exe纯在的目录配置到环境变量里，以便PowerShell能够认识nuget Command

### 为第三方Dll制作Package
1. 新建lib文件夹
2. 将需要打包的dll放到lib文件夹下。放到lib文件夹下的目的是在使用Nuget添加引用后dll可以自动地添加到reference中
3. 在DOS Console中将目录跳转到lib文件夹所在的目录，执行命令 `Nuget Spec xxx.dll`
4. 上一步的命令会生成一个nuspec文件， 需要手动编辑这个文件，制定PackageID， Version等信息
5. 执行 `Nuget Pack xxx.dll.nuspec`即可
6. 执行 `Nuget Push [PackageID] -s [NugetServerUrl] [ApiKey]`推送到Nuget Server了

### 为Project制作Package
1. 将目录跳转到Solution根目录
2. 执行命令`Nuget Pack xxx.csproj -Build -Prop Configuration=Release -IncludeReferencedProjects`即可生成Package
3. 执行命令`Nuget Push [PackageID] -s [NugetServerUrl] [ApiKey]`推送到Nuget Server了


### 从Nuget Server引用Package
> 有两种方式：
* 通过Nuget Package Manager来引用
* 通过命令行来引用

> 这里讲几个常用命令行的操作， 通过Tools > Nuget Package Manager > Packge Manage Console, 打开Package Manage Console

* 查看可用的Package `Get-Package -AvailablePackage`
* 安装Package `Install-Package [PackageID] [-Version]`
* 更新Package `Update-Package`
* 卸载Package `UnInstall-Package`
* 这些命令的具体用法，可以通过 `Get-Help Command` 查找帮助
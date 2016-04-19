#### Fork 远程仓库
这个时代已经早已不是过去单打独斗的时候, 一个项目往往是多个人一起协作完成的. 我们不采用多个人在一个仓库进行代码推送, 这很可能会造成版本的混乱. 我的看法是项目应该有一个公共的仓库, 每一个开发人员去Fork这个这个公共的仓库, 然后开发人员在自己的私有仓库进行开发. 当功能完后后再将经测试的版本合并进公共仓库.
 

- 列出已经所有已经Fork的远程仓库
	``` 
	git remote -v
	origin  https://github.com/YOUR_USERNAME/YOUR_FORK.git (fetch)
	origin  https://github.com/YOUR_USERNAME/YOUR_FORK.git (push)
	```
- 为你要同步的Fork配置upstream, 以后你就可以从你Fork的分支同步版本到你自己的仓库
	```
	git remote add upstream https://github.com/ORIGINAL_OWNER/ORIGINAL_REPOSITORY.git
	```
- 验证是否配置成功

	```
	git remote -v
	origin    https://github.com/YOUR_USERNAME/YOUR_FORK.git (fetch)
	origin    https://github.com/YOUR_USERNAME/YOUR_FORK.git (push)
	upstream  https://github.com/ORIGINAL_OWNER/ORIGINAL_REPOSITORY.git (fetch)
	upstream  https://github.com/ORIGINAL_OWNER/ORIGINAL_REPOSITORY.git (push)
 	```
- 获取upstream 仓库
	```
	git fetch upstream
	```
- 切换到你的Fork的本地master分支
	```
	git checkout master
- 合并 upstream/master
	```
	git merge upstream/master
	```
- 如何有冲突, 解决冲突后, 提交你的Fork仓库.


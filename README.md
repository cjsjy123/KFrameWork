#KFrameWork

这是一套Unity逻辑框架，减少与原始逻辑代码的耦合而设计的.<br>

**By author Kubility**<br>

#API Introduction for Class:<br>
MainLoop: 主循环，负责框架相关事件的触发和框架的初始化<br>
GameFrameWork:负责框架的基本状态，负责框架的属性初始化<br>
AttributeRegister:属性注册类，附加的属性在此注册相应的逻辑<br>
SceneCtr:场景管理类，记录场景的信息，并负责场景的加载和监听<br>
ScriptLogicCtr:脚本逻辑控制器，负责给不同的脚本命令分配不同的脚本执行器执行逻辑代码<br>
GameSyncCtr：游戏同步器，负责游戏的各种逻辑同步以及记录同步的相关信息<br>
Schedule:定时器，脱离了Unity的协同程序，依赖unity的更新来每帧判断定时器事件的状态信息<br>
KObjectPool:对象池，不在内部使用泛型约束new()来达到内部的对象构造，因为new T()实际上调用的activitor.createInstance()比new的开销大约大了2-3倍，所以建议判断返回值，在代码外部使用new进行对象构造，最后舍弃的时候使用push回收<br>

**Command(脱离unity协同程序)** <br>
FrameCommand:帧命令，使得逻辑代码可以在指定的帧数过了之后执行<br>
ScriptCommand:脚本命令，负责逻辑代码消息的传递<br>
TimeCommand:时间命令，使得逻辑代码可以在指定的时间过了之后执行<br>
BatchCommand:批命令，可以使得不同的commond类按照既定的顺序依次执行<br>

**Command Note:**<br>
每个命令都可以同类型叠加，比如 timeCmd+= timeCmd; 但是拒绝不同类型叠加，不同类型叠加必须通过BatchCommand<br>

**Params(泛参数类，支持多类型的参数Read/Write)** <br>
SimpleParams:简单的泛参数类，有同类型参数和总参数上限,gc小于GenericParams<br>
GenericParams：可变泛参数类，没有同类型和总参数上限，gc略大于SimpleParams<br>

**Attrs:**<br>
Script_SharpLogicAttribute: c#脚本逻辑属性，当有同命令的scriptCmd触发的时候，会进入到对应的注册函数中<br>
Script_LuaLogicAttribute：Lua脚本逻辑属性，当有同命令的scriptCmd触发的时候，会进入到对应的注册函数中<br>

FrameWokAwakeAttribute:框架初始化事件<br>
FrameWokDestroyAttribute：框架摧毁事件<br>
FrameWokStartAttribute：框架开始事件<br>
FrameWokDevicePausedAttribute：框架暂停/唤醒事件<br>
FrameWokDeviceQuitAttribute：框架退出事件<br>
FrameWokDisableAttribute：框架未激活事件<br>
FrameWokEnableAttribute：框架激活事件<br>
FrameWokFixedUpdateAttribute：框架Fixedupdate事件<br>
FrameWokUpdateAttribute：框架Update事件<br>
FrameWokLateUpdateAttribute：框架Lateupdate事件<br>
FrameWokBeforeUpdateAttribute：框架早于Upate的BeforeUpdate事件（但是不能保证优先于所有的Update）<br>

SceneEnterAttribute：场景进入监听<br>
ScenLeaveAttribute：场景李磊监听<br>

SingleTonAttribute：单例属性，可以配置来确定单例的实例化时机，比如声明相应的静态字段<br>

**可能会引用到的库**
**ToLua**<br> 
https://github.com/topameng/tolua<br>
**protobuf-net**<br>
**gstring**<br>

**Notes:**<br>
0.0.01Version ----- 2016.10.23：<br>
基于unity5.3以上版本进行开发，所以对于其他版本可能会有API不匹配问题，后续进行兼容支持
同时因为内部大量的使用了对象池，所以很多对象在第一次使用的时候gc一般远大于后面几次，但也得益于此，后面的操作一般可减少50%-90%的gc开销，
其他功能仍在持续迭代开发中，相应的UI,RES模块，以及其他的管理模块尚未完成，此版本为开发版本并不是发布版本，可以简单试用框架的基本功能

0.0.01Version ----- 2016.10.26：<br>
为逻辑框架添加简易状态机结构，辅助ai实现

0.0.01aVersion ----- 2016.11.3：<br>
优化命令，达到0GC
![image](https://github.com/cjsjy123/KFrameWork/blob/master/screenshot/1.png)
![image](https://github.com/cjsjy123/KFrameWork/blob/master/screenshot/2.png)

# Unity使用ProtoBuf
KDGame使用了Google官方提供的ProtoBuf解析工具。在普通的.net工程里，使用该工具可以简单地通过NuGet实现。然而通过NuGet添加的包无法被Unity编辑器识别。这对编译Proto文件没有什么影响（因为编译包可以通过命令行运行），但编译的文件会因为没有Runtime而报错。

所以KDGame选择使用NuGet导入编译工具`Google.Protobuf.Tool`，然后通过命令行工具调用来进行生成。具体代码可以在`Assets/Scripts/Editor/Serialize/ProtoBuffer`内找到。

为了解决没有Runtime的问题，需要去Google官方生成工具的[Github Repository](https://github.com/protocolbuffers/protobuf/tree/main/csharp)。下载和你导入的编译工具版本相同的Release，然后依照库中的指引编译出Dll，放入Plugin文件夹中即可。此项目下路径为`Assets/Plugins/PbRuntime`。
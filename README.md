# 使用说明
- 点击[Release](https://github.com/zmsnfly/UR_TCP/releases)下载最新版本运行，软件运行需要[ .NET Framework 4.7.2](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net472)环境
- 可以在`UR_STREAM/Common/Function.cs `修改需要收集的机器人数据，所需的三个数据分别是数据名称、位数、以及需要返回的数据类型

```c++
KeyValuePairs = new Dictionary<string, KeyValuePair<int, DataType>>
{
    {"J1", new KeyValuePair<int, DataType>(32, DataType.DEG)},
    {"J2", new KeyValuePair<int, DataType>(33, DataType.DEG)},
    {"J3", new KeyValuePair<int, DataType>(34, DataType.DEG)},
    {"J4", new KeyValuePair<int, DataType>(35, DataType.DEG)},
    {"J5", new KeyValuePair<int, DataType>(36, DataType.DEG)},
    {"J6", new KeyValuePair<int, DataType>(37, DataType.DEG)},
    {"X", new KeyValuePair<int, DataType>(56, DataType.NUM)},
    {"Y", new KeyValuePair<int, DataType>(57, DataType.NUM)},
    {"Z", new KeyValuePair<int, DataType>(58, DataType.NUM)},
    {"RX", new KeyValuePair<int, DataType>(59, DataType.RAD)},
    {"RY", new KeyValuePair<int, DataType>(60, DataType.RAD)},
    {"RZ", new KeyValuePair<int, DataType>(61, DataType.RAD)}
};
```




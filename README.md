# IoTDB C# 客户端测试用例

本项目用于测试 Apache IoTDB C# 客户端的测试程序

## 目录

- [项目结构](#项目结构)
- [环境](#环境)
- [安装](#安装)
  - [Windows](#windows)
  - [Linux (Ubuntu)](#linux-ubuntu)
- [使用](#使用)
  - [基础自动化测试](#基础自动化测试)
  - [代码覆盖率测试](#代码覆盖率测试)

---

## 项目结构

```
csharp-native-api-testcase/
├── conf/
│   └── config.properties      # 配置文件
├── lib/
│   └── Apache.IoTDB.dll       # IoTDB C# 客户端库
├── tests/
│   ├── table/                 # Table 测试用例
│   └── tree/                  # Tree 测试用例
├── main.cs                    # 入口文件
├── csharp-native-api-testcase.csproj  # 项目文件
└── README.md                  # 说明文档
```

---

## 环境

### 必需环境

- .NET 8.0 SDK
- Thrift 0.14.1


### 配置

可在 `conf/config.properties` 中修改配置信息

---

## 安装

### Windows

#### 1. 安装环境

```powershell
# 安装 .NET 8.0 SDK
# 从 https://dotnet.microsoft.com/download/dotnet/8.0 下载安装
```

#### 2. 克隆项目并安装依赖

```powershell
# 【不推荐】使用最新的发版（不是最新的源码依赖）：dotnet add package Apache.IoTDB # 里面包含了 Thrift，故不需要添加， 

# 1、拉取源码，并编译生成依赖，生成的 dll 在 iotdb-client-csharp 的 src/Apache.IoTDB/bin/Release 下
git clone https://github.com/apache/iotdb-client-csharp.git
cd iotdb-client-csharp
dotnet build --configuration Release src/Apache.IoTDB/Apache.IoTDB.csproj

# 2、拉去已有测试项目
git clone https://github.com/LinJieXiao-XLJ/csharp-native-api-testcase.git
cd csharp-native-api-testcase
# 引入依赖：根据版本选择一个目录下的 dll 文件，将 dll 文件放在 C# 项目的 lib 目录中，并确保 csharp-native-api-testcase.csproj 文件中 Include 正确

# 3、添加 Thrift 包
dotnet add package Thrift.NetStd

# 4、【可选】其他：添加 NLog  和 iotdb 包（用于日志记录）
dotnet add package NLog
```

### Linux (Ubuntu)

#### 1. 安装环境

```bash
# 安装 .NET 8.0 SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

#### 2. 克隆项目并安装依赖

```powershell
# 【不推荐】使用最新的发版（不是最新的源码依赖）：dotnet add package Apache.IoTDB # 里面包含了 Thrift，故不需要添加， 

# 1、拉取源码，并编译生成依赖，生成的 dll 在 iotdb-client-csharp 的 src/Apache.IoTDB/bin/Release 下
git clone https://github.com/apache/iotdb-client-csharp.git
cd iotdb-client-csharp
dotnet build --configuration Release src/Apache.IoTDB/Apache.IoTDB.csproj

# 2、拉去已有测试项目
git clone https://github.com/LinJieXiao-XLJ/csharp-native-api-testcase.git
cd csharp-native-api-testcase
# 引入依赖：根据版本选择一个目录下的 dll 文件，将 dll 文件放在 C# 项目的 lib 目录中，并确保 csharp-native-api-testcase.csproj 文件中 Include 正确

# 3、添加 Thrift 包
dotnet add package Thrift.NetStd

# 4、【可选】其他：添加 NLog  和 iotdb 包（用于日志记录）
dotnet add package NLog
```

---

## 使用

### 配置修改

在运行测试前，请根据实际 IoTDB 环境修改配置文件 `conf/config.properties`：

### 基础自动化测试

#### 运行所有测试

```bash
dotnet run
```

#### 运行指定模型测试

```bash
# 运行 tree 模型测试
dotnet run -- tree

# 运行 table 模型测试
dotnet run -- table
```

#### 查看帮助

```bash
dotnet run -- help
```

> **说明：** `--` 是 dotnet CLI 的参数分隔符，用于区分 dotnet 参数和程序参数。`--` 后面的参数会传递给程序。

### 代码覆盖率测试

#### 生成 HTML 报告

```bash
# 安装 coverlet 和 reportgenerator
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

# 运行测试并生成覆盖率报告
coverlet bin/Debug/net8.0/csharp-native-api-testcase.dll --target "dotnet" --targetargs "test" --output coverage.xml
reportgenerator -reports:coverage.xml -targetdir:coverage-report
```

---

# 新增测试用例

## 基础用法

1. 测试文件命名规则：
2. 测试函数命名规则：


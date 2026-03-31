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
├── tests/
│   ├── table/                 # Table 测试用例
│   └── tree/                  # Tree 测试用例
├── src/
│   └── csharp-native-api-testcase/
│       ├── Program.cs         # 入口文件
│       └── csharp-native-api-testcase.csproj
├── csharp-native-api-testcase.sln  # 解决方案文件
└── README.md                  # 说明文档
```

---

## 环境

### 必需环境

- .NET 8.0 SDK
- Apache IoTDB 服务器

### 依赖库

- Apache IoTDB C# Client (待添加)



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

#### 2. 克隆项目

```powershell
git clone https://github.com/LinJieXiao-XLJ/csharp-native-api-testcase.git
cd csharp-native-api-testcase
```

#### 3. 安装依赖

```powershell
dotnet restore
```

### Linux (Ubuntu)

#### 1. 安装环境

```bash
# 安装 .NET 8.0 SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

#### 2. 克隆项目

```bash
git clone https://github.com/LinJieXiao-XLJ/csharp-native-api-testcase.git
cd csharp-native-api-testcase
```

#### 3. 安装依赖

```bash
dotnet restore
```

---

## 使用

### 配置修改

在运行测试前，请根据实际 IoTDB 环境修改配置文件 `conf/config.properties`：

### 基础自动化测试

#### 运行所有测试

```bash
dotnet run --project src/csharp-native-api-testcase -- --all
```

#### 运行单个测试

```bash
# 运行 table 模型测试
dotnet run --project src/csharp-native-api-testcase -- --table

# 运行 tree 模型测试
dotnet run --project src/csharp-native-api-testcase -- --tree
```

### 代码覆盖率测试

#### 生成 HTML 报告

```bash
# 安装 coverlet 和 reportgenerator
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

# 运行测试并生成覆盖率报告
coverlet src/csharp-native-api-testcase/bin/Debug/net8.0/csharp-native-api-testcase.dll --target "dotnet" --targetargs "test" --output coverage.xml
reportgenerator -reports:coverage.xml -targetdir:coverage-report
```

---

# 新增测试用例

## 基础用法

1. 测试文件命名规则：
2. 测试函数命名规则：


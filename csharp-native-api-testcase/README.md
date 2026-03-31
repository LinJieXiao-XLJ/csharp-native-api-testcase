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
└── README.md                   # 说明文档
```

---

## 环境

### 必需环境



### 依赖库



### 配置

可在 `conf/config.properties` 中修改配置信息

---

## 安装

### Windows

#### 1. 安装环境

```powershell

```

#### 2. 克隆项目

```powershell

```

#### 3. 安装依赖

```powershell

```

### Linux (Ubuntu)

#### 1. 安装环境

```bash

```

#### 2. 克隆项目

```bash

```

#### 3. 安装依赖

```bash

```

---

## 使用

### 配置修改

在运行测试前，请根据实际 IoTDB 环境修改配置文件 `conf/config.properties`：

### 基础自动化测试

#### 运行所有测试

```bash

```

#### 运行单个测试

```bash

```

### 代码覆盖率测试

#### 生成 HTML 报告

```bash

```

---

# 新增测试用例

## 基础用法

1. 测试文件命名规则：
2. 测试函数命名规则：


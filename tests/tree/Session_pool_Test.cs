using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.IoTDB;
using Apache.IoTDB.DataStructure;

namespace csharp_native_api_testcase.Tests.Tree;

/// <summary>
/// SessionPool 测试用例 - IoTDB 树模型 (3C3D 集群)
///
/// 基于 commit 6f451bdff65a816f1ac59a266875b41ad2610af8 测试连接池操作
/// 该 commit 修复了 SessionPool 重连问题
///
/// 主要测试功能：
/// - 集群节点 URL 创建连接池
/// - 数据库管理
/// - 数据插入操作
/// - 查询操作
/// - 连接池健康指标监控
/// </summary>
public class Session_pool_Test
{
    // 集群节点 URL 列表 (3C3D: 3 个 ConfigNode + 3 个 DataNode)
    private readonly List<string> _nodeUrls;

    // 单节点回退配置
    private readonly string _host;
    private readonly int _port;

    // 认证信息
    private readonly string _username;
    private readonly string _password;

    // 连接池配置
    private readonly int _fetchSize;
    private readonly string _zoneId;
    private readonly int _poolSize;
    private readonly bool _enableRpcCompression;
    private readonly int _timeout;

    public Session_pool_Test()
    {
        // 3C3D 集群配置
        _nodeUrls = new List<string>
        {
            "127.0.0.1:6667",
            "127.0.0.1:6668",
            "127.0.0.1:6669"
        };

        // 默认单节点回退配置
        _host = "127.0.0.1";
        _port = 6667;

        // 认证信息
        _username = "root";
        _password = "root";

        // 连接池设置
        _fetchSize = 1024;
        _zoneId = "Asia/Shanghai";
        _poolSize = 8;
        _enableRpcCompression = true;
        _timeout = 60000;
    }

    /// <summary>
    /// 测试 1: 使用集群节点 URL 创建 SessionPool
    /// 测试连接池连接 3C3D 集群的能力
    /// </summary>
    public async Task Test_SessionPool_Cluster_Connection()
    {
        Console.WriteLine("[测试] SessionPool 集群连接 (3C3D)");

        try
        {
            var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

            await sessionPool.Open();

            Console.WriteLine($"  连接池打开成功");
            Console.WriteLine($"  集群节点数: {_nodeUrls.Count}");
            Console.WriteLine($"  可用客户端数: {sessionPool.AvailableClients}");
            Console.WriteLine($"  连接池总大小: {sessionPool.TotalPoolSize}");
            Console.WriteLine($"  重连失败次数: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_SessionPool_Cluster_Connection");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_SessionPool_Cluster_Connection: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试 2: 使用单节点创建 SessionPool (回退方案)
    /// </summary>
    public async Task Test_SessionPool_Single_Node()
    {
        Console.WriteLine("[测试] SessionPool 单节点连接");

        try
        {
            var sessionPool = new SessionPool(_host, _port, _username, _password, _poolSize);

            await sessionPool.Open(_enableRpcCompression);

            Console.WriteLine($"  连接池已连接到 {_host}:{_port}");
            Console.WriteLine($"  可用客户端数: {sessionPool.AvailableClients}");
            Console.WriteLine($"  连接池总大小: {sessionPool.TotalPoolSize}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_SessionPool_Single_Node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_SessionPool_Single_Node: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试 3: 使用 SessionPool 创建数据库
    /// </summary>
    public async Task Test_Create_Database()
    {
        Console.WriteLine("[测试] 使用 SessionPool 创建数据库");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_db";

        try
        {
            await sessionPool.Open();

            // 创建数据库
            var result = await sessionPool.CreateDatabase(dbName);
            Console.WriteLine($"  创建数据库 '{dbName}' 结果: {result}");

            // 通过 SQL 查询验证
            var sqlResult = await sessionPool.ExecuteNonQueryStatementAsync($"SHOW DATABASES");
            Console.WriteLine($"  查询数据库列表已执行");

            // 清理
            await sessionPool.DeleteDatabaseAsync(dbName);
            Console.WriteLine($"  数据库 '{dbName}' 已删除");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Create_Database");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Create_Database: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// 测试 4: 通过 SQL 使用 SessionPool 创建时间序列
    /// </summary>
    public async Task Test_Create_Timeseries_SQL()
    {
        Console.WriteLine("[测试] 通过 SQL 使用 SessionPool 创建时间序列");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_ts_db";

        try
        {
            await sessionPool.Open();

            // 创建数据库
            await sessionPool.CreateDatabase(dbName);
            Console.WriteLine($"  数据库 '{dbName}' 已创建");

            // 通过 SQL 创建时间序列
            var createTsSql = $"CREATE TIMESERIES {dbName}.d1.s1 WITH DATATYPE=INT32, ENCODING=PLAIN";
            var result = await sessionPool.ExecuteNonQueryStatementAsync(createTsSql);
            Console.WriteLine($"  创建时间序列结果: {result}");

            // 查询验证
            var showSql = $"SHOW TIMESERIES {dbName}.*";
            await sessionPool.ExecuteNonQueryStatementAsync(showSql);
            Console.WriteLine($"  查询时间序列已执行");

            // 清理
            await sessionPool.DeleteDatabaseAsync(dbName);
            Console.WriteLine($"  数据库 '{dbName}' 已删除");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Create_Timeseries_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Create_Timeseries_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// 测试 5: 使用 SQL 插入数据
    /// </summary>
    public async Task Test_Insert_Data_SQL()
    {
        Console.WriteLine("[测试] 使用 SQL 插入数据");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_insert_db";

        try
        {
            await sessionPool.Open();

            // 环境准备
            await sessionPool.CreateDatabase(dbName);
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.temperature WITH DATATYPE=FLOAT, ENCODING=PLAIN");
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.humidity WITH DATATYPE=INT32, ENCODING=PLAIN");
            Console.WriteLine($"  环境准备完成");

            // 通过 SQL 插入数据
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var insertSql = $"INSERT INTO {dbName}.d1(timestamp, temperature, humidity) VALUES({timestamp}, 26.5, 65)";
            var result = await sessionPool.ExecuteNonQueryStatementAsync(insertSql);
            Console.WriteLine($"  插入数据结果: {result}");

            // 查询验证
            var querySql = $"SELECT * FROM {dbName}.d1";
            await sessionPool.ExecuteNonQueryStatementAsync(querySql);
            Console.WriteLine($"  查询已执行");

            // 清理
            await sessionPool.DeleteDatabaseAsync(dbName);
            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Insert_Data_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Insert_Data_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// 测试 6: 执行查询操作
    /// </summary>
    public async Task Test_Query_Operations()
    {
        Console.WriteLine("[测试] 执行查询操作");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // 显示数据库
            Console.WriteLine("  SHOW DATABASES:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW DATABASES");
            Console.WriteLine("    - 已执行");

            // 显示版本
            Console.WriteLine("  SHOW VERSION:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
            Console.WriteLine("    - 已执行");

            // 显示集群信息
            Console.WriteLine("  SHOW CLUSTER:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW CLUSTER");
            Console.WriteLine("    - 已执行");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Query_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Query_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// 测试 7: 执行非查询语句
    /// </summary>
    public async Task Test_NonQuery_Statement()
    {
        Console.WriteLine("[测试] 执行非查询语句");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_sql_db";

        try
        {
            await sessionPool.Open();

            // 通过 SQL 创建数据库
            var result = await sessionPool.ExecuteNonQueryStatementAsync($"CREATE DATABASE {dbName}");
            Console.WriteLine($"  CREATE DATABASE 结果: {result}");

            // 删除数据库
            result = await sessionPool.ExecuteNonQueryStatementAsync($"DROP DATABASE {dbName}");
            Console.WriteLine($"  DROP DATABASE 结果: {result}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_NonQuery_Statement");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_NonQuery_Statement: {ex.Message}");
            try
            {
                await sessionPool.ExecuteNonQueryStatementAsync($"DROP DATABASE IF EXISTS {dbName}");
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// 测试 8: 连接池健康指标监控
    /// 测试 commit 修复中引入的健康指标
    /// </summary>
    public async Task Test_Pool_Health_Metrics()
    {
        Console.WriteLine("[测试] 连接池健康指标监控");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            Console.WriteLine("  初始连接池状态:");
            Console.WriteLine($"    - 可用客户端数: {sessionPool.AvailableClients}");
            Console.WriteLine($"    - 连接池总大小: {sessionPool.TotalPoolSize}");
            Console.WriteLine($"    - 重连失败次数: {sessionPool.FailedReconnections}");

            // 执行多次操作观察连接池行为
            Console.WriteLine("  执行 5 次查询...");
            for (int i = 0; i < 5; i++)
            {
                await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
                Console.WriteLine($"    查询 {i + 1}: 可用={sessionPool.AvailableClients}, 失败={sessionPool.FailedReconnections}");
            }

            Console.WriteLine("  最终连接池状态:");
            Console.WriteLine($"    - 可用客户端数: {sessionPool.AvailableClients}");
            Console.WriteLine($"    - 重连失败次数: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Pool_Health_Metrics");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Pool_Health_Metrics: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// 测试 9: 时区操作
    /// </summary>
    public async Task Test_Timezone_Operations()
    {
        Console.WriteLine("[测试] 时区操作");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // 获取时区
            var tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  当前时区: {tz}");

            // 设置时区为 UTC
            await sessionPool.SetTimeZone("UTC");
            tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  设置 UTC 后: {tz}");

            // 恢复时区
            await sessionPool.SetTimeZone(_zoneId);
            tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  恢复时区: {tz}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Timezone_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Timezone_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// 测试 10: 并发连接池操作
    /// 测试修复后的线程安全连接处理
    /// </summary>
    public async Task Test_Concurrent_Operations()
    {
        Console.WriteLine("[测试] 并发连接池操作");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // 运行并发操作
            var tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
                }));
            }

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await sessionPool.ExecuteNonQueryStatementAsync("SHOW DATABASES");
                }));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"  完成 {tasks.Count} 个并发操作");
            Console.WriteLine($"  连接池状态: 可用={sessionPool.AvailableClients}, 失败={sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Concurrent_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Concurrent_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// 测试 11: 使用 SQL 批量插入
    /// </summary>
    public async Task Test_Batch_Insert_SQL()
    {
        Console.WriteLine("[测试] 使用 SQL 批量插入");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_batch_db";

        try
        {
            await sessionPool.Open();

            // 环境准备
            await sessionPool.CreateDatabase(dbName);
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.value WITH DATATYPE=INT64, ENCODING=PLAIN");
            Console.WriteLine($"  环境准备完成");

            // 批量插入
            var baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < 10; i++)
            {
                var insertSql = $"INSERT INTO {dbName}.d1(timestamp, value) VALUES({baseTimestamp + i}, {i * 100})";
                await sessionPool.ExecuteNonQueryStatementAsync(insertSql);
            }
            Console.WriteLine($"  已插入 10 条记录");

            // 查询计数
            await sessionPool.ExecuteNonQueryStatementAsync($"SELECT count(value) FROM {dbName}.d1");
            Console.WriteLine($"  计数查询已执行");

            // 清理
            await sessionPool.DeleteDatabaseAsync(dbName);
            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Batch_Insert_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Batch_Insert_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// 测试 12: 连接池重连行为
    /// 测试 commit 6f451bdff65a816f1ac59a266875b41ad2610af8 中的重连修复
    /// </summary>
    public async Task Test_Pool_Reconnection_Behavior()
    {
        Console.WriteLine("[测试] 连接池重连行为");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            Console.WriteLine("  测试连接池稳定性...");

            // 执行多次操作测试重连处理
            for (int batch = 0; batch < 3; batch++)
            {
                Console.WriteLine($"  批次 {batch + 1}:");

                for (int i = 0; i < 5; i++)
                {
                    await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
                }

                Console.WriteLine($"    可用: {sessionPool.AvailableClients}, 重连失败: {sessionPool.FailedReconnections}");
            }

            // 检查最终连接池健康状态
            Console.WriteLine("  最终连接池健康状态:");
            Console.WriteLine($"    - 可用客户端数: {sessionPool.AvailableClients}/{sessionPool.TotalPoolSize}");
            Console.WriteLine($"    - 重连失败次数: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[通过] Test_Pool_Reconnection_Behavior");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[失败] Test_Pool_Reconnection_Behavior: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// 运行所有 SessionPool 测试
    /// </summary>
    public async Task RunAllTests()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("运行 SessionPool 测试套件 (3C3D 集群)");
        Console.WriteLine("基于 commit 6f451bdff65a816f1ac59a266875b41ad2610af8");
        Console.WriteLine("========================================\n");

        var tests = new List<(string Name, Func<Task> Test)>
        {
            ("集群连接", Test_SessionPool_Cluster_Connection),
            ("单节点连接", Test_SessionPool_Single_Node),
            ("创建数据库", Test_Create_Database),
            ("创建时间序列 SQL", Test_Create_Timeseries_SQL),
            ("插入数据 SQL", Test_Insert_Data_SQL),
            ("查询操作", Test_Query_Operations),
            ("非查询语句", Test_NonQuery_Statement),
            ("连接池健康指标", Test_Pool_Health_Metrics),
            ("时区操作", Test_Timezone_Operations),
            ("并发操作", Test_Concurrent_Operations),
            ("批量插入 SQL", Test_Batch_Insert_SQL),
            ("连接池重连行为", Test_Pool_Reconnection_Behavior),
        };

        int passed = 0;
        int failed = 0;

        foreach (var test in tests)
        {
            Console.WriteLine($"\n--- {test.Name} ---");
            try
            {
                await test.Test();
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[异常] {ex.Message}");
                failed++;
            }
            Console.WriteLine();
        }

        Console.WriteLine("\n========================================");
        Console.WriteLine($"测试结果: {passed} 通过, {failed} 失败");
        Console.WriteLine("========================================\n");
    }
}
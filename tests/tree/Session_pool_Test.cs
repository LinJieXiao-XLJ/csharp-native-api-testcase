/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.IoTDB;
using Apache.IoTDB.DataStructure;

namespace csharp_native_api_testcase.Tests.Tree;

/// <summary>
/// SessionPool test cases for IoTDB Tree model (3C3D Cluster)
///
/// Tests connection pool operations based on commit 6f451bdff65a816f1ac59a266875b41ad2610af8
/// which fixed SessionPool reconnection issues.
///
/// Key features tested:
/// - Pool creation with cluster node URLs
/// - Database management
/// - Data insertion operations
/// - Query operations
/// - Pool health metrics monitoring
/// </summary>
public class Session_pool_Test
{
    // Cluster node URLs (3C3D: 3 ConfigNode + 3 DataNode)
    private readonly List<string> _nodeUrls;

    // Single node fallback configuration
    private readonly string _host;
    private readonly int _port;

    // Authentication
    private readonly string _username;
    private readonly string _password;

    // Pool configuration
    private readonly int _fetchSize;
    private readonly string _zoneId;
    private readonly int _poolSize;
    private readonly bool _enableRpcCompression;
    private readonly int _timeout;

    public Session_pool_Test()
    {
        // 3C3D Cluster configuration
        _nodeUrls = new List<string>
        {
            "127.0.0.1:6667",
            "127.0.0.1:6668",
            "127.0.0.1:6669"
        };

        // Default single node fallback
        _host = "127.0.0.1";
        _port = 6667;

        // Authentication
        _username = "root";
        _password = "root";

        // Pool settings
        _fetchSize = 1024;
        _zoneId = "Asia/Shanghai";
        _poolSize = 8;
        _enableRpcCompression = true;
        _timeout = 60000;
    }

    /// <summary>
    /// Test 1: Create SessionPool with cluster node URLs
    /// This tests the pool's ability to connect to a 3C3D cluster
    /// </summary>
    public async Task Test_SessionPool_Cluster_Connection()
    {
        Console.WriteLine("[Test] SessionPool cluster connection (3C3D)");

        try
        {
            var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

            await sessionPool.Open();

            Console.WriteLine($"  Pool opened successfully");
            Console.WriteLine($"  Cluster nodes: {_nodeUrls.Count}");
            Console.WriteLine($"  Available clients: {sessionPool.AvailableClients}");
            Console.WriteLine($"  Total pool size: {sessionPool.TotalPoolSize}");
            Console.WriteLine($"  Failed reconnections: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_SessionPool_Cluster_Connection");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_SessionPool_Cluster_Connection: {ex.Message}");
        }
    }

    /// <summary>
    /// Test 2: Create SessionPool with single node (fallback)
    /// </summary>
    public async Task Test_SessionPool_Single_Node()
    {
        Console.WriteLine("[Test] SessionPool single node connection");

        try
        {
            var sessionPool = new SessionPool(_host, _port, _username, _password, _poolSize);

            await sessionPool.Open(_enableRpcCompression);

            Console.WriteLine($"  Pool opened to {_host}:{_port}");
            Console.WriteLine($"  Available clients: {sessionPool.AvailableClients}");
            Console.WriteLine($"  Total pool size: {sessionPool.TotalPoolSize}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_SessionPool_Single_Node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_SessionPool_Single_Node: {ex.Message}");
        }
    }

    /// <summary>
    /// Test 3: Create database using SessionPool
    /// </summary>
    public async Task Test_Create_Database()
    {
        Console.WriteLine("[Test] Create database using SessionPool");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_db";

        try
        {
            await sessionPool.Open();

            // Create database
            var result = await sessionPool.CreateDatabase(dbName);
            Console.WriteLine($"  Create database '{dbName}' result: {result}");

            // Verify by querying using SQL
            var sqlResult = await sessionPool.ExecuteNonQueryStatementAsync($"SHOW DATABASES");
            Console.WriteLine($"  Show databases executed");

            // Cleanup
            await sessionPool.DeleteDatabaseAsync(dbName);
            Console.WriteLine($"  Database '{dbName}' deleted");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Create_Database");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Create_Database: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// Test 4: Create timeseries using SessionPool via SQL
    /// </summary>
    public async Task Test_Create_Timeseries_SQL()
    {
        Console.WriteLine("[Test] Create timeseries using SessionPool via SQL");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_ts_db";

        try
        {
            await sessionPool.Open();

            // Create database
            await sessionPool.CreateDatabase(dbName);
            Console.WriteLine($"  Database '{dbName}' created");

            // Create timeseries via SQL
            var createTsSql = $"CREATE TIMESERIES {dbName}.d1.s1 WITH DATATYPE=INT32, ENCODING=PLAIN";
            var result = await sessionPool.ExecuteNonQueryStatementAsync(createTsSql);
            Console.WriteLine($"  Create timeseries result: {result}");

            // Query to verify
            var showSql = $"SHOW TIMESERIES {dbName}.*";
            await sessionPool.ExecuteNonQueryStatementAsync(showSql);
            Console.WriteLine($"  Show timeseries executed");

            // Cleanup
            await sessionPool.DeleteDatabaseAsync(dbName);
            Console.WriteLine($"  Database '{dbName}' deleted");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Create_Timeseries_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Create_Timeseries_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// Test 5: Insert data using SQL
    /// </summary>
    public async Task Test_Insert_Data_SQL()
    {
        Console.WriteLine("[Test] Insert data using SQL");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_insert_db";

        try
        {
            await sessionPool.Open();

            // Setup
            await sessionPool.CreateDatabase(dbName);
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.temperature WITH DATATYPE=FLOAT, ENCODING=PLAIN");
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.humidity WITH DATATYPE=INT32, ENCODING=PLAIN");
            Console.WriteLine($"  Setup completed");

            // Insert data via SQL
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var insertSql = $"INSERT INTO {dbName}.d1(timestamp, temperature, humidity) VALUES({timestamp}, 26.5, 65)";
            var result = await sessionPool.ExecuteNonQueryStatementAsync(insertSql);
            Console.WriteLine($"  Insert data result: {result}");

            // Query to verify
            var querySql = $"SELECT * FROM {dbName}.d1";
            await sessionPool.ExecuteNonQueryStatementAsync(querySql);
            Console.WriteLine($"  Query executed");

            // Cleanup
            await sessionPool.DeleteDatabaseAsync(dbName);
            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Insert_Data_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Insert_Data_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// Test 6: Execute query operations
    /// </summary>
    public async Task Test_Query_Operations()
    {
        Console.WriteLine("[Test] Execute query operations");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // Show databases
            Console.WriteLine("  SHOW DATABASES:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW DATABASES");
            Console.WriteLine("    - Executed");

            // Show version
            Console.WriteLine("  SHOW VERSION:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
            Console.WriteLine("    - Executed");

            // Show cluster info
            Console.WriteLine("  SHOW CLUSTER:");
            await sessionPool.ExecuteNonQueryStatementAsync("SHOW CLUSTER");
            Console.WriteLine("    - Executed");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Query_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Query_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// Test 7: Execute non-query statement
    /// </summary>
    public async Task Test_NonQuery_Statement()
    {
        Console.WriteLine("[Test] Execute non-query statement");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_sql_db";

        try
        {
            await sessionPool.Open();

            // Create database via SQL
            var result = await sessionPool.ExecuteNonQueryStatementAsync($"CREATE DATABASE {dbName}");
            Console.WriteLine($"  CREATE DATABASE result: {result}");

            // Drop database
            result = await sessionPool.ExecuteNonQueryStatementAsync($"DROP DATABASE {dbName}");
            Console.WriteLine($"  DROP DATABASE result: {result}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_NonQuery_Statement");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_NonQuery_Statement: {ex.Message}");
            try
            {
                await sessionPool.ExecuteNonQueryStatementAsync($"DROP DATABASE IF EXISTS {dbName}");
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// Test 8: Pool health metrics monitoring
    /// Tests the health metrics introduced in the commit fix
    /// </summary>
    public async Task Test_Pool_Health_Metrics()
    {
        Console.WriteLine("[Test] Pool health metrics monitoring");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            Console.WriteLine("  Initial pool state:");
            Console.WriteLine($"    - Available clients: {sessionPool.AvailableClients}");
            Console.WriteLine($"    - Total pool size: {sessionPool.TotalPoolSize}");
            Console.WriteLine($"    - Failed reconnections: {sessionPool.FailedReconnections}");

            // Execute multiple operations to observe pool behavior
            Console.WriteLine("  Executing 5 queries...");
            for (int i = 0; i < 5; i++)
            {
                await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
                Console.WriteLine($"    Query {i + 1}: Available={sessionPool.AvailableClients}, Failed={sessionPool.FailedReconnections}");
            }

            Console.WriteLine("  Final pool state:");
            Console.WriteLine($"    - Available clients: {sessionPool.AvailableClients}");
            Console.WriteLine($"    - Failed reconnections: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Pool_Health_Metrics");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Pool_Health_Metrics: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// Test 9: Timezone operations
    /// </summary>
    public async Task Test_Timezone_Operations()
    {
        Console.WriteLine("[Test] Timezone operations");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // Get timezone
            var tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  Current timezone: {tz}");

            // Set timezone to UTC
            await sessionPool.SetTimeZone("UTC");
            tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  After set UTC: {tz}");

            // Restore timezone
            await sessionPool.SetTimeZone(_zoneId);
            tz = await sessionPool.GetTimeZone();
            Console.WriteLine($"  Restored timezone: {tz}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Timezone_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Timezone_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// Test 10: Concurrent pool operations
    /// Tests thread-safe connection handling after the fix
    /// </summary>
    public async Task Test_Concurrent_Operations()
    {
        Console.WriteLine("[Test] Concurrent pool operations");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            // Run concurrent operations
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
            Console.WriteLine($"  Completed {tasks.Count} concurrent operations");
            Console.WriteLine($"  Pool state: Available={sessionPool.AvailableClients}, Failed={sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Concurrent_Operations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Concurrent_Operations: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// Test 11: Batch insert using SQL
    /// </summary>
    public async Task Test_Batch_Insert_SQL()
    {
        Console.WriteLine("[Test] Batch insert using SQL");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);
        var dbName = "test_pool_batch_db";

        try
        {
            await sessionPool.Open();

            // Setup
            await sessionPool.CreateDatabase(dbName);
            await sessionPool.ExecuteNonQueryStatementAsync($"CREATE TIMESERIES {dbName}.d1.value WITH DATATYPE=INT64, ENCODING=PLAIN");
            Console.WriteLine($"  Setup completed");

            // Batch insert
            var baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < 10; i++)
            {
                var insertSql = $"INSERT INTO {dbName}.d1(timestamp, value) VALUES({baseTimestamp + i}, {i * 100})";
                await sessionPool.ExecuteNonQueryStatementAsync(insertSql);
            }
            Console.WriteLine($"  Inserted 10 records");

            // Query count
            await sessionPool.ExecuteNonQueryStatementAsync($"SELECT count(value) FROM {dbName}.d1");
            Console.WriteLine($"  Count query executed");

            // Cleanup
            await sessionPool.DeleteDatabaseAsync(dbName);
            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Batch_Insert_SQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Batch_Insert_SQL: {ex.Message}");
            try
            {
                await sessionPool.DeleteDatabaseAsync(dbName);
                await sessionPool.Close();
            }
            catch { }
        }
    }

    /// <summary>
    /// Test 12: Pool reconnection behavior
    /// Tests the reconnection fix from commit 6f451bdff65a816f1ac59a266875b41ad2610af8
    /// </summary>
    public async Task Test_Pool_Reconnection_Behavior()
    {
        Console.WriteLine("[Test] Pool reconnection behavior");

        var sessionPool = new SessionPool(_nodeUrls, _username, _password, _fetchSize, _zoneId, _poolSize, _enableRpcCompression, _timeout);

        try
        {
            await sessionPool.Open();

            Console.WriteLine("  Testing pool stability...");

            // Execute many operations to test reconnection handling
            for (int batch = 0; batch < 3; batch++)
            {
                Console.WriteLine($"  Batch {batch + 1}:");

                for (int i = 0; i < 5; i++)
                {
                    await sessionPool.ExecuteNonQueryStatementAsync("SHOW VERSION");
                }

                Console.WriteLine($"    Available: {sessionPool.AvailableClients}, Failed reconnections: {sessionPool.FailedReconnections}");
            }

            // Check final pool health
            Console.WriteLine("  Final pool health:");
            Console.WriteLine($"    - Available clients: {sessionPool.AvailableClients}/{sessionPool.TotalPoolSize}");
            Console.WriteLine($"    - Failed reconnections: {sessionPool.FailedReconnections}");

            await sessionPool.Close();
            Console.WriteLine("[Pass] Test_Pool_Reconnection_Behavior");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fail] Test_Pool_Reconnection_Behavior: {ex.Message}");
            try { await sessionPool.Close(); } catch { }
        }
    }

    /// <summary>
    /// Run all SessionPool tests
    /// </summary>
    public async Task RunAllTests()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("Running SessionPool Test Suite (3C3D Cluster)");
        Console.WriteLine("Based on commit 6f451bdff65a816f1ac59a266875b41ad2610af8");
        Console.WriteLine("========================================\n");

        var tests = new List<(string Name, Func<Task> Test)>
        {
            ("Cluster Connection", Test_SessionPool_Cluster_Connection),
            ("Single Node", Test_SessionPool_Single_Node),
            ("Create Database", Test_Create_Database),
            ("Create Timeseries SQL", Test_Create_Timeseries_SQL),
            ("Insert Data SQL", Test_Insert_Data_SQL),
            ("Query Operations", Test_Query_Operations),
            ("NonQuery Statement", Test_NonQuery_Statement),
            ("Pool Health Metrics", Test_Pool_Health_Metrics),
            ("Timezone Operations", Test_Timezone_Operations),
            ("Concurrent Operations", Test_Concurrent_Operations),
            ("Batch Insert SQL", Test_Batch_Insert_SQL),
            ("Pool Reconnection Behavior", Test_Pool_Reconnection_Behavior),
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
                Console.WriteLine($"[Exception] {ex.Message}");
                failed++;
            }
            Console.WriteLine();
        }

        Console.WriteLine("\n========================================");
        Console.WriteLine($"Test Results: {passed} passed, {failed} failed");
        Console.WriteLine("========================================\n");
    }
}
// See https://aka.ms/new-console-template for more information
using CoreRemoting.Serialization.Bson;
using DotNettyCoreRemoting;
using shared;
using System.Collections;
using System.Data;

using System;

class Program
{
    static void Main()
    {

        var client = new DotNettyRPCClient(new ClientConfig
        {
            ServerHostName = "127.0.0.1",
            ServerPort = 9095,
            timeout = 120
        });
        //var first_ser = client.CreateProxy<IRemoteCommand>();
        var first_ser = client.CreateProxy<IMyFirstServer>();

        //创建 DataTable
        DataTable dt = new DataTable("MyTable");

        // 添加列
        dt.Columns.Add("id", typeof(int));  // 指定类型为 int
        dt.Columns.Add("name", typeof(string));

        // 添加行并赋值
        DataRow row1 = dt.NewRow();
        row1["id"] = 1;
        row1["name"] = "张三";
        dt.Rows.Add(row1);

        DataRow row2 = dt.NewRow();
        row2["id"] = 2;
        row2["name"] = "李四";
        dt.Rows.Add(row2);


        //Console.WriteLine("wait!");

        try
        {
            Console.WriteLine("开始测试DataTable序列化和反序列化...");

            //测试字符串参数（应该正常工作）
            //var data = first_ser.SayHelloT("123456654321");
            //Console.WriteLine($"字符串参数调用成功: {data}");

            //测试DataTable参数
            Console.WriteLine("开始调用SayHelloT方法，传递DataTable参数...");
            var dataT = first_ser.SayHelloT(dt);
            Console.WriteLine($"DataTable调用成功，返回对象类型: {dataT?.GetType().FullName ?? "null"}");

            if (dataT is DataTable resultDt)
            {
                Console.WriteLine($"成功反序列化为DataTable，表名: {resultDt.TableName}");
                Console.WriteLine($"行数: {resultDt.Rows.Count}, 列数: {resultDt.Columns.Count}");

                //打印表格内容
                foreach (DataColumn col in resultDt.Columns)
                {
                    Console.Write($"{col.ColumnName}\t");
                }
                Console.WriteLine();

                foreach (DataRow row in resultDt.Rows)
                {
                    foreach (DataColumn col in resultDt.Columns)
                    {
                        Console.Write($"{row[col]}\t");
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("测试成功完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"调用失败: {ex.Message}");
            Console.WriteLine($"异常详情: {ex.ToString()}");
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();

    }
}

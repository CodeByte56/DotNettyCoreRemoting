// See https://aka.ms/new-console-template for more information
using DotNettyCoreRemoting;
using shared;
using System.Collections;
using System.Data;


//var client = new DotNettyRPCClient(new ClientConfig
//{
//    ServerHostName = "10.251.0.200",
//    ServerPort = 31000
//});

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

string input;
do
{
    Console.WriteLine("输入 'j' 执行 SayHelloT，输入 'q' 退出：");
    input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input))
    {
        Console.WriteLine("输入不能为空，请重新输入。");
        continue;
    }

    if (input.Equals("j", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            //返回表格
            var dataT = first_ser.SayHelloT(dt);

            //返回字符串
            var data = first_ser.SayHelloT("123456654321");

            Console.WriteLine($"返回结果: {data}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"调用失败: {ex.Message}");
        }
    }
    else if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("正在退出...");
    }
    else
    {
        Console.WriteLine("无效输入，请输入 'j' 执行操作，或 'q' 退出。");
    }

} while (!input.Equals("q", StringComparison.OrdinalIgnoreCase));

Console.WriteLine("程序已退出。");
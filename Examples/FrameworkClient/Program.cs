using DotNettyCoreRemoting;
using shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkClient
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var client = new DotNettyRPCClient(new ClientConfig
            {
                ServerHostName = "127.0.0.1",
                ServerPort = 9095
            });

            var first_ser = client.CreateProxy<IMyFirstServer>();


            DataTable dataTable = new DataTable("TestTable");
            dataTable.Columns.Add("id", typeof(Guid));
            dataTable.Columns.Add("name", typeof(string));

            DataRow row = dataTable.NewRow();
            row["id"] = Guid.NewGuid();
            row["name"] = "张三";
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row["id"] = Guid.NewGuid();
            row["name"] = "李四";
            dataTable.Rows.Add(row);

            DataSet dataSet = new DataSet("TestDataSet");
            dataSet.Tables.Add(dataTable);

            DataTable dataTable1 = new DataTable("TestTable1");
            dataTable1.Columns.Add("id", typeof(Guid));
            dataTable1.Columns.Add("name", typeof(string));
            DataRow row1 = dataTable1.NewRow();
            row1["id"] = Guid.NewGuid();
            row1["name"] = "王五";
            dataTable1.Rows.Add(row1);
            row1 = dataTable1.NewRow();
            row1 = dataTable1.NewRow();
            row1["id"] = Guid.NewGuid();
            row1["name"] = "赵六";
            dataTable1.Rows.Add(row1);
            dataSet.Tables.Add(dataTable1);


            first_ser.SayHelloT(dataSet);

            Console.WriteLine("Hello, World!");

            Console.ReadLine();
        }
    }
}

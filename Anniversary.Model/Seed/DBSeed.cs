using Anniversary.Common.DB;
using Anniversary.Common.Helper;
using Anniversary.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anniversary.Model.Seed
{
    public class DBSeed
    {
        public static async Task SeedAsync(MyContext myContext)
        {
            try
            {
                Console.WriteLine("Config data init...");
                Console.WriteLine($"Is multi-DataBase: {Appsettings.app(new string[] { "MutiDBEnabled" })}");
                if (Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
                {
                    Console.WriteLine($"Master DB Type: {MyContext.DbType}");
                    Console.WriteLine($"Master DB ConnectString: {MyContext.ConnectionString}");
                    Console.WriteLine();

                    var slaveIndex = 0;
                    BaseDBConfig.MutiConnectionString.Where(x => x.ConnId != MainDb.CurrentDbConnId).ToList().ForEach(m =>
                    {
                        slaveIndex++;
                        Console.WriteLine($"Slave{slaveIndex} DB ID: {m.ConnId}");
                        Console.WriteLine($"Slave{slaveIndex} DB Type: {m.DbType}");
                        Console.WriteLine($"Slave{slaveIndex} DB ConnectString: {m.Conn}");
                    });

                }
                else
                {
                    Console.WriteLine("DB Type: " + MyContext.DbType);
                    Console.WriteLine("DB ConnectString: " + MyContext.ConnectionString);
                }

                Console.WriteLine("Create Database...");
                // 创建数据库
                myContext.Db.DbMaintenance.CreateDatabase();

                Console.WriteLine("Create Tables...");
                // 创建表
                myContext.CreateTableByEntity(false,
                    typeof(User),
                    typeof(Info),
                    typeof(InfoDetail));

                // 后期单独处理某些表
                 //myContext.Db.CodeFirst.InitTables(typeof(InfoDetail));

                Console.WriteLine("Database is  created success!");
                Console.WriteLine();
                if (Appsettings.app(new string[] { "AppSettings", "SeedDBDataEnabled" }).ObjToBool())
                {
                    Console.WriteLine("Seeding database...");

                    //#region Info
                    //if (!await myContext.Db.Queryable<Info>().AnyAsync())
                    //{
                    //    myContext.GetEntityDB<Info>().Insert(new Info() { InfoId = 0, Name = "initial data", OpenId = "initial data" });
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Table:Info already exists...");
                    //}
                    //#endregion

                    //#region InfoDetail
                    //if (!await myContext.Db.Queryable<InfoDetail>().AnyAsync())
                    //{
                    //    List<InfoDetail> infos = new List<InfoDetail>();

                    //    for (int i = 0; i < 4; i++)
                    //    {
                    //        infos.Add(new InfoDetail() { InfoId = 0, Date = DateTime.Now.Date.AddDays(i * 100), Days = i * 100 });
                    //    }

                    //    myContext.GetEntityDB<InfoDetail>().InsertRange(infos);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Table:InfoDetail already exists...");
                    //}
                    //#endregion

                    Console.WriteLine("Done seeding database.");
                }

                Console.WriteLine();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
    }
}
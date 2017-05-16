using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 自动创建数据库
{
    class Program
    {
        static void Main(string[] args)
        {
            string exit = string.Empty;
            SqlConnection conn = new SqlConnection("Server=localhost;Integrated Security=SSPI;database=master");
            conn.Open();
            while (exit != "n")
            {
                Console.WriteLine("Please input your database name: \n");
                string dbname = Console.ReadLine().Trim();
                string str = string.Format("CREATE DATABASE {0} ON PRIMARY" +
                    "(NAME = {0}_Data, FILENAME = 'C:\\{0}Data.mdf', " +
                    "SIZE = 3MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                    "LOG ON (NAME = {0}_Log, FILENAME='C:\\{0}Log.ldf', " +
                    "SIZE = 1MB, MAXSIZE = 5MB, FILEGROWTH = 10%)", dbname);
                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("Do you want to continue\n? [y/n]");
                exit = Console.ReadLine();
                exit = exit.Trim().ToLower();
            }
            conn.Close();

        }
    }
}

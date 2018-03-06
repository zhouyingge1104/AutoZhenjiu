using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using AutoZhenJiu.entity;

namespace AutoZhenJiu.database
{
    class DBsqlite
    {
        SQLiteConnection conn;

        public DBsqlite(string dbFile)
        {
            this.conn = new SQLiteConnection();
            SQLiteConnectionStringBuilder connStr = new SQLiteConnectionStringBuilder();
            connStr.DataSource = dbFile;
            conn.ConnectionString = connStr.ToString();
            conn.Open();
            Console.WriteLine("Open OK");
        }

        /// <summary>
        /// 查询全部客户
        /// </summary>
        public  List<Client> getClients()
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "select * from t_client";
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            List<Client> objs = new List<Client>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Client obj = new Client();
                    obj.id = reader.GetInt32(0);
                    obj.name = reader.GetString(1);
                    obj.height = reader.GetString(2);
                    obj.weight = reader.GetString(3);
                    obj.idCardNo = reader.GetString(4);
                    obj.addTime = reader.GetString(5);
                    obj.gender = reader.GetString(6);
                    obj.param = reader.GetString(7);
                    obj.age = reader.GetInt32(8);

                    objs.Add(obj);
                }
               
            }

             return objs;

        }

        /// <summary>
        /// 根据身份证号查询客户
        /// </summary>
        public Client getClientByIdCardNo(string idCardNo)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "select * from t_client where idCardNo = '" + idCardNo + "'";
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            List<Client> objs = new List<Client>();
            if (reader.HasRows)
            {
                reader.Read();

                Client obj = new Client();
                obj.id = reader.GetInt32(0);
                obj.name = reader.GetString(1);
                obj.height = reader.GetString(2);
                obj.weight = reader.GetString(3);
                obj.idCardNo = reader.GetString(4);
                obj.addTime = reader.GetString(5);
                obj.gender = reader.GetString(6);
                obj.param = reader.GetString(7);
                obj.age = reader.GetInt32(8);

                return obj;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 根据id查找客户
        /// </summary>
        public Client getClient(string id)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "select * from t_client where id = " + id;
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            List<Client> objs = new List<Client>();
            if (reader.HasRows)
            {
                reader.Read();

                Client obj = new Client();
                obj.id = reader.GetInt32(0);
                obj.name = reader.GetString(1);
                obj.height = reader.GetString(2);
                obj.weight = reader.GetString(3);
                obj.idCardNo = reader.GetString(4);
                obj.addTime = reader.GetString(5);
                obj.gender = reader.GetString(6);
                obj.param = reader.GetString(7);
                obj.age = reader.GetInt32(8);

                return obj;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 保存一条客户记录
        /// </summary>
        public bool saveClient(Client client)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "insert into t_client (name, height, weight, idCardNo, addTime, gender, param, age) values ('" + client.name + "', '" + client.height + "', '" + client.weight + "', '" + client.idCardNo + "','" + client.addTime +"','" + client.gender + "','" + client.param + "'," + client.age + ")";
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            int effictedNum = cmd.ExecuteNonQuery();
            if (effictedNum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 更新一条客户记录
        /// </summary>
        public bool updateClient(Client client)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "update t_client set name = '" + client.name +  "', height = '" + client.height + "', weight = '" + client.weight + "', idCardNo = '" + client.idCardNo + "', gender = '" + client.gender + "', param = '" + client.param + "', age = " + client.age + " where id = " + client.id;
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            int effictedNum = cmd.ExecuteNonQuery();
            if (effictedNum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 根据id查找病历记录
        /// </summary>
        public MR getMR(string id)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "select * from t_mr where id = " + id;
            Console.WriteLine(sql);
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
           
            if (reader.Read())
            {
                MR obj = new MR();
                obj.id = reader.GetInt32(0);
                obj.clientId = reader.GetInt32(1);
                obj.mr = reader.GetString(2);
                obj.addTime = reader.GetString(3);
                obj.date1 = reader.GetString(4);
                obj.doctor = reader.GetString(5);

                return obj;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 根据客户ID查询病历列表
        /// </summary>
        public List<MR> getMRsByClientId(int clientId)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "select * from t_mr where clientId = " + clientId + " order by date1 desc";
            Console.WriteLine(sql);
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            List<MR> objs = new List<MR>();
            while(reader.Read())
            {
                MR obj = new MR();
                obj.id = reader.GetInt32(0);
                obj.clientId = reader.GetInt32(1);
                obj.mr = reader.GetString(2);
                obj.addTime = reader.GetString(3);
                obj.date1 = reader.GetString(4);
                obj.doctor = reader.GetString(5);

                objs.Add(obj); 
            }

            return objs;

        }

        /// <summary>
        /// 保存一次病历记录
        /// </summary>
        public bool saveMR(MR mr)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "insert into t_mr (clientId, mr, addTime, date1, doctor) values (" + mr.clientId + ", '" + mr.mr + "', '" + mr.addTime + "', '" + mr.date1 + "','" + mr.doctor + "')";
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            int effictedNum = cmd.ExecuteNonQuery();

            if (effictedNum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 更新一条病历记录
        /// </summary>
        public bool updateMR(MR mr)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "update t_mr set mr = '" + mr.mr + "', date1 = '" + mr.date1 + "', doctor = '" + mr.doctor + "' where id = " + mr.id;
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            int effictedNum = cmd.ExecuteNonQuery();
            if (effictedNum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 删除一条病历记录
        /// </summary>
        public bool deleteMR(MR mr)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            string sql = "delete from t_mr where id = " + mr.id;
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            int effictedNum = cmd.ExecuteNonQuery();
            if (effictedNum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 通用的执行SQL方法
        /// </summary>
        public void execute(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.conn;
            Console.WriteLine("sql:" + sql);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            Console.WriteLine("sql执行成功");
        }

    }
}

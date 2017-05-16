using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    class DALLogMainTable
    {
        public DALLogMainTable()
        { }
        #region  Method
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(string LogID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from LogMainTable");
            strSql.Append(" where LogID=@LogID ");
            SqlParameter[] parameters = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,50)           };
            parameters[0].Value = LogID;

            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }


        /// <summary>
        /// 增加一条数据,及其子表数据
        /// </summary>
        public int Add(LogMainTable model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into LogMainTable(");
            strSql.Append("LogID,AxisSKC4,AxisSKC5,AxisSKC6,AxisSKC7,AxisSKC8,ZZ,CS,XZPara,XZZZ,ZS,AxisSKC1,AxisSKC2,AxisSKC3)");
            strSql.Append(" values (");
            strSql.Append("@LogID,@AxisSKC4,@AxisSKC5,@AxisSKC6,@AxisSKC7,@AxisSKC8,@ZZ,@CS,@XZPara,@XZZZ,@ZS,@AxisSKC1,@AxisSKC2,@AxisSKC3)");
            SqlParameter[] parameters = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,50),
                    new SqlParameter("@AxisSKC4", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC5", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC6", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC7", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC8", SqlDbType.Int,4),
                    new SqlParameter("@ZZ", SqlDbType.Int,4),
                    new SqlParameter("@CS", SqlDbType.Int,4),
                    new SqlParameter("@XZPara", SqlDbType.Decimal,9),
                    new SqlParameter("@XZZZ", SqlDbType.Int,4),
                    new SqlParameter("@ZS", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC1", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC2", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC3", SqlDbType.Int,4)};
            parameters[0].Value = model.LogID;
            parameters[1].Value = model.AxisSKC4;
            parameters[2].Value = model.AxisSKC5;
            parameters[3].Value = model.AxisSKC6;
            parameters[4].Value = model.AxisSKC7;
            parameters[5].Value = model.AxisSKC8;
            parameters[6].Value = model.ZZ;
            parameters[7].Value = model.CS;
            parameters[8].Value = model.XZPara;
            parameters[9].Value = model.XZZZ;
            parameters[10].Value = model.ZS;
            parameters[11].Value = model.AxisSKC1;
            parameters[12].Value = model.AxisSKC2;
            parameters[13].Value = model.AxisSKC3;

            List<CommandInfo> sqllist = new List<CommandInfo>();
            CommandInfo cmd = new CommandInfo(strSql.ToString(), parameters);
            sqllist.Add(cmd);
            StringBuilder strSql2;
            foreach (LogDetailTable models in model.LogDetailTables)
            {
                strSql2 = new StringBuilder();
                strSql2.Append("insert into LogDetailTable(");
                strSql2.Append("LogID,CBNo,JLZ,SK,CBZZ,UsedSKC)");
                strSql2.Append(" values (");
                strSql2.Append("@LogID,@CBNo,@JLZ,@SK,@CBZZ,@UsedSKC)");
                SqlParameter[] parameters2 = {
                        new SqlParameter("@LogID", SqlDbType.NVarChar,50),
                        new SqlParameter("@CBNo", SqlDbType.Int,4),
                        new SqlParameter("@JLZ", SqlDbType.Int,4),
                        new SqlParameter("@SK", SqlDbType.Int,4),
                        new SqlParameter("@CBZZ", SqlDbType.Int,4),
                        new SqlParameter("@UsedSKC", SqlDbType.Int,4)};
                parameters2[0].Value = models.LogID;
                parameters2[1].Value = models.CBNo;
                parameters2[2].Value = models.JLZ;
                parameters2[3].Value = models.SK;
                parameters2[4].Value = models.CBZZ;
                parameters2[5].Value = models.UsedSKC;

                cmd = new CommandInfo(strSql2.ToString(), parameters2);
                sqllist.Add(cmd);
            }
            return DbHelperSQL.ExecuteSqlTran(sqllist);
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(LogMainTable model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update LogMainTable set ");
            strSql.Append("AxisSKC4=@AxisSKC4,");
            strSql.Append("AxisSKC5=@AxisSKC5,");
            strSql.Append("AxisSKC6=@AxisSKC6,");
            strSql.Append("AxisSKC7=@AxisSKC7,");
            strSql.Append("AxisSKC8=@AxisSKC8,");
            strSql.Append("ZZ=@ZZ,");
            strSql.Append("CS=@CS,");
            strSql.Append("XZPara=@XZPara,");
            strSql.Append("XZZZ=@XZZZ,");
            strSql.Append("ZS=@ZS,");
            strSql.Append("AxisSKC1=@AxisSKC1,");
            strSql.Append("AxisSKC2=@AxisSKC2,");
            strSql.Append("AxisSKC3=@AxisSKC3");
            strSql.Append(" where LogID=@LogID ");
            SqlParameter[] parameters = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,50),
                    new SqlParameter("@AxisSKC4", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC5", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC6", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC7", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC8", SqlDbType.Int,4),
                    new SqlParameter("@ZZ", SqlDbType.Int,4),
                    new SqlParameter("@CS", SqlDbType.Int,4),
                    new SqlParameter("@XZPara", SqlDbType.Decimal,9),
                    new SqlParameter("@XZZZ", SqlDbType.Int,4),
                    new SqlParameter("@ZS", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC1", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC2", SqlDbType.Int,4),
                    new SqlParameter("@AxisSKC3", SqlDbType.Int,4)};
            parameters[0].Value = model.LogID;
            parameters[1].Value = model.AxisSKC4;
            parameters[2].Value = model.AxisSKC5;
            parameters[3].Value = model.AxisSKC6;
            parameters[4].Value = model.AxisSKC7;
            parameters[5].Value = model.AxisSKC8;
            parameters[6].Value = model.ZZ;
            parameters[7].Value = model.CS;
            parameters[8].Value = model.XZPara;
            parameters[9].Value = model.XZZZ;
            parameters[10].Value = model.ZS;
            parameters[11].Value = model.AxisSKC1;
            parameters[12].Value = model.AxisSKC2;
            parameters[13].Value = model.AxisSKC3;

            int rowsAffected = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除一条数据，及子表所有相关数据
        /// </summary>
        public bool Delete(string LogID)
        {
            List<CommandInfo> sqllist = new List<CommandInfo>();
            StringBuilder strSql2 = new StringBuilder();
            strSql2.Append("delete LogDetailTable ");
            strSql2.Append(" where LogID=@LogID ");
            SqlParameter[] parameters2 = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,-1)};
            parameters2[0].Value = LogID;

            CommandInfo cmd = new CommandInfo(strSql2.ToString(), parameters2);
            sqllist.Add(cmd);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete LogMainTable ");
            strSql.Append(" where LogID=@LogID ");
            SqlParameter[] parameters = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,50)           };
            parameters[0].Value = LogID;

            cmd = new CommandInfo(strSql.ToString(), parameters);
            sqllist.Add(cmd);
            int rowsAffected = DbHelperSQL.ExecuteSqlTran(sqllist);
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool DeleteList(string LogIDlist)
        {
            List<string> sqllist = new List<string>();
            StringBuilder strSql2 = new StringBuilder();
            strSql2.Append("delete from LogDetailTable ");
            strSql2.Append(" where LogID in (" + LogIDlist + ")  ");
            sqllist.Add(strSql2.ToString());
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from LogMainTable ");
            strSql.Append(" where LogID in (" + LogIDlist + ")  ");
            sqllist.Add(strSql.ToString());
            int rowsAffected = DbHelperSQL.ExecuteSqlTran(sqllist);
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public LogMainTable GetModel(string LogID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select LogID,AxisSKC4,AxisSKC5,AxisSKC6,AxisSKC7,AxisSKC8,ZZ,CS,XZPara,XZZZ,ZS,AxisSKC1,AxisSKC2,AxisSKC3 from LogMainTable ");
            strSql.Append(" where LogID=@LogID ");
            SqlParameter[] parameters = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,50)           };
            parameters[0].Value = LogID;

            LogMainTable model = new LogMainTable();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                #region  父表信息
                if (ds.Tables[0].Rows[0]["LogID"] != null && ds.Tables[0].Rows[0]["LogID"].ToString() != "")
                {
                    model.LogID = ds.Tables[0].Rows[0]["LogID"].ToString();
                }
                if (ds.Tables[0].Rows[0]["AxisSKC4"] != null && ds.Tables[0].Rows[0]["AxisSKC4"].ToString() != "")
                {
                    model.AxisSKC4 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC4"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC5"] != null && ds.Tables[0].Rows[0]["AxisSKC5"].ToString() != "")
                {
                    model.AxisSKC5 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC5"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC6"] != null && ds.Tables[0].Rows[0]["AxisSKC6"].ToString() != "")
                {
                    model.AxisSKC6 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC6"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC7"] != null && ds.Tables[0].Rows[0]["AxisSKC7"].ToString() != "")
                {
                    model.AxisSKC7 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC7"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC8"] != null && ds.Tables[0].Rows[0]["AxisSKC8"].ToString() != "")
                {
                    model.AxisSKC8 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC8"].ToString());
                }
                if (ds.Tables[0].Rows[0]["ZZ"] != null && ds.Tables[0].Rows[0]["ZZ"].ToString() != "")
                {
                    model.ZZ = int.Parse(ds.Tables[0].Rows[0]["ZZ"].ToString());
                }
                if (ds.Tables[0].Rows[0]["CS"] != null && ds.Tables[0].Rows[0]["CS"].ToString() != "")
                {
                    model.CS = int.Parse(ds.Tables[0].Rows[0]["CS"].ToString());
                }
                if (ds.Tables[0].Rows[0]["XZPara"] != null && ds.Tables[0].Rows[0]["XZPara"].ToString() != "")
                {
                    model.XZPara = decimal.Parse(ds.Tables[0].Rows[0]["XZPara"].ToString());
                }
                if (ds.Tables[0].Rows[0]["XZZZ"] != null && ds.Tables[0].Rows[0]["XZZZ"].ToString() != "")
                {
                    model.XZZZ = int.Parse(ds.Tables[0].Rows[0]["XZZZ"].ToString());
                }
                if (ds.Tables[0].Rows[0]["ZS"] != null && ds.Tables[0].Rows[0]["ZS"].ToString() != "")
                {
                    model.ZS = int.Parse(ds.Tables[0].Rows[0]["ZS"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC1"] != null && ds.Tables[0].Rows[0]["AxisSKC1"].ToString() != "")
                {
                    model.AxisSKC1 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC1"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC2"] != null && ds.Tables[0].Rows[0]["AxisSKC2"].ToString() != "")
                {
                    model.AxisSKC2 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC2"].ToString());
                }
                if (ds.Tables[0].Rows[0]["AxisSKC3"] != null && ds.Tables[0].Rows[0]["AxisSKC3"].ToString() != "")
                {
                    model.AxisSKC3 = int.Parse(ds.Tables[0].Rows[0]["AxisSKC3"].ToString());
                }
                #endregion  父表信息end

                #region  子表信息
                StringBuilder strSql2 = new StringBuilder();
                strSql2.Append("select ID,LogID,CBNo,JLZ,SK,CBZZ,UsedSKC from LogDetailTable ");
                strSql2.Append(" where LogID=@LogID ");
                SqlParameter[] parameters2 = {
                    new SqlParameter("@LogID", SqlDbType.NVarChar,-1)};
                parameters2[0].Value = LogID;

                DataSet ds2 = DbHelperSQL.Query(strSql2.ToString(), parameters2);
                if (ds2.Tables[0].Rows.Count > 0)
                {
                    #region  子表字段信息
                    int i = ds2.Tables[0].Rows.Count;
                    List<LogDetailTable> models = new List<LogDetailTable>();
                    LogDetailTable modelt;
                    for (int n = 0; n < i; n++)
                    {
                        modelt = new LogDetailTable();
                        if (ds2.Tables[0].Rows[n]["ID"] != null && ds2.Tables[0].Rows[n]["ID"].ToString() != "")
                        {
                            modelt.ID = int.Parse(ds2.Tables[0].Rows[n]["ID"].ToString());
                        }
                        if (ds2.Tables[0].Rows[n]["LogID"] != null && ds2.Tables[0].Rows[n]["LogID"].ToString() != "")
                        {
                            modelt.LogID = ds2.Tables[0].Rows[n]["LogID"].ToString();
                        }
                        if (ds2.Tables[0].Rows[n]["CBNo"] != null && ds2.Tables[0].Rows[n]["CBNo"].ToString() != "")
                        {
                            modelt.CBNo = int.Parse(ds2.Tables[0].Rows[n]["CBNo"].ToString());
                        }
                        if (ds2.Tables[0].Rows[n]["JLZ"] != null && ds2.Tables[0].Rows[n]["JLZ"].ToString() != "")
                        {
                            modelt.JLZ = int.Parse(ds2.Tables[0].Rows[n]["JLZ"].ToString());
                        }
                        if (ds2.Tables[0].Rows[n]["SK"] != null && ds2.Tables[0].Rows[n]["SK"].ToString() != "")
                        {
                            modelt.SK = int.Parse(ds2.Tables[0].Rows[n]["SK"].ToString());
                        }
                        if (ds2.Tables[0].Rows[n]["CBZZ"] != null && ds2.Tables[0].Rows[n]["CBZZ"].ToString() != "")
                        {
                            modelt.CBZZ = int.Parse(ds2.Tables[0].Rows[n]["CBZZ"].ToString());
                        }
                        if (ds2.Tables[0].Rows[n]["UsedSKC"] != null && ds2.Tables[0].Rows[n]["UsedSKC"].ToString() != "")
                        {
                            modelt.UsedSKC = int.Parse(ds2.Tables[0].Rows[n]["UsedSKC"].ToString());
                        }
                        models.Add(modelt);
                    }
                    model.LogDetailTables = models;
                    #endregion  子表字段信息end
                }
                #endregion  子表信息end

                return model;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public LogMainTable DataRowToModel(DataRow row)
        {
            LogMainTable model = new LogMainTable();
            if (row != null)
            {
                if (row["LogID"] != null && row["LogID"].ToString() != "")
                {
                    model.LogID = row["LogID"].ToString();
                }
                if (row["AxisSKC4"] != null && row["AxisSKC4"].ToString() != "")
                {
                    model.AxisSKC4 = int.Parse(row["AxisSKC4"].ToString());
                }
                if (row["AxisSKC5"] != null && row["AxisSKC5"].ToString() != "")
                {
                    model.AxisSKC5 = int.Parse(row["AxisSKC5"].ToString());
                }
                if (row["AxisSKC6"] != null && row["AxisSKC6"].ToString() != "")
                {
                    model.AxisSKC6 = int.Parse(row["AxisSKC6"].ToString());
                }
                if (row["AxisSKC7"] != null && row["AxisSKC7"].ToString() != "")
                {
                    model.AxisSKC7 = int.Parse(row["AxisSKC7"].ToString());
                }
                if (row["AxisSKC8"] != null && row["AxisSKC8"].ToString() != "")
                {
                    model.AxisSKC8 = int.Parse(row["AxisSKC8"].ToString());
                }
                if (row["ZZ"] != null && row["ZZ"].ToString() != "")
                {
                    model.ZZ = int.Parse(row["ZZ"].ToString());
                }
                if (row["CS"] != null && row["CS"].ToString() != "")
                {
                    model.CS = int.Parse(row["CS"].ToString());
                }
                if (row["XZPara"] != null && row["XZPara"].ToString() != "")
                {
                    model.XZPara = decimal.Parse(row["XZPara"].ToString());
                }
                if (row["XZZZ"] != null && row["XZZZ"].ToString() != "")
                {
                    model.XZZZ = int.Parse(row["XZZZ"].ToString());
                }
                if (row["ZS"] != null && row["ZS"].ToString() != "")
                {
                    model.ZS = int.Parse(row["ZS"].ToString());
                }
                if (row["AxisSKC1"] != null && row["AxisSKC1"].ToString() != "")
                {
                    model.AxisSKC1 = int.Parse(row["AxisSKC1"].ToString());
                }
                if (row["AxisSKC2"] != null && row["AxisSKC2"].ToString() != "")
                {
                    model.AxisSKC2 = int.Parse(row["AxisSKC2"].ToString());
                }
                if (row["AxisSKC3"] != null && row["AxisSKC3"].ToString() != "")
                {
                    model.AxisSKC3 = int.Parse(row["AxisSKC3"].ToString());
                }
            }
            return model;
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetList(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select LogID,AxisSKC4,AxisSKC5,AxisSKC6,AxisSKC7,AxisSKC8,ZZ,CS,XZPara,XZZZ,ZS,AxisSKC1,AxisSKC2,AxisSKC3 ");
            strSql.Append(" FROM LogMainTable ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select ");
            if (Top > 0)
            {
                strSql.Append(" top " + Top.ToString());
            }
            strSql.Append(" LogID,AxisSKC4,AxisSKC5,AxisSKC6,AxisSKC7,AxisSKC8,ZZ,CS,XZPara,XZZZ,ZS,AxisSKC1,AxisSKC2,AxisSKC3 ");
            strSql.Append(" FROM LogMainTable ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            strSql.Append(" order by " + filedOrder);
            return DbHelperSQL.Query(strSql.ToString());
        }


        #endregion  Method
    }
}

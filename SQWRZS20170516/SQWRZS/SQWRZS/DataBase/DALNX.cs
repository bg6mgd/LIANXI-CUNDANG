using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    public class DALNX
    {
        public DALNX()
        { }
        #region  BasicMethod
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from NXTable");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
                    new SqlParameter("@ID", SqlDbType.Int,4)
            };
            parameters[0].Value = ID;

            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }


        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(NXTable model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into NXTable(");
            strSql.Append("CPH,CPYS,CPTX,PLATE,CWTX,CMTX,Video,JCSJ)");
            strSql.Append(" values (");
            strSql.Append("@CPH,@CPYS,@CPTX,@PLATE,@CWTX,@CMTX,@Video,@JCSJ)");
            strSql.Append(";select @@IDENTITY");
            SqlParameter[] parameters = {
                    new SqlParameter("@CPH", SqlDbType.NVarChar,15),
                    new SqlParameter("@CPYS", SqlDbType.NVarChar,10),
                    new SqlParameter("@CPTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@PLATE", SqlDbType.NVarChar,200),
                    new SqlParameter("@CWTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@CMTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@Video", SqlDbType.NVarChar,200),
                    new SqlParameter("@JCSJ", SqlDbType.DateTime)};
            parameters[0].Value = model.CPH;
            parameters[1].Value = model.CPYS;
            parameters[2].Value = model.CPTX;
            parameters[3].Value = model.PLATE;
            parameters[4].Value = model.CWTX;
            parameters[5].Value = model.CMTX;
            parameters[6].Value = model.Video;
            parameters[7].Value = model.JCSJ;

            object obj = DbHelperSQL.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(NXTable model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update NXTable set ");
            strSql.Append("CPH=@CPH,");
            strSql.Append("CPYS=@CPYS,");
            strSql.Append("CPTX=@CPTX,");
            strSql.Append("PLATE=@PLATE,");
            strSql.Append("CWTX=@CWTX,");
            strSql.Append("CMTX=@CMTX,");
            strSql.Append("Video=@Video,");
            strSql.Append("JCSJ=@JCSJ");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
                    new SqlParameter("@CPH", SqlDbType.NVarChar,15),
                    new SqlParameter("@CPYS", SqlDbType.NVarChar,10),
                    new SqlParameter("@CPTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@PLATE", SqlDbType.NVarChar,200),
                    new SqlParameter("@CWTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@CMTX", SqlDbType.NVarChar,200),
                    new SqlParameter("@Video", SqlDbType.NVarChar,200),
                    new SqlParameter("@JCSJ", SqlDbType.DateTime),
                    new SqlParameter("@ID", SqlDbType.Int,4)};
            parameters[0].Value = model.CPH;
            parameters[1].Value = model.CPYS;
            parameters[2].Value = model.CPTX;
            parameters[3].Value = model.PLATE;
            parameters[4].Value = model.CWTX;
            parameters[5].Value = model.CMTX;
            parameters[6].Value = model.Video;
            parameters[7].Value = model.JCSJ;
            parameters[8].Value = model.ID;

            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
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
        public bool Delete(int ID)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from NXTable ");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
                    new SqlParameter("@ID", SqlDbType.Int,4)
            };
            parameters[0].Value = ID;

            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 批量删除数据
        /// </summary>
        public bool DeleteList(string IDlist)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from NXTable ");
            strSql.Append(" where ID in (" + IDlist + ")  ");
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rows > 0)
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
        public NXTable GetModel(int ID)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 ID,CPH,CPYS,CPTX,PLATE,CWTX,CMTX,Video,JCSJ from NXTable ");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
                    new SqlParameter("@ID", SqlDbType.Int,4)
            };
            parameters[0].Value = ID;

            NXTable model = new NXTable();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DataRowToModel(ds.Tables[0].Rows[0]);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public NXTable DataRowToModel(DataRow row)
        {
            NXTable model = new NXTable();
            if (row != null)
            {
                if (row["ID"] != null && row["ID"].ToString() != "")
                {
                    model.ID = int.Parse(row["ID"].ToString());
                }
                if (row["CPH"] != null)
                {
                    model.CPH = row["CPH"].ToString();
                }
                if (row["CPYS"] != null)
                {
                    model.CPYS = row["CPYS"].ToString();
                }
                if (row["CPTX"] != null)
                {
                    model.CPTX = row["CPTX"].ToString();
                }
                if (row["PLATE"] != null)
                {
                    model.PLATE = row["PLATE"].ToString();
                }
                if (row["CWTX"] != null)
                {
                    model.CWTX = row["CWTX"].ToString();
                }
                if (row["CMTX"] != null)
                {
                    model.CMTX = row["CMTX"].ToString();
                }
                if (row["Video"] != null)
                {
                    model.Video = row["Video"].ToString();
                }
                if (row["JCSJ"] != null && row["JCSJ"].ToString() != "")
                {
                    model.JCSJ = DateTime.Parse(row["JCSJ"].ToString());
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
            strSql.Append("select ID,CPH,CPYS,CPTX,PLATE,CWTX,CMTX,Video,JCSJ ");
            strSql.Append(" FROM NXTable ");
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
            strSql.Append(" ID,CPH,CPYS,CPTX,PLATE,CWTX,CMTX,Video,JCSJ ");
            strSql.Append(" FROM NXTable ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            strSql.Append(" order by " + filedOrder);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获取记录总数
        /// </summary>
        public int GetRecordCount(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) FROM NXTable ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            object obj = DbHelperSQL.GetSingle(strSql.ToString());
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }
        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM ( ");
            strSql.Append(" SELECT ROW_NUMBER() OVER (");
            if (!string.IsNullOrEmpty(orderby.Trim()))
            {
                strSql.Append("order by T." + orderby);
            }
            else
            {
                strSql.Append("order by T.ID desc");
            }
            strSql.Append(")AS Row, T.*  from NXTable T ");
            if (!string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(" ) TT");
            strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /*
		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		public DataSet GetList(int PageSize,int PageIndex,string strWhere)
		{
			SqlParameter[] parameters = {
					new SqlParameter("@tblName", SqlDbType.VarChar, 255),
					new SqlParameter("@fldName", SqlDbType.VarChar, 255),
					new SqlParameter("@PageSize", SqlDbType.Int),
					new SqlParameter("@PageIndex", SqlDbType.Int),
					new SqlParameter("@IsReCount", SqlDbType.Bit),
					new SqlParameter("@OrderType", SqlDbType.Bit),
					new SqlParameter("@strWhere", SqlDbType.VarChar,1000),
					};
			parameters[0].Value = "NXTable";
			parameters[1].Value = "ID";
			parameters[2].Value = PageSize;
			parameters[3].Value = PageIndex;
			parameters[4].Value = 0;
			parameters[5].Value = 0;
			parameters[6].Value = strWhere;	
			return DbHelperSQL.RunProcedure("UP_GetRecordByPage",parameters,"ds");
		}*/

        #endregion  BasicMethod
        #region  ExtensionMethod

        #endregion  ExtensionMethod
    }
}

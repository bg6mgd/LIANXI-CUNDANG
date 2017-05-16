using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace SQWRZS.DataBase
{
    class DALCBData
    {
        public DALCBData()
		{}
		#region  BasicMethod
		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public bool Exists(int ID)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select count(1) from CBData");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

			return DbHelperSQL.Exists(strSql.ToString(),parameters);
		}


		/// <summary>
		/// 增加一条数据
		/// </summary>
        public int Add(CBDataTable model)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("insert into CBData(");
			strSql.Append("TDH,JLZ,SK,SKC,CKZ,JSSJ,YSSJ)");
			strSql.Append(" values (");
			strSql.Append("@TDH,@JLZ,@SK,@SKC,@CKZ,@JSSJ,@YSSJ)");
			strSql.Append(";select @@IDENTITY");
			SqlParameter[] parameters = {
					new SqlParameter("@TDH", SqlDbType.NVarChar,100),
					new SqlParameter("@JLZ", SqlDbType.NVarChar,500),
					new SqlParameter("@SK", SqlDbType.NVarChar,500),
					new SqlParameter("@SKC", SqlDbType.NVarChar,500),
					new SqlParameter("@CKZ", SqlDbType.NVarChar,500),
					new SqlParameter("@JSSJ", SqlDbType.NVarChar,1000),
					new SqlParameter("@YSSJ", SqlDbType.NVarChar,4000)};
			parameters[0].Value = model.TDH;
			parameters[1].Value = model.JLZ;
			parameters[2].Value = model.SK;
			parameters[3].Value = model.SKC;
			parameters[4].Value = model.CKZ;
			parameters[5].Value = model.JSSJ;
			parameters[6].Value = model.YSSJ;

			object obj = DbHelperSQL.GetSingle(strSql.ToString(),parameters);
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
        public bool Update(CBDataTable model)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("update CBData set ");
			strSql.Append("TDH=@TDH,");
			strSql.Append("JLZ=@JLZ,");
			strSql.Append("SK=@SK,");
			strSql.Append("SKC=@SKC,");
			strSql.Append("CKZ=@CKZ,");
			strSql.Append("JSSJ=@JSSJ,");
			strSql.Append("YSSJ=@YSSJ");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@TDH", SqlDbType.NVarChar,100),
					new SqlParameter("@JLZ", SqlDbType.NVarChar,500),
					new SqlParameter("@SK", SqlDbType.NVarChar,500),
					new SqlParameter("@SKC", SqlDbType.NVarChar,500),
					new SqlParameter("@CKZ", SqlDbType.NVarChar,500),
					new SqlParameter("@JSSJ", SqlDbType.NVarChar,1000),
					new SqlParameter("@YSSJ", SqlDbType.NVarChar,4000),
					new SqlParameter("@ID", SqlDbType.Int,4)};
			parameters[0].Value = model.TDH;
			parameters[1].Value = model.JLZ;
			parameters[2].Value = model.SK;
			parameters[3].Value = model.SKC;
			parameters[4].Value = model.CKZ;
			parameters[5].Value = model.JSSJ;
			parameters[6].Value = model.YSSJ;
			parameters[7].Value = model.ID;

			int rows=DbHelperSQL.ExecuteSql(strSql.ToString(),parameters);
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
			
			StringBuilder strSql=new StringBuilder();
			strSql.Append("delete from CBData ");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

			int rows=DbHelperSQL.ExecuteSql(strSql.ToString(),parameters);
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
		public bool DeleteList(string IDlist )
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("delete from CBData ");
			strSql.Append(" where ID in ("+IDlist + ")  ");
			int rows=DbHelperSQL.ExecuteSql(strSql.ToString());
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
        public CBDataTable GetModel(int ID)
		{
			
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select  top 1 ID,TDH,JLZ,SK,SKC,CKZ,JSSJ,YSSJ from CBData ");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

            CBDataTable model = new CBDataTable();
			DataSet ds=DbHelperSQL.Query(strSql.ToString(),parameters);
			if(ds.Tables[0].Rows.Count>0)
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
        public CBDataTable DataRowToModel(DataRow row)
		{
            CBDataTable model = new CBDataTable();
			if (row != null)
			{
				if(row["ID"]!=null && row["ID"].ToString()!="")
				{
					model.ID=int.Parse(row["ID"].ToString());
				}
				if(row["TDH"]!=null)
				{
					model.TDH=row["TDH"].ToString();
				}
				if(row["JLZ"]!=null)
				{
					model.JLZ=row["JLZ"].ToString();
				}
				if(row["SK"]!=null)
				{
					model.SK=row["SK"].ToString();
				}
				if(row["SKC"]!=null)
				{
					model.SKC=row["SKC"].ToString();
				}
				if(row["CKZ"]!=null)
				{
					model.CKZ=row["CKZ"].ToString();
				}
				if(row["JSSJ"]!=null)
				{
					model.JSSJ=row["JSSJ"].ToString();
				}
				if(row["YSSJ"]!=null)
				{
					model.YSSJ=row["YSSJ"].ToString();
				}
			}
			return model;
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public DataSet GetList(string strWhere)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select ID,TDH,JLZ,SK,SKC,CKZ,JSSJ,YSSJ ");
			strSql.Append(" FROM CBData ");
			if(strWhere.Trim()!="")
			{
				strSql.Append(" where "+strWhere);
			}
			return DbHelperSQL.Query(strSql.ToString());
		}

		/// <summary>
		/// 获得前几行数据
		/// </summary>
		public DataSet GetList(int Top,string strWhere,string filedOrder)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select ");
			if(Top>0)
			{
				strSql.Append(" top "+Top.ToString());
			}
			strSql.Append(" ID,TDH,JLZ,SK,SKC,CKZ,JSSJ,YSSJ ");
			strSql.Append(" FROM CBData ");
			if(strWhere.Trim()!="")
			{
				strSql.Append(" where "+strWhere);
			}
			strSql.Append(" order by " + filedOrder);
			return DbHelperSQL.Query(strSql.ToString());
		}

		/// <summary>
		/// 获取记录总数
		/// </summary>
		public int GetRecordCount(string strWhere)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select count(1) FROM CBData ");
			if(strWhere.Trim()!="")
			{
				strSql.Append(" where "+strWhere);
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
			StringBuilder strSql=new StringBuilder();
			strSql.Append("SELECT * FROM ( ");
			strSql.Append(" SELECT ROW_NUMBER() OVER (");
			if (!string.IsNullOrEmpty(orderby.Trim()))
			{
				strSql.Append("order by T." + orderby );
			}
			else
			{
				strSql.Append("order by T.ID desc");
			}
			strSql.Append(")AS Row, T.*  from CBData T ");
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
			parameters[0].Value = "CBData";
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

using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Maticsoft.DBUtility;//Please add references
namespace Maticsoft.DAL.test
{
	/// <summary>
	/// 数据访问类:Table_1
	/// </summary>
	public partial class Table_1
	{
		public Table_1()
		{}
		#region  Method



		/// <summary>
		/// 增加一条数据
		/// </summary>
		public bool Add(Maticsoft.Model.test.Table_1 model)
		{
			StringBuilder strSql=new StringBuilder();
			StringBuilder strSql1=new StringBuilder();
			StringBuilder strSql2=new StringBuilder();
			if (model.name != null)
			{
				strSql1.Append("name,");
				strSql2.Append("'"+model.name+"',");
			}
			if (model.nianling != null)
			{
				strSql1.Append("nianling,");
				strSql2.Append("'"+model.nianling+"',");
			}
			if (model.shenggao != null)
			{
				strSql1.Append("shenggao,");
				strSql2.Append("'"+model.shenggao+"',");
			}
			if (model.bianhao != null)
			{
				strSql1.Append("bianhao,");
				strSql2.Append("'"+model.bianhao+"',");
			}
			strSql.Append("insert into Table_1(");
			strSql.Append(strSql1.ToString().Remove(strSql1.Length - 1));
			strSql.Append(")");
			strSql.Append(" values (");
			strSql.Append(strSql2.ToString().Remove(strSql2.Length - 1));
			strSql.Append(")");
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
		/// 更新一条数据
		/// </summary>
		public bool Update(Maticsoft.Model.test.Table_1 model)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("update Table_1 set ");
			if (model.name != null)
			{
				strSql.Append("name='"+model.name+"',");
			}
			else
			{
				strSql.Append("name= null ,");
			}
			if (model.nianling != null)
			{
				strSql.Append("nianling='"+model.nianling+"',");
			}
			else
			{
				strSql.Append("nianling= null ,");
			}
			if (model.shenggao != null)
			{
				strSql.Append("shenggao='"+model.shenggao+"',");
			}
			else
			{
				strSql.Append("shenggao= null ,");
			}
			if (model.bianhao != null)
			{
				strSql.Append("bianhao='"+model.bianhao+"',");
			}
			else
			{
				strSql.Append("bianhao= null ,");
			}
			int n = strSql.ToString().LastIndexOf(",");
			strSql.Remove(n, 1);
			strSql.Append(" where ");
			int rowsAffected=DbHelperSQL.ExecuteSql(strSql.ToString());
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
		public bool Delete()
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("delete from Table_1 ");
			strSql.Append(" where " );
			int rowsAffected=DbHelperSQL.ExecuteSql(strSql.ToString());
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
		public Maticsoft.Model.test.Table_1 GetModel()
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select  top 1  ");
			strSql.Append(" name,nianling,shenggao,bianhao ");
			strSql.Append(" from Table_1 ");
			strSql.Append(" where " );
			Maticsoft.Model.test.Table_1 model=new Maticsoft.Model.test.Table_1();
			DataSet ds=DbHelperSQL.Query(strSql.ToString());
			if(ds.Tables[0].Rows.Count>0)
			{
				if(ds.Tables[0].Rows[0]["name"]!=null && ds.Tables[0].Rows[0]["name"].ToString()!="")
				{
					model.name=ds.Tables[0].Rows[0]["name"].ToString();
				}
				if(ds.Tables[0].Rows[0]["nianling"]!=null && ds.Tables[0].Rows[0]["nianling"].ToString()!="")
				{
					model.nianling=ds.Tables[0].Rows[0]["nianling"].ToString();
				}
				if(ds.Tables[0].Rows[0]["shenggao"]!=null && ds.Tables[0].Rows[0]["shenggao"].ToString()!="")
				{
					model.shenggao=ds.Tables[0].Rows[0]["shenggao"].ToString();
				}
				if(ds.Tables[0].Rows[0]["bianhao"]!=null && ds.Tables[0].Rows[0]["bianhao"].ToString()!="")
				{
					model.bianhao=ds.Tables[0].Rows[0]["bianhao"].ToString();
				}
				return model;
			}
			else
			{
				return null;
			}
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public DataSet GetList(string strWhere)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select name,nianling,shenggao,bianhao ");
			strSql.Append(" FROM Table_1 ");
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
			strSql.Append(" name,nianling,shenggao,bianhao ");
			strSql.Append(" FROM Table_1 ");
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
			strSql.Append("select count(1) FROM Table_1 ");
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
				strSql.Append("order by T.userid desc");
			}
			strSql.Append(")AS Row, T.*  from Table_1 T ");
			if (!string.IsNullOrEmpty(strWhere.Trim()))
			{
				strSql.Append(" WHERE " + strWhere);
			}
			strSql.Append(" ) TT");
			strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
			return DbHelperSQL.Query(strSql.ToString());
		}

		/*
		*/

		#endregion  Method
	}
}


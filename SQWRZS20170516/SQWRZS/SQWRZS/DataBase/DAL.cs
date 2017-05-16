using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace SQWRZS.DataBase
{
    public class DAL
    {
        public DAL()
        { }
        #region  BasicMethod


        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(ZH_ST_ZDCZB model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into zh_st_zdczb(");
            strSql.Append("CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX,JCSJ,ZX,CD,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,SJDJ,PLATE,FX,CWTX,CMTX,Video,SFSC)");
            strSql.Append(" values (");
            strSql.Append("@CPH,@CPYS,@ZZ,@ZS,@CS,@XZ,@CXL,@CPTX,@JCSJ,@ZX,@CD,@CZY,@ZDBZ,@SFCX,@SFXZ,@FJBZ,@XHZL,@JCZT,@SJDJ,@PLATE,@FX,@CWTX,@CMTX,@Video,@SFSC)");
            strSql.Append(";select @@IDENTITY");
            SqlParameter[] parameters = {
					new SqlParameter("@CPH", SqlDbType.NVarChar,50),
					new SqlParameter("@CPYS", SqlDbType.NVarChar,50),
					new SqlParameter("@ZZ", SqlDbType.BigInt,8),
					new SqlParameter("@ZS", SqlDbType.Int,4),
					new SqlParameter("@CS", SqlDbType.Decimal,5),
					new SqlParameter("@XZ", SqlDbType.BigInt,8),
					new SqlParameter("@CXL", SqlDbType.Int,4),
					new SqlParameter("@CPTX", SqlDbType.NVarChar,500),
					new SqlParameter("@JCSJ", SqlDbType.DateTime),
					new SqlParameter("@ZX", SqlDbType.Int,4),
					new SqlParameter("@CD", SqlDbType.Int,4),
					new SqlParameter("@CZY", SqlDbType.NVarChar,50),
					new SqlParameter("@ZDBZ", SqlDbType.NVarChar,60),
					new SqlParameter("@SFCX", SqlDbType.Int,4),
					new SqlParameter("@SFXZ", SqlDbType.Int,4),
					new SqlParameter("@FJBZ", SqlDbType.Int,4),
					new SqlParameter("@XHZL", SqlDbType.BigInt,8),
					new SqlParameter("@JCZT", SqlDbType.Int,4),
					new SqlParameter("@SJDJ", SqlDbType.Int,4),
					new SqlParameter("@PLATE", SqlDbType.NVarChar,100),
					new SqlParameter("@FX", SqlDbType.Char,10),
                    new SqlParameter("@CWTX",SqlDbType.NVarChar,500),
                    new SqlParameter("@CMTX",SqlDbType.NVarChar,500),
                    new SqlParameter("@Video",SqlDbType.NVarChar,500),
                    new SqlParameter("@SFSC",SqlDbType.Int,4),};
            parameters[0].Value = model.CPH;
            parameters[1].Value = model.CPYS;
            parameters[2].Value = model.ZZ;
            parameters[3].Value = model.ZS;
            parameters[4].Value = model.CS;
            parameters[5].Value = model.XZ;
            parameters[6].Value = model.CXL;
            parameters[7].Value = model.CPTX;
            parameters[8].Value = model.JCSJ;
            parameters[9].Value = model.ZX;
            parameters[10].Value = model.CD;
            parameters[11].Value = model.CZY;
            parameters[12].Value = model.ZDBZ;
            parameters[13].Value = model.SFCX;
            parameters[14].Value = model.SFXZ;
            parameters[15].Value = model.FJBZ;
            parameters[16].Value = model.XHZL;
            parameters[17].Value = model.JCZT;
            parameters[18].Value = model.SJDJ;
            parameters[19].Value = model.PLATE;
            parameters[20].Value = model.FX;
            parameters[21].Value = model.CWTX;
            parameters[22].Value = model.CMTX;
            parameters[23].Value = model.Video;
            parameters[24].Value = model.SFSC;

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
        public bool Update(ZH_ST_ZDCZB model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update zh_st_zdczb set ");
            strSql.Append("CPH=@CPH,");
            strSql.Append("CPYS=@CPYS,");
            strSql.Append("ZZ=@ZZ,");
            strSql.Append("ZS=@ZS,");
            strSql.Append("CS=@CS,");
            strSql.Append("XZ=@XZ,");
            strSql.Append("CXL=@CXL,");
            strSql.Append("CPTX=@CPTX,");
            strSql.Append("JCSJ=@JCSJ,");
            strSql.Append("ZX=@ZX,");
            strSql.Append("CD=@CD,");
            strSql.Append("CZY=@CZY,");
            strSql.Append("ZDBZ=@ZDBZ,");
            strSql.Append("SFCX=@SFCX,");
            strSql.Append("SFXZ=@SFXZ,");
            strSql.Append("FJBZ=@FJBZ,");
            strSql.Append("XHZL=@XHZL,");
            strSql.Append("JCZT=@JCZT,");
            strSql.Append("SJDJ=@SJDJ,");
            strSql.Append("PLATE=@PLATE,");
            strSql.Append("FX=@FX");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
					new SqlParameter("@CPH", SqlDbType.NVarChar,50),
					new SqlParameter("@CPYS", SqlDbType.NVarChar,50),
					new SqlParameter("@ZZ", SqlDbType.BigInt,8),
					new SqlParameter("@ZS", SqlDbType.Int,4),
					new SqlParameter("@CS", SqlDbType.Decimal,5),
					new SqlParameter("@XZ", SqlDbType.BigInt,8),
					new SqlParameter("@CXL", SqlDbType.Int,4),
					new SqlParameter("@CPTX", SqlDbType.NVarChar,500),
					new SqlParameter("@JCSJ", SqlDbType.DateTime),
					new SqlParameter("@ZX", SqlDbType.Int,4),
					new SqlParameter("@CD", SqlDbType.Int,4),
					new SqlParameter("@CZY", SqlDbType.NVarChar,50),
					new SqlParameter("@ZDBZ", SqlDbType.NVarChar,60),
					new SqlParameter("@SFCX", SqlDbType.Int,4),
					new SqlParameter("@SFXZ", SqlDbType.Int,4),
					new SqlParameter("@FJBZ", SqlDbType.Int,4),
					new SqlParameter("@XHZL", SqlDbType.BigInt,8),
					new SqlParameter("@JCZT", SqlDbType.Int,4),
					new SqlParameter("@SJDJ", SqlDbType.Int,4),
					new SqlParameter("@PLATE", SqlDbType.NVarChar,100),
					new SqlParameter("@FX", SqlDbType.Char,10),
					new SqlParameter("@ID", SqlDbType.Int,4)};
            parameters[0].Value = model.CPH;
            parameters[1].Value = model.CPYS;
            parameters[2].Value = model.ZZ;
            parameters[3].Value = model.ZS;
            parameters[4].Value = model.CS;
            parameters[5].Value = model.XZ;
            parameters[6].Value = model.CXL;
            parameters[7].Value = model.CPTX;
            parameters[8].Value = model.JCSJ;
            parameters[9].Value = model.ZX;
            parameters[10].Value = model.CD;
            parameters[11].Value = model.CZY;
            parameters[12].Value = model.ZDBZ;
            parameters[13].Value = model.SFCX;
            parameters[14].Value = model.SFXZ;
            parameters[15].Value = model.FJBZ;
            parameters[16].Value = model.XHZL;
            parameters[17].Value = model.JCZT;
            parameters[18].Value = model.SJDJ;
            parameters[19].Value = model.PLATE;
            parameters[20].Value = model.FX;
            parameters[21].Value = model.ID;

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
            strSql.Append("delete from zh_st_zdczb ");
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
            strSql.Append("delete from zh_st_zdczb ");
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
        public ZH_ST_ZDCZB GetModel(int ID)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 ID,CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX,JCSJ,ZX,CD,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,SJDJ,PLATE,FX from zh_st_zdczb ");
            strSql.Append(" where ID=@ID");
            SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
            parameters[0].Value = ID;

            ZH_ST_ZDCZB model = new ZH_ST_ZDCZB();
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
        public ZH_ST_ZDCZB DataRowToModel(DataRow row)
        {
            ZH_ST_ZDCZB model = new ZH_ST_ZDCZB();
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
                if (row["ZZ"] != null && row["ZZ"].ToString() != "")
                {
                    model.ZZ = long.Parse(row["ZZ"].ToString());
                }
                if (row["ZS"] != null && row["ZS"].ToString() != "")
                {
                    model.ZS = int.Parse(row["ZS"].ToString());
                }
                if (row["CS"] != null && row["CS"].ToString() != "")
                {
                    model.CS = decimal.Parse(row["CS"].ToString());
                }
                if (row["XZ"] != null && row["XZ"].ToString() != "")
                {
                    model.XZ = long.Parse(row["XZ"].ToString());
                }
                if (row["CXL"] != null && row["CXL"].ToString() != "")
                {
                    model.CXL = int.Parse(row["CXL"].ToString());
                }
                if (row["CPTX"] != null)
                {
                    model.CPTX = row["CPTX"].ToString();
                }
                if (row["JCSJ"] != null && row["JCSJ"].ToString() != "")
                {
                    model.JCSJ = DateTime.Parse(row["JCSJ"].ToString());
                }
                if (row["ZX"] != null && row["ZX"].ToString() != "")
                {
                    model.ZX = int.Parse(row["ZX"].ToString());
                }
                if (row["CD"] != null && row["CD"].ToString() != "")
                {
                    model.CD = int.Parse(row["CD"].ToString());
                }
                if (row["CZY"] != null)
                {
                    model.CZY = row["CZY"].ToString();
                }
                if (row["ZDBZ"] != null)
                {
                    model.ZDBZ = row["ZDBZ"].ToString();
                }
                if (row["SFCX"] != null && row["SFCX"].ToString() != "")
                {
                    model.SFCX = int.Parse(row["SFCX"].ToString());
                }
                if (row["SFXZ"] != null && row["SFXZ"].ToString() != "")
                {
                    model.SFXZ = int.Parse(row["SFXZ"].ToString());
                }
                if (row["FJBZ"] != null && row["FJBZ"].ToString() != "")
                {
                    model.FJBZ = int.Parse(row["FJBZ"].ToString());
                }
                if (row["XHZL"] != null && row["XHZL"].ToString() != "")
                {
                    model.XHZL = long.Parse(row["XHZL"].ToString());
                }
                if (row["JCZT"] != null && row["JCZT"].ToString() != "")
                {
                    model.JCZT = int.Parse(row["JCZT"].ToString());
                }
                if (row["SJDJ"] != null && row["SJDJ"].ToString() != "")
                {
                    model.SJDJ = int.Parse(row["SJDJ"].ToString());
                }
                if (row["PLATE"] != null)
                {
                    model.PLATE = row["PLATE"].ToString();
                }
                if (row["FX"] != null)
                {
                    model.FX = row["FX"].ToString();
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
            strSql.Append("select ID,CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX,JCSJ,ZX,CD,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,SJDJ,PLATE,FX ");
            strSql.Append(" FROM zh_st_zdczb ");
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
            strSql.Append(" ID,CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX,JCSJ,ZX,CD,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,SJDJ,PLATE,FX ");
            strSql.Append(" FROM zh_st_zdczb ");
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
            strSql.Append("select count(1) FROM zh_st_zdczb ");
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
            strSql.Append(")AS Row, T.*  from zh_st_zdczb T ");
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
            parameters[0].Value = "zh_st_zdczb";
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

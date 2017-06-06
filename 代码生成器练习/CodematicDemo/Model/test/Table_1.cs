using System;
namespace Maticsoft.Model.test
{
	/// <summary>
	/// Table_1:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
	public partial class Table_1
	{
		public Table_1()
		{}
		#region Model
		private string _name;
		private string _nianling;
		private string _shenggao;
		private string _bianhao;
		/// <summary>
		/// 
		/// </summary>
		public string name
		{
			set{ _name=value;}
			get{return _name;}
		}
		/// <summary>
		/// 
		/// </summary>
		public string nianling
		{
			set{ _nianling=value;}
			get{return _nianling;}
		}
		/// <summary>
		/// 
		/// </summary>
		public string shenggao
		{
			set{ _shenggao=value;}
			get{return _shenggao;}
		}
		/// <summary>
		/// 
		/// </summary>
		public string bianhao
		{
			set{ _bianhao=value;}
			get{return _bianhao;}
		}
		#endregion Model

	}
}


﻿using System;
using System.Collections.Generic;
using Dos.ORM;

namespace AideM
{
    /// <summary>
    /// 实体类User。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Table("User")]
    [Serializable]
    public partial class User : Entity
    {
        #region Model
        private int _Id;
        private string _UserName;
        private string _PassWord;
        private string _Company;
        private string _LinkInfo;
        private int _UserType;
        private DateTime? _DueTime;
        private int _Status;
        private DateTime? _LastAllotTime;

        /// <summary>
        /// 
        /// </summary>
        [Field("Id")]
        public int Id
        {
            get { return _Id; }
            set
            {
                this.OnPropertyValueChange("Id");
                this._Id = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("UserName")]
        public string UserName
        {
            get { return _UserName; }
            set
            {
                this.OnPropertyValueChange("UserName");
                this._UserName = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("PassWord")]
        public string PassWord
        {
            get { return _PassWord; }
            set
            {
                this.OnPropertyValueChange("PassWord");
                this._PassWord = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("Company")]
        public string Company
        {
            get { return _Company; }
            set
            {
                this.OnPropertyValueChange("Company");
                this._Company = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("LinkInfo")]
        public string LinkInfo
        {
            get { return _LinkInfo; }
            set
            {
                this.OnPropertyValueChange("LinkInfo");
                this._LinkInfo = value;
            }
        }
        /// <summary>
        /// 用户类型:0试用 1付费
        /// </summary>
        [Field("UserType")]
        public int UserType
        {
            get { return _UserType; }
            set
            {
                this.OnPropertyValueChange("UserType");
                this._UserType = value;
            }
        }
        /// <summary>
        /// 到期时间
        /// </summary>
        [Field("DueTime")]
        public DateTime? DueTime
        {
            get { return _DueTime; }
            set
            {
                this.OnPropertyValueChange("DueTime");
                this._DueTime = value;
            }
        }
        /// <summary>
        /// 软件状态:0离线 1运行
        /// </summary>
        [Field("Status")]
        public int Status
        {
            get { return _Status; }
            set
            {
                this.OnPropertyValueChange("Status");
                this._Status = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("LastAllotTime")]
        public DateTime? LastAllotTime
        {
            get { return _LastAllotTime; }
            set
            {
                this.OnPropertyValueChange("LastAllotTime");
                this._LastAllotTime = value;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 获取实体中的主键列
        /// </summary>
        public override Field[] GetPrimaryKeyFields()
        {
            return new Field[] {
                _.Id,
            };
        }
        /// <summary>
        /// 获取实体中的标识列
        /// </summary>
        public override Field GetIdentityField()
        {
            return _.Id;
        }
        /// <summary>
        /// 获取列信息
        /// </summary>
        public override Field[] GetFields()
        {
            return new Field[] {
                _.Id,
                _.UserName,
                _.PassWord,
                _.Company,
                _.LinkInfo,
                _.UserType,
                _.DueTime,
                _.Status,
                _.LastAllotTime,
            };
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
                this._Id,
                this._UserName,
                this._PassWord,
                this._Company,
                this._LinkInfo,
                this._UserType,
                this._DueTime,
                this._Status,
                this._LastAllotTime,
            };
        }
        /// <summary>
        /// 是否是v1.10.5.6及以上版本实体。
        /// </summary>
        /// <returns></returns>
        public override bool V1_10_5_6_Plus()
        {
            return true;
        }
        #endregion

        #region _Field
        /// <summary>
        /// 字段信息
        /// </summary>
        public class _
        {
            /// <summary>
            /// * 
            /// </summary>
            public readonly static Field All = new Field("*", "User");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field Id = new Field("Id", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field UserName = new Field("UserName", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field PassWord = new Field("PassWord", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field Company = new Field("Company", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field LinkInfo = new Field("LinkInfo", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field UserType = new Field("UserType", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field DueTime = new Field("DueTime", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field Status = new Field("Status", "User", "");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field LastAllotTime = new Field("LastAllotTime", "User", "");
        }
        #endregion
    }

    public class UserResult
    {
        public bool Result { get; set; }

        public string Message { get; set; }

        public User Data { get; set; }
    }

    public class LinkResult
    {
        public int Result { get; set; }
        public bool Reload { get; set; }

        public string Message { get; set; }

        public Data Data { get; set; }
    }

    public class Data
    {
        public bool IsAdmin { get; set; }

        public List<Sale> SaleList { get; set; }
    }

    public class Sale
    {
        public string Name { get; set; }
        
        public string Phone { get; set; }

        public string TelPhone { get; set; }

        public string Sex { get; set; }

        public string RoleName { get; set; }

        public string CompanyString { get; set; }
    }
}
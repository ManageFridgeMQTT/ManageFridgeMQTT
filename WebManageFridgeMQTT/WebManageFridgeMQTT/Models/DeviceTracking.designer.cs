﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebManageFridgeMQTT.Models
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="DF_RELEASE")]
	public partial class DeviceTrackingDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertUser(User instance);
    partial void UpdateUser(User instance);
    partial void DeleteUser(User instance);
    #endregion
		
		public DeviceTrackingDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["DF_RELEASEConnectionString"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DeviceTrackingDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DeviceTrackingDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DeviceTrackingDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DeviceTrackingDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.GetInfoDeviceModify")]
		public ISingleResult<GetInfoDeviceModifyResult> GetInfoDeviceModify([global::System.Data.Linq.Mapping.ParameterAttribute(DbType="NVarChar(MAX)")] string strThietBiID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="FromDate", DbType="DateTime")] System.Nullable<System.DateTime> fromDate, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="ToDate", DbType="DateTime")] System.Nullable<System.DateTime> toDate)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), strThietBiID, fromDate, toDate);
			return ((ISingleResult<GetInfoDeviceModifyResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.GetInfoDeviceMove")]
		public ISingleResult<GetInfoDeviceMoveResult> GetInfoDeviceMove([global::System.Data.Linq.Mapping.ParameterAttribute(DbType="NVarChar(MAX)")] string strThietBiID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="FromDate", DbType="DateTime")] System.Nullable<System.DateTime> fromDate, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="ToDate", DbType="DateTime")] System.Nullable<System.DateTime> toDate)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), strThietBiID, fromDate, toDate);
			return ((ISingleResult<GetInfoDeviceMoveResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.GetInfoDeviceActivity")]
		public ISingleResult<GetInfoDeviceActivityResult> GetInfoDeviceActivity([global::System.Data.Linq.Mapping.ParameterAttribute(DbType="NVarChar(MAX)")] string strThietBiID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="FromDate", DbType="DateTime")] System.Nullable<System.DateTime> fromDate, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="ToDate", DbType="DateTime")] System.Nullable<System.DateTime> toDate)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), strThietBiID, fromDate, toDate);
			return ((ISingleResult<GetInfoDeviceActivityResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.Sp_GetInfoDevice")]
		public ISingleResult<Sp_GetInfoDeviceResult> Sp_GetInfoDevice()
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())));
			return ((ISingleResult<Sp_GetInfoDeviceResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateThieBiSatusMess")]
		public int UpdateThieBiSatusMess([global::System.Data.Linq.Mapping.ParameterAttribute(DbType="NVarChar(MAX)")] string strThietBiID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="CommandType", DbType="NVarChar(200)")] string commandType, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="CommandId", DbType="NVarChar(200)")] string commandId, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="CommandAction", DbType="NVarChar(200)")] string commandAction, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Loai", DbType="Int")] System.Nullable<int> loai, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="StatusMay", DbType="Int")] System.Nullable<int> statusMay, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Time", DbType="Float")] System.Nullable<double> time, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="TrangThaiHienTai", DbType="Int")] System.Nullable<int> trangThaiHienTai, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="LatitudeHienTai", DbType="Decimal(18,10)")] System.Nullable<decimal> latitudeHienTai, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="LongtitudeHienTai", DbType="Decimal(18,10)")] System.Nullable<decimal> longtitudeHienTai)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), strThietBiID, commandType, commandId, commandAction, loai, statusMay, time, trangThaiHienTai, latitudeHienTai, longtitudeHienTai);
			return ((int)(result.ReturnValue));
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.[User]")]
	public partial class User : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _UserId;
		
		private string _UserName;
		
		private string _Password;
		
		private System.Nullable<System.Guid> _NhanVienId;
		
		private bool _IsActive;
		
		private System.DateTime _LastSync;
		
		private string _PassLock;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnUserIdChanging(System.Guid value);
    partial void OnUserIdChanged();
    partial void OnUserNameChanging(string value);
    partial void OnUserNameChanged();
    partial void OnPasswordChanging(string value);
    partial void OnPasswordChanged();
    partial void OnNhanVienIdChanging(System.Nullable<System.Guid> value);
    partial void OnNhanVienIdChanged();
    partial void OnIsActiveChanging(bool value);
    partial void OnIsActiveChanged();
    partial void OnLastSyncChanging(System.DateTime value);
    partial void OnLastSyncChanged();
    partial void OnPassLockChanging(string value);
    partial void OnPassLockChanged();
    #endregion
		
		public User()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UserId", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid UserId
		{
			get
			{
				return this._UserId;
			}
			set
			{
				if ((this._UserId != value))
				{
					this.OnUserIdChanging(value);
					this.SendPropertyChanging();
					this._UserId = value;
					this.SendPropertyChanged("UserId");
					this.OnUserIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UserName", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string UserName
		{
			get
			{
				return this._UserName;
			}
			set
			{
				if ((this._UserName != value))
				{
					this.OnUserNameChanging(value);
					this.SendPropertyChanging();
					this._UserName = value;
					this.SendPropertyChanged("UserName");
					this.OnUserNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Password", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string Password
		{
			get
			{
				return this._Password;
			}
			set
			{
				if ((this._Password != value))
				{
					this.OnPasswordChanging(value);
					this.SendPropertyChanging();
					this._Password = value;
					this.SendPropertyChanged("Password");
					this.OnPasswordChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NhanVienId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> NhanVienId
		{
			get
			{
				return this._NhanVienId;
			}
			set
			{
				if ((this._NhanVienId != value))
				{
					this.OnNhanVienIdChanging(value);
					this.SendPropertyChanging();
					this._NhanVienId = value;
					this.SendPropertyChanged("NhanVienId");
					this.OnNhanVienIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsActive", DbType="Bit NOT NULL")]
		public bool IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				if ((this._IsActive != value))
				{
					this.OnIsActiveChanging(value);
					this.SendPropertyChanging();
					this._IsActive = value;
					this.SendPropertyChanged("IsActive");
					this.OnIsActiveChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastSync", DbType="DateTime NOT NULL")]
		public System.DateTime LastSync
		{
			get
			{
				return this._LastSync;
			}
			set
			{
				if ((this._LastSync != value))
				{
					this.OnLastSyncChanging(value);
					this.SendPropertyChanging();
					this._LastSync = value;
					this.SendPropertyChanged("LastSync");
					this.OnLastSyncChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PassLock", DbType="NVarChar(20)")]
		public string PassLock
		{
			get
			{
				return this._PassLock;
			}
			set
			{
				if ((this._PassLock != value))
				{
					this.OnPassLockChanging(value);
					this.SendPropertyChanging();
					this._PassLock = value;
					this.SendPropertyChanged("PassLock");
					this.OnPassLockChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	public partial class GetInfoDeviceModifyResult
	{
		
		private System.Guid _SuaChuaThietBiID;
		
		private System.Guid _ThietBiID;
		
		private int _TrangThai;
		
		private System.DateTime _NgayHong;
		
		private System.Nullable<System.DateTime> _NgaySua;
		
		private System.Nullable<System.DateTime> _NgayHoanThien;
		
		private string _MoTa;
		
		private string _DonViSua;
		
		private System.Nullable<double> _ChiPhi;
		
		private System.Nullable<System.Guid> _NguoiKhaiBao;
		
		private System.Nullable<System.Guid> _NguoiBanGiao;
		
		private bool _IsActive;
		
		public GetInfoDeviceModifyResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SuaChuaThietBiID", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid SuaChuaThietBiID
		{
			get
			{
				return this._SuaChuaThietBiID;
			}
			set
			{
				if ((this._SuaChuaThietBiID != value))
				{
					this._SuaChuaThietBiID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThietBiID", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ThietBiID
		{
			get
			{
				return this._ThietBiID;
			}
			set
			{
				if ((this._ThietBiID != value))
				{
					this._ThietBiID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TrangThai", DbType="Int NOT NULL")]
		public int TrangThai
		{
			get
			{
				return this._TrangThai;
			}
			set
			{
				if ((this._TrangThai != value))
				{
					this._TrangThai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NgayHong", DbType="DateTime NOT NULL")]
		public System.DateTime NgayHong
		{
			get
			{
				return this._NgayHong;
			}
			set
			{
				if ((this._NgayHong != value))
				{
					this._NgayHong = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NgaySua", DbType="DateTime")]
		public System.Nullable<System.DateTime> NgaySua
		{
			get
			{
				return this._NgaySua;
			}
			set
			{
				if ((this._NgaySua != value))
				{
					this._NgaySua = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NgayHoanThien", DbType="DateTime")]
		public System.Nullable<System.DateTime> NgayHoanThien
		{
			get
			{
				return this._NgayHoanThien;
			}
			set
			{
				if ((this._NgayHoanThien != value))
				{
					this._NgayHoanThien = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MoTa", DbType="NVarChar(500)")]
		public string MoTa
		{
			get
			{
				return this._MoTa;
			}
			set
			{
				if ((this._MoTa != value))
				{
					this._MoTa = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DonViSua", DbType="NVarChar(500)")]
		public string DonViSua
		{
			get
			{
				return this._DonViSua;
			}
			set
			{
				if ((this._DonViSua != value))
				{
					this._DonViSua = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ChiPhi", DbType="Float")]
		public System.Nullable<double> ChiPhi
		{
			get
			{
				return this._ChiPhi;
			}
			set
			{
				if ((this._ChiPhi != value))
				{
					this._ChiPhi = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NguoiKhaiBao", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> NguoiKhaiBao
		{
			get
			{
				return this._NguoiKhaiBao;
			}
			set
			{
				if ((this._NguoiKhaiBao != value))
				{
					this._NguoiKhaiBao = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NguoiBanGiao", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> NguoiBanGiao
		{
			get
			{
				return this._NguoiBanGiao;
			}
			set
			{
				if ((this._NguoiBanGiao != value))
				{
					this._NguoiBanGiao = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsActive", DbType="Bit NOT NULL")]
		public bool IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				if ((this._IsActive != value))
				{
					this._IsActive = value;
				}
			}
		}
	}
	
	public partial class GetInfoDeviceMoveResult
	{
		
		private System.DateTime _Ngay;
		
		private string _TuViTri;
		
		private string _DenViTri;
		
		public GetInfoDeviceMoveResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Ngay", DbType="DateTime NOT NULL")]
		public System.DateTime Ngay
		{
			get
			{
				return this._Ngay;
			}
			set
			{
				if ((this._Ngay != value))
				{
					this._Ngay = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TuViTri", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
		public string TuViTri
		{
			get
			{
				return this._TuViTri;
			}
			set
			{
				if ((this._TuViTri != value))
				{
					this._TuViTri = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DenViTri", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
		public string DenViTri
		{
			get
			{
				return this._DenViTri;
			}
			set
			{
				if ((this._DenViTri != value))
				{
					this._DenViTri = value;
				}
			}
		}
	}
	
	public partial class GetInfoDeviceActivityResult
	{
		
		private System.Guid _ThietBiID;
		
		private System.DateTime _ThoiGian;
		
		private System.Nullable<int> _Loai;
		
		private System.Nullable<double> _Time;
		
		private System.Nullable<int> _TrangThai;
		
		private string _StrTrangThai;
		
		public GetInfoDeviceActivityResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThietBiID", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ThietBiID
		{
			get
			{
				return this._ThietBiID;
			}
			set
			{
				if ((this._ThietBiID != value))
				{
					this._ThietBiID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThoiGian", DbType="DateTime NOT NULL")]
		public System.DateTime ThoiGian
		{
			get
			{
				return this._ThoiGian;
			}
			set
			{
				if ((this._ThoiGian != value))
				{
					this._ThoiGian = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Loai", DbType="Int")]
		public System.Nullable<int> Loai
		{
			get
			{
				return this._Loai;
			}
			set
			{
				if ((this._Loai != value))
				{
					this._Loai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Time", DbType="Float")]
		public System.Nullable<double> Time
		{
			get
			{
				return this._Time;
			}
			set
			{
				if ((this._Time != value))
				{
					this._Time = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TrangThai", DbType="Int")]
		public System.Nullable<int> TrangThai
		{
			get
			{
				return this._TrangThai;
			}
			set
			{
				if ((this._TrangThai != value))
				{
					this._TrangThai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StrTrangThai", DbType="NVarChar(17) NOT NULL", CanBeNull=false)]
		public string StrTrangThai
		{
			get
			{
				return this._StrTrangThai;
			}
			set
			{
				if ((this._StrTrangThai != value))
				{
					this._StrTrangThai = value;
				}
			}
		}
	}
	
	public partial class Sp_GetInfoDeviceResult
	{
		
		private System.Nullable<long> _STT;
		
		private System.Guid _ThietBiId;
		
		private string _TenCongTrinh;
		
		private string _DiaDiem;
		
		private string _TenVietTat;
		
		private string _TenThietBi;
		
		private System.Nullable<int> _TrangThai;
		
		private string _DaHoatDong;
		
		private string _LoaiHinh;
		
		private System.Nullable<System.DateTime> _ThoiGianVao;
		
		private System.Nullable<int> _NgayOCongTrinh;
		
		private System.Nullable<double> _GiaThue;
		
		private System.Nullable<double> _ThanhTien;
		
		private System.Nullable<double> _ThayDauTiep;
		
		private System.Nullable<double> _ThayLocTiep;
		
		private System.Nullable<int> _TrangThaiHienTai;
		
		private string _StrTrangThai;
		
		private System.Nullable<decimal> _Latitude;
		
		private System.Nullable<decimal> _Longitude;
		
		public Sp_GetInfoDeviceResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_STT", DbType="BigInt")]
		public System.Nullable<long> STT
		{
			get
			{
				return this._STT;
			}
			set
			{
				if ((this._STT != value))
				{
					this._STT = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThietBiId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ThietBiId
		{
			get
			{
				return this._ThietBiId;
			}
			set
			{
				if ((this._ThietBiId != value))
				{
					this._ThietBiId = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TenCongTrinh", DbType="NVarChar(500)")]
		public string TenCongTrinh
		{
			get
			{
				return this._TenCongTrinh;
			}
			set
			{
				if ((this._TenCongTrinh != value))
				{
					this._TenCongTrinh = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DiaDiem", DbType="NVarChar(1000)")]
		public string DiaDiem
		{
			get
			{
				return this._DiaDiem;
			}
			set
			{
				if ((this._DiaDiem != value))
				{
					this._DiaDiem = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TenVietTat", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string TenVietTat
		{
			get
			{
				return this._TenVietTat;
			}
			set
			{
				if ((this._TenVietTat != value))
				{
					this._TenVietTat = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TenThietBi", DbType="NVarChar(250) NOT NULL", CanBeNull=false)]
		public string TenThietBi
		{
			get
			{
				return this._TenThietBi;
			}
			set
			{
				if ((this._TenThietBi != value))
				{
					this._TenThietBi = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TrangThai", DbType="Int")]
		public System.Nullable<int> TrangThai
		{
			get
			{
				return this._TrangThai;
			}
			set
			{
				if ((this._TrangThai != value))
				{
					this._TrangThai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DaHoatDong", DbType="VarChar(30)")]
		public string DaHoatDong
		{
			get
			{
				return this._DaHoatDong;
			}
			set
			{
				if ((this._DaHoatDong != value))
				{
					this._DaHoatDong = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LoaiHinh", DbType="NVarChar(11) NOT NULL", CanBeNull=false)]
		public string LoaiHinh
		{
			get
			{
				return this._LoaiHinh;
			}
			set
			{
				if ((this._LoaiHinh != value))
				{
					this._LoaiHinh = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThoiGianVao", DbType="DateTime")]
		public System.Nullable<System.DateTime> ThoiGianVao
		{
			get
			{
				return this._ThoiGianVao;
			}
			set
			{
				if ((this._ThoiGianVao != value))
				{
					this._ThoiGianVao = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NgayOCongTrinh", DbType="Int")]
		public System.Nullable<int> NgayOCongTrinh
		{
			get
			{
				return this._NgayOCongTrinh;
			}
			set
			{
				if ((this._NgayOCongTrinh != value))
				{
					this._NgayOCongTrinh = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_GiaThue", DbType="Float")]
		public System.Nullable<double> GiaThue
		{
			get
			{
				return this._GiaThue;
			}
			set
			{
				if ((this._GiaThue != value))
				{
					this._GiaThue = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThanhTien", DbType="Float")]
		public System.Nullable<double> ThanhTien
		{
			get
			{
				return this._ThanhTien;
			}
			set
			{
				if ((this._ThanhTien != value))
				{
					this._ThanhTien = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThayDauTiep", DbType="Float")]
		public System.Nullable<double> ThayDauTiep
		{
			get
			{
				return this._ThayDauTiep;
			}
			set
			{
				if ((this._ThayDauTiep != value))
				{
					this._ThayDauTiep = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ThayLocTiep", DbType="Float")]
		public System.Nullable<double> ThayLocTiep
		{
			get
			{
				return this._ThayLocTiep;
			}
			set
			{
				if ((this._ThayLocTiep != value))
				{
					this._ThayLocTiep = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TrangThaiHienTai", DbType="Int")]
		public System.Nullable<int> TrangThaiHienTai
		{
			get
			{
				return this._TrangThaiHienTai;
			}
			set
			{
				if ((this._TrangThaiHienTai != value))
				{
					this._TrangThaiHienTai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StrTrangThai", DbType="NVarChar(17) NOT NULL", CanBeNull=false)]
		public string StrTrangThai
		{
			get
			{
				return this._StrTrangThai;
			}
			set
			{
				if ((this._StrTrangThai != value))
				{
					this._StrTrangThai = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Latitude", DbType="Decimal(18,10)")]
		public System.Nullable<decimal> Latitude
		{
			get
			{
				return this._Latitude;
			}
			set
			{
				if ((this._Latitude != value))
				{
					this._Latitude = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Longitude", DbType="Decimal(18,10)")]
		public System.Nullable<decimal> Longitude
		{
			get
			{
				return this._Longitude;
			}
			set
			{
				if ((this._Longitude != value))
				{
					this._Longitude = value;
				}
			}
		}
	}
}
#pragma warning restore 1591

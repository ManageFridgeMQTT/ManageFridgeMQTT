using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace sis.lib.ini
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property )]
	public class IniAttribute : Attribute
	{
		public IniAttribute() { }

		public string Section { get; set; }
		public string Key { get; set; }
		public object Default { get; set; }
	}

	public class IniHelper
	{
		public static void WriteString( string section, string key, string value, string fileName )
		{
			WritePrivateProfileString( section, key, value, fileName );
		}
		public static string ReadString( string section, string key, string defaultValue, string fileName )
		{
			StringBuilder temp = new StringBuilder( 255 );
			int i = GetPrivateProfileString( section, key, defaultValue, temp, 255, fileName );
			return temp.ToString();
		}

		private static BindingFlags _bindingFlags =
			  BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.Public
			| BindingFlags.NonPublic
			;
		public static void Load( object instance, string fileName )
		{
			Load( instance, null, fileName );
		}
		public static void Save( object instance, string fileName )
		{
			Save( instance, null, fileName );
		}
		public static void Load( object instance, string memberName, string fileName )
		{
			Type theType = instance.GetType();
			IniAttribute instanceIniAttribute = (IniAttribute)theType.GetCustomAttributes( typeof( IniAttribute ), false ).SingleOrDefault();
			string defaultSection = ( instanceIniAttribute == null || string.IsNullOrWhiteSpace( instanceIniAttribute.Section ) ) ? theType.Name : instanceIniAttribute.Section;

			MemberInfo[] members = string.IsNullOrWhiteSpace( memberName ) ? theType.GetMembers( _bindingFlags ) : theType.GetMember( memberName, _bindingFlags );
			foreach ( var member in members )
			{
				IniAttribute memberIniAttribute = (IniAttribute)member.GetCustomAttributes( typeof( IniAttribute ), false ).SingleOrDefault();
				if ( memberIniAttribute == null )
					continue;

				string section = string.IsNullOrWhiteSpace( memberIniAttribute.Section ) ? defaultSection : memberIniAttribute.Section;
				string key = string.IsNullOrWhiteSpace( memberIniAttribute.Key ) ? member.Name : memberIniAttribute.Key;
				string defaultValue = memberIniAttribute.Default == null ? string.Empty : memberIniAttribute.Default.ToString();
				string value = ReadString( section, key, defaultValue, fileName );

				#region member is property
				if ( member.MemberType == MemberTypes.Property )
				{
					PropertyInfo property = member as PropertyInfo;
					if ( property == null )
						continue;

					if ( IsClassType( property.PropertyType ) )
					{
						object nestedInstance = property.GetValue( instance, null );
						if ( nestedInstance != null )
							Load( nestedInstance, fileName );

						continue;
					}

					SetPrimitivePropertyValue( instance, property, value );
					continue;
				}
				#endregion member is property

				#region member is field
				if ( member.MemberType == MemberTypes.Field )
				{
					FieldInfo field = member as FieldInfo;
					if ( field == null )
						continue;

					if ( IsClassType( field.FieldType ) )
					{
						object nestedInstance = field.GetValue( instance );
						if ( nestedInstance != null )
							Load( nestedInstance, fileName );

						continue;
					}

					SetPrimitiveFieldValue( instance, field, value );
					continue;
				}
				#endregion member is field

			}
		}
		public static void Save( object instance, string memberName, string fileName )
		{
			Type theType = instance.GetType();
			IniAttribute instanceIniAttribute = (IniAttribute)theType.GetCustomAttributes( typeof( IniAttribute ), false ).SingleOrDefault();
			string defaultSection = ( instanceIniAttribute == null || string.IsNullOrWhiteSpace( instanceIniAttribute.Section ) ) ? theType.Name : instanceIniAttribute.Section;

			MemberInfo[] members = string.IsNullOrWhiteSpace( memberName ) ? theType.GetMembers( _bindingFlags ) : theType.GetMember( memberName, _bindingFlags );
			foreach ( var member in members )
			{
				IniAttribute memberIniAttribute = (IniAttribute)member.GetCustomAttributes( typeof( IniAttribute ), false ).SingleOrDefault();
				if ( memberIniAttribute == null )
					continue;

				string section = string.IsNullOrWhiteSpace( memberIniAttribute.Section ) ? defaultSection : memberIniAttribute.Section;
				string key = string.IsNullOrWhiteSpace( memberIniAttribute.Key ) ? member.Name : memberIniAttribute.Key;

				#region member is propperty
				if ( member.MemberType == MemberTypes.Property )
				{
					PropertyInfo property = member as PropertyInfo;
					if ( property == null )
						continue;

					if ( !property.CanRead )
						continue;

					if ( IsClassType( property.PropertyType ) )
					{
						object nestedInstance = property.GetValue( instance, null );
						if ( nestedInstance != null )
							Save( nestedInstance, fileName );

						continue;
					}

					string value = ConvertToString( property.GetValue( instance, null ), property.PropertyType );
					WriteString( section, key, value, fileName );

					continue;
				}
				#endregion member is propperty

				#region member is field
				if ( member.MemberType == MemberTypes.Field )
				{
					FieldInfo field = member as FieldInfo;
					if ( field == null )
						continue;

					if ( IsClassType( field.FieldType ) )
					{
						object nestedInstance = field.GetValue( instance );
						if ( nestedInstance != null )
							Save( nestedInstance, fileName );

						continue;
					}

					string value = ConvertToString( field.GetValue( instance ), field.FieldType );
					WriteString( section, key, value, fileName );

					continue;
				}
				#endregion member is field

			}
		}
		#region private
		private static bool IsClassType( Type type )
		{
			return !type.IsPrimitive
				&& !type.IsEnum
				&& type != typeof( Boolean )
				&& type != typeof( String )
				&& type != typeof( Decimal )
				&& type != typeof( Double )
				&& type != typeof( DateTime )
				&& type != typeof( DateTime? )
				;
		}
		private static void SetPrimitivePropertyValue( object instance, PropertyInfo property, string value )
		{
			if ( !property.CanWrite )
				return;

			if ( property.PropertyType == typeof( string ) )
			{
				property.SetValue( instance, value, null );
				return;
			}

			object[] tryParseParameters;
			MethodInfo tryParse = GetTryParseMethod( property.PropertyType, value, out tryParseParameters );
			if ( tryParse == null )
				return;

			if ( (bool)tryParse.Invoke( null, tryParseParameters ) )
				property.SetValue( instance, tryParseParameters.Last(), null );
			else if ( property.PropertyType.IsValueType && Nullable.GetUnderlyingType( property.PropertyType ) == null )
				//property.SetValue( instance, Activator.CreateInstance( t ), null );
				property.SetValue( instance, tryParseParameters.Last(), null );
			else
				property.SetValue( instance, null, null );
		}
		private static void SetPrimitiveFieldValue( object instance, FieldInfo field, string value )
		{
			if ( field.FieldType == typeof( string ) )
			{
				field.SetValue( instance, value );
				return;
			}

			object[] tryParseParameters;
			MethodInfo tryParse = GetTryParseMethod( field.FieldType, value, out tryParseParameters );
			if ( tryParse == null )
				return;

			if ( (bool)tryParse.Invoke( null, tryParseParameters ) )
				field.SetValue( instance, tryParseParameters.Last() );
			else if ( field.FieldType.IsValueType && Nullable.GetUnderlyingType( field.FieldType ) == null )
				//field.SetValue( instance, Activator.CreateInstance( t ) );
				field.SetValue( instance, tryParseParameters.Last() );
			else
				field.SetValue( instance, null );

		}
		private static MethodInfo GetTryParseMethod( Type ownerType, string value, out object[] tryParseParameters )
		{
			if ( ownerType.IsEnum )
			{

				MethodInfo enumTryParse = typeof( Enum ).GetMethods( BindingFlags.Public | BindingFlags.Static )
					.Where( m => m.Name == "TryParse" && m.GetParameters().Length == 3 )
					.First();

				if ( enumTryParse != null )
					enumTryParse = enumTryParse.MakeGenericMethod( ownerType );

				tryParseParameters = new object[] { value, true, null };
				return enumTryParse;
			}

			Type tryParseClassType;
			string tryParseMethodName;
			Type[] tryParseParametersType;
			if ( ownerType == typeof( DateTime ) || ownerType == typeof( DateTime? ) )
			{
				tryParseClassType = typeof( DateTime );
				tryParseMethodName = "TryParseExact";
				tryParseParametersType = new Type[] { typeof( string ), typeof( string ), typeof( IFormatProvider ), typeof( DateTimeStyles ), typeof( DateTime ).MakeByRefType() };
				tryParseParameters = new object[] { value, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, null };
			}
			else
			{
				tryParseClassType = Nullable.GetUnderlyingType( ownerType ) ?? ownerType;
				tryParseMethodName = "TryParse";
				tryParseParametersType = new Type[] { typeof( string ), tryParseClassType.MakeByRefType() };
				tryParseParameters = new object[] { value, null };
			}

			MethodInfo tryParse = tryParseClassType.GetMethod( tryParseMethodName, BindingFlags.Static | BindingFlags.Public, null, tryParseParametersType, null );

			return tryParse;
		}
		private static string ConvertToString( object value, Type objectType )
		{
			if ( value == null )
				return string.Empty;

			if ( objectType == typeof( DateTime ) || objectType == typeof( DateTime? ) )
				return ( (DateTime)value ).ToString( "s" );

			return value.ToString();
		}
		#endregion private
		#region kernel32.dll
		[DllImport( "kernel32" )]
		private static extern long WritePrivateProfileString( string section, string key, string val, string filePath );
		[DllImport( "kernel32" )]
		private static extern int GetPrivateProfileString( string section, string key, string def, StringBuilder retVal, int size, string filePath );
		#endregion kernel32.dll
	}
}

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public static class ReflectionUtils { 
	public static bool	TypeContainsAttributes( Type type, params Type[] attributes ) {
		Attribute[] typeAttribs = type.GetCustomAttributes( true ) as Attribute[];
		foreach( Attribute typeAttrib in typeAttribs ) {
			foreach( Type argAttrib in attributes ) {
				if( typeAttrib.GetType() == argAttrib ) {
					return true;
				}
			}
		}

		return false;
	}

	public static Type FindType(string typeName)
	{
		return Type.GetType(typeName);
	}

	/*public static Type	FindType( string typeName ) {
		if( string.IsNullOrEmpty(typeName) ) {
			return null;
		}

		foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach(Type t in a.GetTypes())
			{
				if(t.Name == typeName)
				{
					return t;
				}
			}
		}

		return null;
	}*/
	
	//Returns true if a is a child of or is the same as b
	public static bool CompareType(Type a, Type b)
	{
		bool subclass = a.IsSubclassOf(b);
		bool same = (a == b);
		bool assignable = b.IsAssignableFrom(a); //for interface checks
		return subclass || same || assignable;	
	}
	
	public static Dictionary<string, System.Type> GetChildTypes<T>() 
	{
		return GetChildTypes(typeof(T));
	}
	
	public static Dictionary<string, System.Type> GetChildTypes(System.Type baseType)
	{
		List<Type> types = Assembly.GetAssembly(baseType).GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
		Dictionary<string, System.Type> output = new Dictionary<string, System.Type>();
		foreach(Type child in types)
		{
			output.Add(child.ToString(), child);
		}
		return output;
	}

	public static MethodInfo	FindMethodInfo( Type objType, string methodName, Type[] typeArray, BindingFlags bindingFlags ) {
		return objType.GetMethod( methodName, bindingFlags, null, typeArray, null );
	}

	public static MethodInfo	FindMethodInfo( Type objType, string methodName, Type[] typeArray ) {
		return FindMethodInfo( objType, methodName, typeArray, BindingFlags.Instance|BindingFlags.Public );
	}

	public static MethodInfo	FindMethodInfo( Type objType, string methodName, BindingFlags bindingFlags ) {
		return objType.GetMethod( methodName, bindingFlags );
	}

	public static MethodInfo	FindMethodInfo( Type objType, string methodName ) {
		return FindMethodInfo( objType, methodName, BindingFlags.Instance|BindingFlags.Public );
	}

	public static string	BuildMethodInfoError( string methodName, Type[] typeArray ) {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		if( typeArray.Length > 0 ) {
			for( int ix = 0; ix < typeArray.Length - 1; ++ix ) {
				builder.AppendFormat( "{0}, ", typeArray[ix] );
			}
			builder.Append( typeArray[typeArray.Length - 1] );
		}
		
		return string.Format( "Handler for {0}({1}) not found!", methodName, builder );
	}
}
using System;

public static class ExceptionUtility {
	public static void	Verify<TException>( bool exp ) where TException : Exception, new() {
		Verify<TException>( exp, "" );
	}
	
	public static void	Verify<TException>( bool exp, string message ) where TException : Exception, new() {
		if( exp ) {
			return;
		}
		
		System.Exception e = System.Activator.CreateInstance( typeof(TException), new object[]{message} ) as System.Exception;
		throw e;
	}
}
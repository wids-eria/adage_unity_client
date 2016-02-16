// Messenger.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
//
// Inspired by and based on Rod Hyde's Messenger:
// http://www.unifycommunity.com/wiki/index.php?title=CSharpMessenger
//
// This is a C# messenger (notification center). It uses delegates
// and generics to provide type-checked messaging between event producers and
// event consumers, without the need for producers or consumers to be aware of
// each other. The major improvement from Hyde's implementation is that
// there is more extensive error detection, preventing silent bugs.
//
// Usage example:
// Messenger<float>.AddListener("myEvent", MyEventHandler);
// ...
// Messenger<float>.Broadcast("myEvent", 1.0f);
 
using System;
using System.Collections.Generic;

// Delegates used in Messenger.cs.
public delegate void Callback();
public delegate void Callback<T>(T arg1);
public delegate void Callback<T, U>(T arg1, U arg2);
public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
public delegate void Callback<T, U, V, W>(T arg1, U arg2, V arg3, W arg4);

public static class MessengerGuard {
	//[System.Diagnostics.Conditional("DEBUG")]
	public static void OnEnterMethod() {
		//System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
		//UnityEngine.Debug.Log( stackTrace.GetFrame(1).GetMethod().Name );
		//UnityEngine.Debug.Log( stackTrace );
	}
}
 
public enum MessengerMode {
	DONT_REQUIRE_LISTENER,
	REQUIRE_LISTENER,
}
 
static internal class MessengerInternal {
	static public Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();
	static public readonly MessengerMode DEFAULT_MODE = MessengerMode.DONT_REQUIRE_LISTENER;
 
	static public void OnListenerAdding(string eventType, Delegate listenerBeingAdded) {
		if (!eventTable.ContainsKey(eventType)) {
			eventTable.Add(eventType, null);
		}
 
		Delegate d = eventTable[eventType];
		if( d == null ) {
			return;
		}

		Type dType = d.GetType();
		Type listenerType = listenerBeingAdded.GetType();

		if (d != null && dType != listenerType) {
			UnityEngine.Debug.Log( string.Format("Delegate {0}, Listener: {1}", dType, listenerType) );
			throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
		}
	}
 
	static public bool OnListenerRemoving(string eventType, Delegate listenerBeingRemoved) {
		if( !eventTable.ContainsKey(eventType) ) {
			//throw new ListenerException(string.Format("Attempting to remove listener for type {0} but Messenger doesn't know about this event type.", eventType));
			return false;
		}
			
		Delegate d = eventTable[eventType];
 
		if (d == null) {
			throw new ListenerException(string.Format("Attempting to remove listener with for event type {0} but current listener is null.", eventType));
		} else if (d.GetType() != listenerBeingRemoved.GetType()) {
			throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
		}

		return true;
	}
 
	static public void OnListenerRemoved(string eventType) {
		try {
			if( eventTable[eventType] == null ) {
				eventTable.Remove(eventType);
			}
		}
		catch( KeyNotFoundException ) {
		}
	}
 
	static public void OnBroadcasting(string eventType, MessengerMode mode) {
		if (mode == MessengerMode.REQUIRE_LISTENER && !eventTable.ContainsKey(eventType)) {
			throw new MessengerInternal.BroadcastException(string.Format("Broadcasting message {0} but no listener found.", eventType));
		}
	}
 
	static public BroadcastException CreateBroadcastSignatureException(string eventType) {
		return new BroadcastException(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", eventType));
	}
 
	public class BroadcastException : Exception {
		public BroadcastException(string msg)
			: base(msg) {
		}
	}
 
	public class ListenerException : Exception {
		public ListenerException(string msg)
			: base(msg) {
		}
	}
}

// No parameters
static public class Messenger {
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;
 
	static public void AddListener(string eventType, Callback handler) {
		MessengerInternal.OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback)eventTable[eventType] + handler;
	}
 
	static public void RemoveListener(string eventType, Callback handler) {
		if( MessengerInternal.OnListenerRemoving(eventType, handler) ) {	
			eventTable[eventType] = (Callback)eventTable[eventType] - handler;
		}
		
		MessengerInternal.OnListenerRemoved(eventType);
	}
 
	static public void Broadcast(string eventType) {
		Broadcast(eventType, MessengerInternal.DEFAULT_MODE);
	}
 
	static public void Broadcast(string eventType, MessengerMode mode) {
		MessengerInternal.OnBroadcasting(eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue(eventType, out d)) {
			Callback callback = d as Callback;
			if (callback != null) {
				callback();
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException(eventType);
			}
		}
	}
	
	static public bool	IsListeningTo( string eventType, Callback handler ) {
		Delegate del;
		
		if( !eventTable.TryGetValue(eventType, out del) ) {
			return false;
		}
		
		return (Callback)del == handler;
	}
}
 
// One parameter
static public class Messenger<T> {
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;
 
	static public void AddListener(string eventType, Callback<T> handler) {
		MessengerInternal.OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
	}
 
	static public void RemoveListener(string eventType, Callback<T> handler) {
		if( !MessengerInternal.OnListenerRemoving(eventType, handler) ) {
			return;
		}

		eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
		MessengerInternal.OnListenerRemoved(eventType);
	}
 
	static public void Broadcast(string eventType, T arg1) {
		Broadcast(eventType, arg1, MessengerInternal.DEFAULT_MODE);
	}
 
	static public void Broadcast(string eventType, T arg1, MessengerMode mode) {
		MessengerInternal.OnBroadcasting(eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue(eventType, out d)) {
			Callback<T> callback = d as Callback<T>;
			if (callback != null) {
				callback(arg1);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException(eventType);
			}
		}
	}
	
	static public bool	IsListeningTo( string eventType, Callback<T> handler ) {
		Delegate del;
		
		if( !eventTable.TryGetValue(eventType, out del) ) {
			return false;
		}
		
		return (Callback<T>)del == handler;
	}
}
 
 
// Two parameters
static public class Messenger<T, U> {
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;
 
	static public void AddListener(string eventType, Callback<T, U> handler) {
		MessengerInternal.OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U>)eventTable[eventType] + handler;
	}
 
	static public void RemoveListener(string eventType, Callback<T, U> handler) {
		if( MessengerInternal.OnListenerRemoving(eventType, handler) ) {
			eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
		}
		MessengerInternal.OnListenerRemoved(eventType);
	}
 
	static public void Broadcast(string eventType, T arg1, U arg2) {
		Broadcast(eventType, arg1, arg2, MessengerInternal.DEFAULT_MODE);
	}
 
	static public void Broadcast(string eventType, T arg1, U arg2, MessengerMode mode) {
		MessengerInternal.OnBroadcasting(eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue(eventType, out d)) {
			Callback<T, U> callback = d as Callback<T, U>;
			if (callback != null) {
				callback(arg1, arg2);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException(eventType);
			}
		}
	}
	
	static public bool	IsListeningTo( string eventType, Callback<T, U> handler ) {
		Delegate del;
		
		if( !eventTable.TryGetValue(eventType, out del) ) {
			return false;
		}
		
		return (Callback<T, U>)del == handler;
	}
}
 
 
// Three parameters
static public class Messenger<T, U, V> {
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;
 
	static public void AddListener(string eventType, Callback<T, U, V> handler) {
		MessengerInternal.OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] + handler;
	}
 
	static public void RemoveListener(string eventType, Callback<T, U, V> handler) {
		if( MessengerInternal.OnListenerRemoving(eventType, handler) ) {
			eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
		}
		MessengerInternal.OnListenerRemoved(eventType);
	}
 
	static public void Broadcast(string eventType, T arg1, U arg2, V arg3) {
		Broadcast(eventType, arg1, arg2, arg3, MessengerInternal.DEFAULT_MODE);
	}
 
	static public void Broadcast(string eventType, T arg1, U arg2, V arg3, MessengerMode mode) {
		MessengerInternal.OnBroadcasting(eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue(eventType, out d)) {
			Callback<T, U, V> callback = d as Callback<T, U, V>;
			if (callback != null) {
				callback(arg1, arg2, arg3);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException(eventType);
			}
		}
	}
	
	static public bool	IsListeningTo( string eventType, Callback<T, U, V> handler ) {
		Delegate del;
		
		if( !eventTable.TryGetValue(eventType, out del) ) {
			return false;
		}
		
		return (Callback<T, U, V>)del == handler;
	}
}

// Three parameters
static public class Messenger<T, U, V, W> {
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;
	
	static public void AddListener(string eventType, Callback<T, U, V, W> handler) {
		MessengerInternal.OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] + handler;
	}
	
	static public void RemoveListener(string eventType, Callback<T, U, V, W> handler) {
		if( MessengerInternal.OnListenerRemoving(eventType, handler) ) {
			eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] - handler;
		}
		MessengerInternal.OnListenerRemoved(eventType);
	}
	
	static public void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4) {
		Broadcast(eventType, arg1, arg2, arg3, arg4, MessengerInternal.DEFAULT_MODE);
	}
	
	static public void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, MessengerMode mode) {
		MessengerInternal.OnBroadcasting(eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue(eventType, out d)) {
			Callback<T, U, V, W> callback = d as Callback<T, U, V, W>;
			if (callback != null) {
				callback(arg1, arg2, arg3, arg4);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException(eventType);
			}
		}
	}
	
	static public bool	IsListeningTo( string eventType, Callback<T, U, V, W> handler ) {
		Delegate del;
		
		if( !eventTable.TryGetValue(eventType, out del) ) {
			return false;
		}
		
		return (Callback<T, U, V, W>)del == handler;
	}
}
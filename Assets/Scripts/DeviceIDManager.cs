using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public static class DeviceIDManager {
	// TODO: Uncomment for IOS
	// TODO: Comment out for non-IOS builds
	/*
	[DllImport("__Internal")]
	static extern string _Get_Device_id();
	*/

	// Use this for initialization
	public static string GetDeviceID () {
		// TODO: Uncomment for IOS
		// TODO: comment out for non-IOS builds
		/*
		return _Get_Device_id();
		*/
		
		return SystemInfo.deviceUniqueIdentifier;

	}
}

using UnityEngine;
using System.Collections;
using System;

public class ADAGEDeviceInfo
{
    public string device_model = ""; //The model of the device (Read Only).
    public string device_type = ""; //Returns the kind of device the application is running on.
    public string device_unique_identifier = ""; //A unique device identifier. It is guaranteed to be unique for every device (Read Only).

    public int graphics_device_id = -1; //The identifier code of the graphics device (Read Only).
    public string graphics_device_name = ""; //The name of the graphics device (Read Only).
    public string graphics_device_vendor = ""; //The vendor of the graphics device (Read Only).
    public int graphics_device_vendor_id = -1; //The identifier code of the graphics device vendor (Read Only).
    public string graphics_device_version = ""; //The graphics API version supported by the graphics device (Read Only).
    public int graphics_memory_size = 0; //Amount of video memory present (Read Only).
    public int graphics_pixel_fillrate = -1; //Approximate pixel fill-rate of the graphics device (Read Only).
    public int graphics_shader_level = -1; //Graphics device shader capability level (Read Only).

    public string operating_system = ""; //Operating system name with version (Read Only).
    public int processor_count = -1; //Number of processors present (Read Only).
    public string processor_type = ""; //Processor name (Read Only).
    public int system_memory_size = -1; //Amount of system memory present (Read Only).

    public bool supports_accelerometer = false; //Is an accelerometer available on the device?
    public bool supports_compute_shaders = false; //Are compute shaders supported? (Read Only)
    public bool supports_image_effects = false; //Are image effects supported? (Read Only)
    public bool supports_instancing = false; //Is GPU draw call instancing supported? (Read Only)
    public bool supports_location_service = false; //Is the device capable of reporting its location?
    public bool supports_render_textures = false; //Are render textures supported? (Read Only)
    public bool supports_shadows = false; //Are built-in shadows supported? (Read Only)
    public int supports_stencil = -1; //Is the stencil buffer supported? (Read Only)
    public bool supports_vibration = false; //Is the device capable of providing the user haptic feedback by vibration?

    public ADAGEDeviceInfo()
    {
        this.device_model = SystemInfo.deviceModel;
        this.device_type = SystemInfo.deviceType.ToString();

		#if (UNITY_EDITOR)
		this.device_unique_identifier = "Editor Mode - No Device Info Found";
		#else
		this.device_unique_identifier = SystemInfo.deviceUniqueIdentifier;
		#endif

        this.graphics_device_id = SystemInfo.graphicsDeviceID;
        this.graphics_device_name = SystemInfo.graphicsDeviceName;
        this.graphics_device_vendor = SystemInfo.graphicsDeviceVendor;
        this.graphics_device_vendor_id = SystemInfo.graphicsDeviceVendorID;
        this.graphics_device_version = SystemInfo.graphicsDeviceVersion;
        this.graphics_memory_size = SystemInfo.graphicsMemorySize;
        this.graphics_pixel_fillrate = SystemInfo.graphicsPixelFillrate;
        this.graphics_shader_level = SystemInfo.graphicsShaderLevel;

        this.operating_system = SystemInfo.operatingSystem;
        this.processor_count = SystemInfo.processorCount;
        this.processor_type = SystemInfo.processorType;
        this.system_memory_size = SystemInfo.systemMemorySize;

        this.supports_accelerometer = SystemInfo.supportsAccelerometer;
        this.supports_compute_shaders = SystemInfo.supportsComputeShaders;
        this.supports_image_effects = SystemInfo.supportsImageEffects;
        this.supports_instancing = SystemInfo.supportsInstancing;
        this.supports_location_service = SystemInfo.supportsLocationService;
        this.supports_render_textures = SystemInfo.supportsRenderTextures;
        this.supports_shadows = SystemInfo.supportsShadows;
        this.supports_stencil = SystemInfo.supportsStencil;
        this.supports_vibration = SystemInfo.supportsVibration;
    }
}

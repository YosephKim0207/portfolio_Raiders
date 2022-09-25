﻿using System;

namespace InControl
{
	// Token: 0x020007D7 RID: 2007
	[AutoDiscover]
	public class PlayStation3ShenghicWinProfile : UnityInputDeviceProfile
	{
		// Token: 0x06002B2D RID: 11053 RVA: 0x000D3188 File Offset: 0x000D1388
		public PlayStation3ShenghicWinProfile()
		{
			base.Name = "PlayStation 3 Controller";
			base.Meta = "Shenghic PlayStation 3 Controller on Windows (not MotionInJoy)";
			base.DeviceClass = InputDeviceClass.Controller;
			base.DeviceStyle = InputDeviceStyle.PlayStation3;
			base.IncludePlatforms = new string[] { "Windows" };
			this.JoystickNames = new string[] { "PLAYSTATION(R)3Conteroller" };
			base.ControllerSymbology = GameOptions.ControllerSymbology.PS4;
			base.ButtonMappings = new InputControlMapping[]
			{
				new InputControlMapping
				{
					Handle = "Cross",
					Target = InputControlType.Action1,
					Source = UnityInputDeviceProfile.Button14
				},
				new InputControlMapping
				{
					Handle = "Circle",
					Target = InputControlType.Action2,
					Source = UnityInputDeviceProfile.Button13
				},
				new InputControlMapping
				{
					Handle = "Square",
					Target = InputControlType.Action3,
					Source = UnityInputDeviceProfile.Button15
				},
				new InputControlMapping
				{
					Handle = "Triangle",
					Target = InputControlType.Action4,
					Source = UnityInputDeviceProfile.Button12
				},
				new InputControlMapping
				{
					Handle = "DPad Up",
					Target = InputControlType.DPadUp,
					Source = UnityInputDeviceProfile.Button4
				},
				new InputControlMapping
				{
					Handle = "DPad Down",
					Target = InputControlType.DPadDown,
					Source = UnityInputDeviceProfile.Button6
				},
				new InputControlMapping
				{
					Handle = "DPad Left",
					Target = InputControlType.DPadLeft,
					Source = UnityInputDeviceProfile.Button7
				},
				new InputControlMapping
				{
					Handle = "DPad Right",
					Target = InputControlType.DPadRight,
					Source = UnityInputDeviceProfile.Button5
				},
				new InputControlMapping
				{
					Handle = "Left Bumper",
					Target = InputControlType.LeftBumper,
					Source = UnityInputDeviceProfile.Button10
				},
				new InputControlMapping
				{
					Handle = "Right Bumper",
					Target = InputControlType.RightBumper,
					Source = UnityInputDeviceProfile.Button11
				},
				new InputControlMapping
				{
					Handle = "Left Trigger",
					Target = InputControlType.LeftTrigger,
					Source = UnityInputDeviceProfile.Button8
				},
				new InputControlMapping
				{
					Handle = "Right Trigger",
					Target = InputControlType.RightTrigger,
					Source = UnityInputDeviceProfile.Button9
				},
				new InputControlMapping
				{
					Handle = "Select",
					Target = InputControlType.Select,
					Source = UnityInputDeviceProfile.Button3
				},
				new InputControlMapping
				{
					Handle = "Left Stick Button",
					Target = InputControlType.LeftStickButton,
					Source = UnityInputDeviceProfile.Button1
				},
				new InputControlMapping
				{
					Handle = "Right Stick Button",
					Target = InputControlType.RightStickButton,
					Source = UnityInputDeviceProfile.Button2
				},
				new InputControlMapping
				{
					Handle = "Start",
					Target = InputControlType.Start,
					Source = UnityInputDeviceProfile.Button3
				},
				new InputControlMapping
				{
					Handle = "System",
					Target = InputControlType.System,
					Source = UnityInputDeviceProfile.Button16
				}
			};
			base.AnalogMappings = new InputControlMapping[]
			{
				UnityInputDeviceProfile.LeftStickLeftMapping(UnityInputDeviceProfile.Analog0),
				UnityInputDeviceProfile.LeftStickRightMapping(UnityInputDeviceProfile.Analog0),
				UnityInputDeviceProfile.LeftStickUpMapping(UnityInputDeviceProfile.Analog1),
				UnityInputDeviceProfile.LeftStickDownMapping(UnityInputDeviceProfile.Analog1),
				UnityInputDeviceProfile.RightStickLeftMapping(UnityInputDeviceProfile.Analog2),
				UnityInputDeviceProfile.RightStickRightMapping(UnityInputDeviceProfile.Analog2),
				UnityInputDeviceProfile.RightStickUpMapping(UnityInputDeviceProfile.Analog3),
				UnityInputDeviceProfile.RightStickDownMapping(UnityInputDeviceProfile.Analog3)
			};
		}
	}
}

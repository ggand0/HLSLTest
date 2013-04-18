// http://www.pasteall.org/23882/csharp
// geekさんからもらったHigenekoさんのコード
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HLSLTest
{
    public enum JoystickAxis
    {
        X = 0,
        Y = 1,
        Z = 2,
        R = 3,
        U = 4,
        V = 5,
    }
    public enum JoystickPositionType
    {
        Centered,
        Lever
    }
 
    public class JoystickPositionAssign
    {
        public JoystickAxis Axis { get; set; }
        public bool IsNegate { get; set; }
 
        public JoystickPositionAssign()
        {
 
        }
 
        public JoystickPositionAssign(JoystickAxis axis, bool isNegate/* = false*/)//vs2008では使えない...
        {
            Axis = axis;
            IsNegate = isNegate;
        }
 
        internal float GetValue(ref JoystickState state)
        {
            float value = state.GetPosition(Axis);
            return IsNegate ? -value : value;
        }
    }
    /// <summary>
    /// Joystick to game pad assign information.
    /// </summary>
    public class JoystickAssingn
    {
        public const int NumButtons = 32;
 
        public JoystickPositionAssign LeftThumbstickX { get; set; }
        public JoystickPositionAssign LeftThumbstickY { get; set; }
        public JoystickPositionAssign RightThumbstickX { get; set; }
        public JoystickPositionAssign RightThumbstickY { get; set; }
        public JoystickPositionAssign LeftTrigger { get; set; }
        public JoystickPositionAssign RightTrigger { get; set; }
 
        public IList<Buttons> ButtonMap { get { return buttonMap; } }
 
        private Buttons[] buttonMap;
 
        public JoystickAssingn()
        {
            LeftThumbstickX = new JoystickPositionAssign(JoystickAxis.X, false);
            LeftThumbstickY = new JoystickPositionAssign(JoystickAxis.Y, true);
            RightThumbstickX = new JoystickPositionAssign(JoystickAxis.U, false);
            RightThumbstickY = new JoystickPositionAssign(JoystickAxis.R, true);
            LeftTrigger = new JoystickPositionAssign(JoystickAxis.Z, false);
            RightTrigger = new JoystickPositionAssign(JoystickAxis.Z, true);
 
            buttonMap = new Buttons[NumButtons];
            buttonMap[0] = Buttons.A;
            buttonMap[1] = Buttons.B;
            buttonMap[2] = Buttons.X;
            buttonMap[3] = Buttons.Y;
            buttonMap[4] = Buttons.LeftShoulder;
            buttonMap[5] = Buttons.RightShoulder;
            buttonMap[6] = Buttons.Back;
            buttonMap[7] = Buttons.Start;
            buttonMap[8] = Buttons.LeftStick;
            buttonMap[9] = Buttons.RightStick;
        }
 
        public static readonly JoystickAssingn Default = new JoystickAssingn();
    }
    public class JoystickPositionCalibration
    {
        public JoystickPositionType PositionType { get; set; }
        public float DeadZone { get; set; }
        public bool Negate { get; set; }
 
        public JoystickPositionCalibration()
        {
            PositionType = JoystickPositionType.Centered;
            DeadZone = 0.1f;
        }
 
        internal float ConvertInternal(uint value)
        {
            float result = MathHelper.Clamp((float)value / 65535.0f, 0, 1);
 
            if (PositionType == JoystickPositionType.Centered)
            {
                result = 2.0f * result - 1.0f;
            }
 
            if (Math.Abs(result) < DeadZone)
            {
                result = 0.0f;
            }
 
            return result;
        }
    }
	/// <summary>
	/// Calibration:”目盛り定め”
	/// </summary>
    public class JoystickCalibration
    {
        public const int NumAxies = 6;
 
        public JoystickPositionCalibration PositionX { get { return Positions[(int)JoystickAxis.X]; } }
        public JoystickPositionCalibration PositionY { get { return Positions[(int)JoystickAxis.Y]; } }
        public JoystickPositionCalibration PositionZ { get { return Positions[(int)JoystickAxis.Z]; } }
        public JoystickPositionCalibration PositionR { get { return Positions[(int)JoystickAxis.R]; } }
        public JoystickPositionCalibration PositionU { get { return Positions[(int)JoystickAxis.U]; } }
        public JoystickPositionCalibration PositionV { get { return Positions[(int)JoystickAxis.V]; } }
 
        public IList<JoystickPositionCalibration> Positions { get { return positions; } }
 
        public JoystickCalibration()
        {
            for ( int i = 0; i < NumAxies; ++i )
            {
                positions[i] = new JoystickPositionCalibration();
            }
        }
 
        JoystickPositionCalibration[] positions = new JoystickPositionCalibration[NumAxies];
    }
    public struct JoystickState
    {
        public bool IsConnected { get { return isConnected; } }
        public float PositionX { get { return positionX; } }
        public float PositionY { get { return positionY; } }
        public float PositionZ { get { return positionZ; } }
        public float PositionR { get { return positionR; } }
        public float PositionU { get { return positionU; } }
        public float PositionV { get { return positionV; } }
 
 
        public float GetPosition(JoystickAxis axis)
        {
            switch (axis)
            {
                case JoystickAxis.X: return positionX;
                case JoystickAxis.Y: return positionY;
                case JoystickAxis.Z: return positionZ;
                case JoystickAxis.R: return positionR;
                case JoystickAxis.U: return positionU;
                case JoystickAxis.V: return positionV;
            }
 
            throw new InvalidOperationException("It can't be!!");
        }
        public int Buttons { get { return buttons; } }
		public int buttonNum;
		public bool[] buttonMap;

        public GamePadDPad DPad { get { return dpad; } }
 
        public JoystickState(
            bool isConnected,
            float posX, float posY, float posZ,
            float posR, float posU, float posV,
            int buttons, int buttonNum, bool[] buttonMap, GamePadDPad dPad)
        {
            this.isConnected = isConnected;
            this.positionX = posX;
            this.positionY = posY;
            this.positionZ = posZ;
            this.positionR = posR;
            this.positionU = posU;
            this.positionV = posV;
            this.buttons = buttons;
			this.buttonNum = buttonNum;
			this.buttonMap = buttonMap;
            this.dpad = dPad;
        }
 
        float positionX;
        float positionY;
        float positionZ;
        float positionR;
        float positionU;
        float positionV;
        int buttons;
        GamePadDPad dpad;
 
        bool isConnected;
    }
	
	public static class Calc
	{
		public static int[] DecToBin(int dec, int num)
		{
			int[] bin = new int[num];

			for (int i = 0; i < num; i++) {
				bin[i] = dec % 2;
				dec = dec / 2;
			}

			return bin;
		}
		public static bool[] BinToBoolean(int[] bin)
		{
			bool[] boolean = new bool[bin.Length];

			for (int i = 0; i < bin.Length; i++) {
				if (bin[i] == 1) boolean[i] = true;
				else if (bin[i] == 0) boolean[i] = false;
			}

			return boolean;
		}
	}
    public static class JoystickInput
    {
        public const int NumJoysticks = 16;
        private static JoystickCalibration[] calibrations;
		public static IList<JoystickCalibration> Calibrations
		{
			get { return calibrations; }
		}
		
        static JoystickInput()
        {
            calibrations = new JoystickCalibration[NumJoysticks];
            for (int i = 0; i < calibrations.Length; ++i)
            {
                calibrations[i] = new JoystickCalibration();
            }
        }

        public static JoystickState GetAsJoystickState(int joystickIndex)
        {
            if (joystickIndex < 0 || joystickIndex >= NumJoysticks)
                throw new ArgumentOutOfRangeException("joystickIndex");
 
            JOYINFOEX pji = new JOYINFOEX();
            pji.dwSize = sizeOfJOYINFOEX;
            pji.dwFlags = 255;
			//pji.dwButtons = new int[11];// ここでdwButtonsにボタンマップの状態を入れたいがどうすればよいのか..
			//pji.dwButtonNumber = new int[32];
			/*ようやくわかった dwButtonsは32bitフラグ！　つまりボタンマップ配列に変換する所だけ追加すればいける！*/
            int ret = joyGetPosEx((uint)joystickIndex, ref pji);// ここでpjiに値が入る
 
            // Convert to managed joystick states.
            return new JoystickState(
                ret == 0,
                calibrations[joystickIndex].PositionX.ConvertInternal(pji.dwXpos),
                calibrations[joystickIndex].PositionY.ConvertInternal(pji.dwYpos),
                calibrations[joystickIndex].PositionZ.ConvertInternal(pji.dwZpos),
                calibrations[joystickIndex].PositionR.ConvertInternal(pji.dwRpos),
                calibrations[joystickIndex].PositionU.ConvertInternal(pji.dwUpos),
                calibrations[joystickIndex].PositionV.ConvertInternal(pji.dwVpos),
				/*(int)pji.dwButtons*/(int)pji.dwButtons, pji.dwButtonNumber, Calc.BinToBoolean(Calc.DecToBin(pji.dwButtons, 32)), pji.GetDPad());
        }
        public static GamePadState GetAsGamePadState(int joystickIndex, JoystickAssingn assign/* = null*/)
        {
            JoystickState joyState = GetAsJoystickState(joystickIndex);
 
            if (!joyState.IsConnected)
            {
                return new GamePadState();
            }
 
            if (assign == null)
            {
                assign = JoystickAssingn.Default;
            }
 
            Buttons buttons = (Buttons)0;
 
            for (int i = 0; i < JoystickAssingn.NumButtons; ++i)
            {
                int bit = 1 << i;
                if ((joyState.Buttons & bit) != 0)//joyState.Buttons & bit) != 0)
                {
                    buttons |= assign.ButtonMap[i];
                }
            }
 
            return new GamePadState(
                new GamePadThumbSticks(
                    new Vector2(assign.LeftThumbstickX.GetValue(ref joyState),assign.LeftThumbstickY.GetValue(ref joyState)),
                    new Vector2(assign.RightThumbstickX.GetValue(ref joyState),assign.RightThumbstickY.GetValue(ref joyState))),
                new GamePadTriggers(
                    assign.LeftTrigger.GetValue(ref joyState),
                    assign.RightTrigger.GetValue(ref joyState)),
                new GamePadButtons(buttons), joyState.DPad);
        }
        public static string GetState(int joystickIndex)
        {
            GamePadState padState = GetAsGamePadState(joystickIndex, null);
 
            return String.Format("IsConnected:{0}\nLeftThubSticks:{1}\nRightThumbSticks:{2}\nDPad:{3}:Triggers:{4}\nButtons:{5}",
                padState.IsConnected, padState.ThumbSticks.Left, padState.ThumbSticks.Right,
                padState.DPad,
                padState.Triggers,
                padState.Buttons);
 
        }

        #region P/Invoke
 
        [StructLayout(LayoutKind.Sequential)]
        struct JOYINFOEX
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwXpos;
            public uint dwYpos;
            public uint dwZpos;
            public uint dwRpos;
            public uint dwUpos;
            public uint dwVpos;
            public int dwButtons;
            public int dwButtonNumber;//uint
            public uint dwPOV;
            public uint dwReserved1;
            public uint dwReserved2;
 
			// ボタンはどうしよう...　Triggerが無いPadにも対応させたいけどとりあえず
			// GetDPadを真似てGamePadStateを使わざるを得ないか...
			public GamePadButtons GetButtons()
			{
				return new GamePadButtons();
			}

            public GamePadDPad GetDPad()
            {
                ButtonState u = ButtonState.Released;
                ButtonState r = ButtonState.Released;
                ButtonState d = ButtonState.Released;
                ButtonState l = ButtonState.Released;
 
                switch (dwPOV)
                {
                    case 0:     u = ButtonState.Pressed; break;
                    case 4500:  u = ButtonState.Pressed; r = ButtonState.Pressed; break;
                    case 9000:  r = ButtonState.Pressed; break;
                    case 13500: r = ButtonState.Pressed; d = ButtonState.Pressed; break;
                    case 18000: d = ButtonState.Pressed; break;
                    case 22500: d = ButtonState.Pressed; l = ButtonState.Pressed; break;
                    case 27000: l = ButtonState.Pressed; break;
                    case 31500: l = ButtonState.Pressed; u = ButtonState.Pressed; break;
                }
 
                return new GamePadDPad(u, d, l, r);
            }
        };
 
        static uint sizeOfJOYINFOEX = (uint)Marshal.SizeOf(typeof(JOYINFOEX));
 
        [DllImport("winmm.dll")]
        static extern int joyGetPosEx(uint uJoyId, ref JOYINFOEX pji);
 
        #endregion
    }
}
﻿////////////////////////////////////////////////////////////////////////////
//
//  This file is part of Rpi.SenseHat.Demo
//
//  Copyright (c) 2015, Mattias Larsson
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of 
//  this software and associated documentation files (the "Software"), to deal in 
//  the Software without restriction, including without limitation the rights to use, 
//  copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
//  Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all 
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Windows.UI;
using Emmellsoft.IoT.Rpi.SenseHat;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts.BW;

namespace RPi.SenseHat.Demo.Demos
{
	public class BwScrollText : SenseHatDemo
	{
		private readonly string _scrollText;

		private enum RenderMode
		{
            StaticRainbowOnBlack,
            YellowOnBlue,
			BlackOnStaticRainbow,
			BlackOnMovingRainbow,
			MovingRainbowOnBlack,
		}

		private readonly Color[,] _rainbowColors = new Color[8, 8];
		private RenderMode _currentMode;

		public BwScrollText(ISenseHat senseHat, string scrollText)
			: base(senseHat)
		{
			_scrollText = scrollText;
		}

		public override void Run()
		{
			// Get a copy of the rainbow colors.
			SenseHat.Display.Reset();
			SenseHat.Display.CopyScreenToColors(_rainbowColors);

			// Recreate the font from the serialized bytes.
			BwFont font = BwFont.Deserialize(FontBytes);

			// Get the characters to scroll.
			IEnumerable<BwCharacter> characters = font.GetChars(_scrollText);

			// Create the character renderer.
			BwCharacterRenderer characterRenderer = new BwCharacterRenderer(GetCharacterColor);

			// Create the text scroller.
			TextScroller<BwCharacter> textScroller = new TextScroller<BwCharacter>(SenseHat.Display, characterRenderer, characters);

			while (true)
			{
				// Step the scroller.
				if (!textScroller.Step())
				{
					// Reset the scroller when reaching the end.
					textScroller.Reset();
				}

				// Draw the background.
				FillDisplay(textScroller.ScrollPixelOffset);

				// Draw the scroll text.
				textScroller.Render();

				// Update the physical display.
				SenseHat.Display.Update();

				// Should we stop?
				if (SenseHat.Joystick.Update() && (SenseHat.Joystick.EnterKey == KeyState.Pressing))
				{
					// The middle button is just pressed.
					break;
				}

				// Pause for a short while.
				Sleep(TimeSpan.FromMilliseconds(50));
			}
		}

		private void SwitchToNextScrollMode()
		{
			_currentMode++;
			if (_currentMode > RenderMode.MovingRainbowOnBlack)
			{
				_currentMode = RenderMode.YellowOnBlue;
			}
		}

		private void FillDisplay(int scrollPixelOffset)
		{
			switch (_currentMode)
			{
				case RenderMode.YellowOnBlue:
					SenseHat.Display.Fill(Colors.Blue);
					break;

				case RenderMode.BlackOnStaticRainbow:
					// Reset to the rainbow colors.
					SenseHat.Display.Reset();
					break;

				case RenderMode.BlackOnMovingRainbow:
					// Copy the rainbow colors -- but with an offset so that it "moves" with the characters.
					SenseHat.Display.CopyColorsToScreen(_rainbowColors, -scrollPixelOffset);
					break;

				case RenderMode.StaticRainbowOnBlack:
					SenseHat.Display.Fill(Colors.Black);
					break;

				case RenderMode.MovingRainbowOnBlack:
					SenseHat.Display.Fill(Colors.Black);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private Color GetCharacterColor(BwCharacterRendererPixelMap pixelMap)
		{
			switch (_currentMode)
			{
				case RenderMode.YellowOnBlue:
					return Colors.Yellow;

				case RenderMode.BlackOnStaticRainbow:
					return Colors.Black;

				case RenderMode.BlackOnMovingRainbow:
					return Colors.Black;

				case RenderMode.StaticRainbowOnBlack:
					// Let the rainbow colors be "pinned" to the display.
					return _rainbowColors[pixelMap.DisplayPixelX, pixelMap.DisplayPixelY];

				case RenderMode.MovingRainbowOnBlack:
					// Let the rainbow colors move with the characters ("restarting" on each character).
					return _rainbowColors[pixelMap.CharPixelX, pixelMap.CharPixelY];

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static IEnumerable<byte> FontBytes
		{
			get
			{
				return new byte[]
				{
					0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0x41, 0x00, 0x00, 0x7c, 0x7e, 0x0b, 0x0b, 0x7e, 0x7c,
					0x00, 0xff, 0x00, 0x42, 0x00, 0x00, 0x7f, 0x7f, 0x49, 0x49, 0x7f, 0x36, 0x00, 0xff, 0x00, 0x43, 0x00, 0x00, 0x3e,
					0x7f, 0x41, 0x41, 0x63, 0x22, 0x00, 0xff, 0x00, 0x44, 0x00, 0x00, 0x7f, 0x7f, 0x41, 0x63, 0x3e, 0x1c, 0x00, 0xff,
					0x00, 0x45, 0x00, 0x00, 0x7f, 0x7f, 0x49, 0x49, 0x41, 0x41, 0x00, 0xff, 0x00, 0x46, 0x00, 0x00, 0x7f, 0x7f, 0x09,
					0x09, 0x01, 0x01, 0x00, 0xff, 0x00, 0x47, 0x00, 0x00, 0x3e, 0x7f, 0x41, 0x49, 0x7b, 0x3a, 0x00, 0xff, 0x00, 0x48,
					0x00, 0x00, 0x7f, 0x7f, 0x08, 0x08, 0x7f, 0x7f, 0x00, 0xff, 0x00, 0x49, 0x00, 0x00, 0x41, 0x7f, 0x7f, 0x41, 0x00,
					0xff, 0x00, 0x4a, 0x00, 0x00, 0x20, 0x60, 0x41, 0x7f, 0x3f, 0x01, 0x00, 0xff, 0x00, 0x4b, 0x00, 0x00, 0x7f, 0x7f,
					0x1c, 0x36, 0x63, 0x41, 0x00, 0xff, 0x00, 0x4c, 0x00, 0x00, 0x7f, 0x7f, 0x40, 0x40, 0x40, 0x40, 0x00, 0xff, 0x00,
					0x4d, 0x00, 0x00, 0x7f, 0x7f, 0x06, 0x0c, 0x06, 0x7f, 0x7f, 0x00, 0xff, 0x00, 0x4e, 0x00, 0x00, 0x7f, 0x7f, 0x0e,
					0x1c, 0x7f, 0x7f, 0x00, 0xff, 0x00, 0x4f, 0x00, 0x00, 0x3e, 0x7f, 0x41, 0x41, 0x7f, 0x3e, 0x00, 0xff, 0x00, 0x50,
					0x00, 0x00, 0x7f, 0x7f, 0x09, 0x09, 0x0f, 0x06, 0x00, 0xff, 0x00, 0x51, 0x00, 0x00, 0x1e, 0x3f, 0x21, 0x61, 0x7f,
					0x5e, 0x00, 0xff, 0x00, 0x52, 0x00, 0x00, 0x7f, 0x7f, 0x19, 0x39, 0x6f, 0x46, 0x00, 0xff, 0x00, 0x53, 0x00, 0x00,
					0x26, 0x6f, 0x49, 0x49, 0x7b, 0x32, 0x00, 0xff, 0x00, 0x54, 0x00, 0x00, 0x01, 0x01, 0x7f, 0x7f, 0x01, 0x01, 0x00,
					0xff, 0x00, 0x55, 0x00, 0x00, 0x3f, 0x7f, 0x40, 0x40, 0x7f, 0x3f, 0x00, 0xff, 0x00, 0x56, 0x00, 0x00, 0x1f, 0x3f,
					0x60, 0x60, 0x3f, 0x1f, 0x00, 0xff, 0x00, 0x57, 0x00, 0x00, 0x7f, 0x7f, 0x30, 0x18, 0x30, 0x7f, 0x7f, 0x00, 0xff,
					0x00, 0x58, 0x00, 0x00, 0x63, 0x77, 0x1c, 0x1c, 0x77, 0x63, 0x00, 0xff, 0x00, 0x59, 0x00, 0x00, 0x07, 0x0f, 0x78,
					0x78, 0x0f, 0x07, 0x00, 0xff, 0x00, 0x5a, 0x00, 0x00, 0x61, 0x71, 0x59, 0x4d, 0x47, 0x43, 0x00, 0xff, 0x00, 0xc5,
					0x00, 0x00, 0x70, 0x7a, 0x2d, 0x2d, 0x7a, 0x70, 0x00, 0xff, 0x00, 0xc4, 0x00, 0x00, 0x71, 0x79, 0x2c, 0x2c, 0x79,
					0x71, 0x00, 0xff, 0x00, 0xd6, 0x00, 0x00, 0x39, 0x7d, 0x44, 0x44, 0x7d, 0x39, 0x00, 0xff, 0x00, 0xc9, 0x00, 0x00,
					0x7c, 0x7c, 0x54, 0x56, 0x45, 0x45, 0x00, 0xff, 0x00, 0xdc, 0x00, 0x00, 0x3d, 0x7d, 0x40, 0x40, 0x7d, 0x3d, 0x00,
					0xff, 0x00, 0x61, 0x00, 0x20, 0x74, 0x54, 0x54, 0x7c, 0x78, 0x00, 0xff, 0x00, 0x62, 0x00, 0x00, 0x7f, 0x7f, 0x48,
					0x48, 0x78, 0x30, 0x00, 0xff, 0x00, 0x63, 0x00, 0x00, 0x38, 0x7c, 0x44, 0x44, 0x44, 0x00, 0xff, 0x00, 0x64, 0x00,
					0x00, 0x38, 0x7c, 0x44, 0x44, 0x7f, 0x7f, 0x00, 0xff, 0x00, 0x65, 0x00, 0x00, 0x38, 0x7c, 0x54, 0x54, 0x5c, 0x18,
					0x00, 0xff, 0x00, 0x66, 0x00, 0x00, 0x04, 0x7e, 0x7f, 0x05, 0x05, 0x00, 0xff, 0x00, 0x67, 0x00, 0x00, 0x98, 0xbc,
					0xa4, 0xa4, 0xfc, 0x7c, 0x00, 0xff, 0x00, 0x68, 0x00, 0x00, 0x7f, 0x7f, 0x08, 0x08, 0x78, 0x70, 0x00, 0xff, 0x00,
					0x69, 0x00, 0x00, 0x48, 0x7a, 0x7a, 0x40, 0x00, 0xff, 0x00, 0x6a, 0x00, 0x80, 0x80, 0x80, 0xfa, 0x7a, 0x00, 0xff,
					0x00, 0x6b, 0x00, 0x00, 0x7f, 0x7f, 0x10, 0x38, 0x68, 0x40, 0x00, 0xff, 0x00, 0x6c, 0x00, 0x00, 0x41, 0x7f, 0x7f,
					0x40, 0x00, 0xff, 0x00, 0x6d, 0x00, 0x00, 0x7c, 0x7c, 0x18, 0x38, 0x1c, 0x7c, 0x78, 0x00, 0xff, 0x00, 0x6e, 0x00,
					0x00, 0x7c, 0x7c, 0x04, 0x04, 0x7c, 0x78, 0x00, 0xff, 0x00, 0x6f, 0x00, 0x00, 0x38, 0x7c, 0x44, 0x44, 0x7c, 0x38,
					0x00, 0xff, 0x00, 0x70, 0x00, 0x00, 0xfc, 0xfc, 0x24, 0x24, 0x3c, 0x18, 0x00, 0xff, 0x00, 0x71, 0x00, 0x00, 0x18,
					0x3c, 0x24, 0x24, 0xfc, 0xfc, 0x00, 0xff, 0x00, 0x72, 0x00, 0x00, 0x7c, 0x7c, 0x04, 0x04, 0x0c, 0x08, 0x00, 0xff,
					0x00, 0x73, 0x00, 0x00, 0x48, 0x5c, 0x54, 0x54, 0x74, 0x24, 0x00, 0xff, 0x00, 0x74, 0x00, 0x00, 0x04, 0x04, 0x3f,
					0x7f, 0x44, 0x44, 0x00, 0xff, 0x00, 0x75, 0x00, 0x00, 0x3c, 0x7c, 0x40, 0x40, 0x7c, 0x7c, 0x00, 0xff, 0x00, 0x76,
					0x00, 0x00, 0x1c, 0x3c, 0x60, 0x60, 0x3c, 0x1c, 0x00, 0xff, 0x00, 0x77, 0x00, 0x00, 0x1c, 0x7c, 0x70, 0x38, 0x70,
					0x7c, 0x1c, 0x00, 0xff, 0x00, 0x78, 0x00, 0x00, 0x44, 0x6c, 0x38, 0x38, 0x6c, 0x44, 0x00, 0xff, 0x00, 0x79, 0x00,
					0x00, 0x9c, 0xbc, 0xa0, 0xe0, 0x7c, 0x3c, 0x00, 0xff, 0x00, 0x7a, 0x00, 0x00, 0x44, 0x64, 0x74, 0x5c, 0x4c, 0x44,
					0x00, 0xff, 0x00, 0xe5, 0x00, 0x20, 0x74, 0x55, 0x55, 0x7c, 0x78, 0x00, 0xff, 0x00, 0xe4, 0x00, 0x20, 0x75, 0x54,
					0x54, 0x7d, 0x78, 0x00, 0xff, 0x00, 0xf6, 0x00, 0x00, 0x30, 0x7a, 0x48, 0x48, 0x7a, 0x30, 0x00, 0xff, 0x00, 0xe9,
					0x00, 0x00, 0x38, 0x7c, 0x54, 0x56, 0x5d, 0x19, 0x00, 0xff, 0x00, 0xfc, 0x00, 0x00, 0x3a, 0x7a, 0x40, 0x40, 0x7a,
					0x7a, 0x00, 0xff, 0x00, 0x30, 0x00, 0x00, 0x3e, 0x7f, 0x49, 0x45, 0x7f, 0x3e, 0x00, 0xff, 0x00, 0x31, 0x00, 0x00,
					0x40, 0x44, 0x7f, 0x7f, 0x40, 0x40, 0x00, 0xff, 0x00, 0x32, 0x00, 0x00, 0x62, 0x73, 0x51, 0x49, 0x4f, 0x46, 0x00,
					0xff, 0x00, 0x33, 0x00, 0x00, 0x22, 0x63, 0x49, 0x49, 0x7f, 0x36, 0x00, 0xff, 0x00, 0x34, 0x00, 0x00, 0x18, 0x18,
					0x14, 0x16, 0x7f, 0x7f, 0x10, 0xff, 0x00, 0x35, 0x00, 0x00, 0x27, 0x67, 0x45, 0x45, 0x7d, 0x39, 0x00, 0xff, 0x00,
					0x36, 0x00, 0x00, 0x3e, 0x7f, 0x49, 0x49, 0x7b, 0x32, 0x00, 0xff, 0x00, 0x37, 0x00, 0x00, 0x03, 0x03, 0x79, 0x7d,
					0x07, 0x03, 0x00, 0xff, 0x00, 0x38, 0x00, 0x00, 0x36, 0x7f, 0x49, 0x49, 0x7f, 0x36, 0x00, 0xff, 0x00, 0x39, 0x00,
					0x00, 0x26, 0x6f, 0x49, 0x49, 0x7f, 0x3e, 0x00, 0xff, 0x00, 0x2e, 0x00, 0x00, 0x60, 0x60, 0x00, 0xff, 0x00, 0x2c,
					0x00, 0x00, 0x80, 0xe0, 0x60, 0x00, 0xff, 0x00, 0x3f, 0x00, 0x00, 0x02, 0x03, 0x51, 0x59, 0x0f, 0x06, 0x00, 0xff,
					0x00, 0x21, 0x00, 0x00, 0x4f, 0x4f, 0x00, 0xff, 0x00, 0x22, 0x00, 0x00, 0x07, 0x07, 0x00, 0x00, 0x07, 0x07, 0x00,
					0xff, 0x00, 0x23, 0x00, 0x00, 0x14, 0x7f, 0x7f, 0x14, 0x14, 0x7f, 0x7f, 0x14, 0x00, 0xff, 0x00, 0x24, 0x00, 0x00,
					0x24, 0x2e, 0x6b, 0x6b, 0x3a, 0x12, 0x00, 0xff, 0x00, 0x25, 0x00, 0x00, 0x63, 0x33, 0x18, 0x0c, 0x66, 0x63, 0x00,
					0xff, 0x00, 0x26, 0x00, 0x00, 0x32, 0x7f, 0x4d, 0x4d, 0x77, 0x72, 0x50, 0x00, 0xff, 0x00, 0x2d, 0x00, 0x00, 0x08,
					0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0xff, 0x00, 0x2b, 0x00, 0x00, 0x08, 0x08, 0x3e, 0x3e, 0x08, 0x08, 0x00, 0xff,
					0x00, 0x2a, 0x00, 0x00, 0x08, 0x2a, 0x3e, 0x1c, 0x1c, 0x3e, 0x2a, 0x08, 0x00, 0xff, 0x00, 0x3a, 0x00, 0x00, 0x66,
					0x66, 0x00, 0xff, 0x00, 0x3b, 0x00, 0x00, 0x80, 0xe6, 0x66, 0x00, 0xff, 0x00, 0x2f, 0x00, 0x00, 0x40, 0x60, 0x30,
					0x18, 0x0c, 0x06, 0x02, 0x00, 0xff, 0x00, 0x5c, 0x00, 0x00, 0x02, 0x06, 0x0c, 0x18, 0x30, 0x60, 0x40, 0x00, 0xff,
					0x00, 0x3c, 0x00, 0x00, 0x08, 0x1c, 0x36, 0x63, 0x41, 0x41, 0x00, 0xff, 0x00, 0x3e, 0x00, 0x00, 0x41, 0x41, 0x63,
					0x36, 0x1c, 0x08, 0x00, 0xff, 0x00, 0x28, 0x00, 0x00, 0x1c, 0x3e, 0x63, 0x41, 0x00, 0xff, 0x00, 0x29, 0x00, 0x00,
					0x41, 0x63, 0x3e, 0x1c, 0x00, 0xff, 0x00, 0x27, 0x00, 0x00, 0x04, 0x06, 0x03, 0x01, 0x00, 0xff, 0x00, 0x60, 0x00,
					0x00, 0x01, 0x03, 0x06, 0x04, 0x00, 0xff, 0x00, 0x3d, 0x00, 0x00, 0x14, 0x14, 0x14, 0x14, 0x14, 0x14, 0x00
				};
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public enum PinyinColorScheme { None = 0, Dummitt = 1, Knibb = 2 };

    public class PinyinColorOptions : List<PinyinColorPalette>
    {
        public static Color DodgerBlue = Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF);
        public static Color LimeGreen = Color.FromArgb(0xFF, 0x32, 0xCD, 0x32);

        public PinyinColorOptions()
        {
            // Add(...) PinyinColors in enumeration order
            // PinyinColorScheme.None = 0
            Add(new PinyinColorPalette(PinyinColorScheme.None, Colors.Black, Colors.Black, Colors.Black, Colors.Black, Colors.Black));
            // PinyinColorScheme.Dummitt = 1
            Add(new PinyinColorPalette(PinyinColorScheme.Dummitt, Colors.Red, Colors.Orange, LimeGreen, DodgerBlue, Colors.Black));
            // PinyinColorScheme.Knibb = 2
            Add(new PinyinColorPalette(PinyinColorScheme.Knibb, DodgerBlue, LimeGreen, Colors.Orange, Colors.Red, Colors.Black));
        }

        public PinyinColorPalette this[PinyinColorScheme scheme]
        {
            get
            {
                int index = (int)scheme;
                return this[index];
            }
        }
    }

    public class PinyinColorPalette : List<Color>
    {
        public PinyinColorScheme Scheme;

        public PinyinColorPalette(PinyinColorScheme scheme, Color flat, Color rising, Color fallingRising, Color falling, Color neutral)
        {
            Scheme = scheme;
            // Add(...) Colors in Pinyin.Tones enumeration order
            // Pinyin.Tones.Neutral = 5 (in accessor mod 5 so this wraps to 0)
            Add(neutral);
            // Pinyin.Tones.Flat = 1
            Add(flat);
            // Pinyin.Tones.Rising = 2
            Add(rising);
            // Pinyin.Tones.FallingRising = 3
            Add(fallingRising);
            // Pinyin.Tones.Falling = 4
            Add(falling);
        }

        public string Name
        {
            get
            {
                return Enum.GetName(typeof(PinyinColorScheme), Scheme);
            }
        }

        public string Color1
        {
            get
            {
                return this[Pinyin.Tones.Flat].ToString();
            }
        }

        public string Color2
        {
            get
            {
                return this[Pinyin.Tones.Rising].ToString();
            }
        }

        public string Color3
        {
            get
            {
                return this[Pinyin.Tones.FallingRising].ToString();
            }
        }

        public string Color4
        {
            get
            {
                return this[Pinyin.Tones.Falling].ToString();
            }
        }

        public string Color5
        {
            get
            {
                return this[Pinyin.Tones.Neutral].ToString();
            }
        }

        public Color this[Pinyin.Tones tone]
        {
            get
            {
                int index = ((int)tone) % 5;
                return this[index];
            }
        }
    }
}

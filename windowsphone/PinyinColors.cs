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
    public enum PinyinColorScheme { None = 0, Dummitt = 1 };

    public class PinyinColorOptions : List<PinyinColors>
    {
        public static Color DodgerBlue = Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF);
        public static Color LimeGreen = Color.FromArgb(0xFF, 0x32, 0xCD, 0x32);

        public PinyinColorOptions()
        {
            Add(new PinyinColors(PinyinColorScheme.None, Colors.Black, Colors.Black, Colors.Black, Colors.Black, Colors.Black));
            Add(new PinyinColors(PinyinColorScheme.Dummitt, Colors.Red, Colors.Orange, LimeGreen, DodgerBlue, Colors.Black));
        }

        public PinyinColors this[PinyinColorScheme scheme]
        {
            get
            {
                int index = (int)scheme;
                return this[index];
            }
        }
    }

    public class PinyinColors : List<Color>
    {
        public PinyinColorScheme Scheme;

        public PinyinColors(PinyinColorScheme scheme, Color flat, Color rising, Color fallingRising, Color falling, Color neutral)
        {
            Scheme = scheme;
            Add(neutral);
            Add(flat);
            Add(rising);
            Add(fallingRising);
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

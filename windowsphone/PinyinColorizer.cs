using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class PinyinColorizer
    {
        public PinyinColorizer()
        {
        }

        public void Colorize(TextBlock block, DictionaryRecord record, PinyinColorScheme selected)
        {
            if (selected == PinyinColorScheme.None)
            {
                block.Text = record.Chinese.Pinyin;
                return;
            }

            PinyinColorOptions options = new PinyinColorOptions();
            PinyinColors colors = options[selected];
            block.Text = "";
            bool first = true;

            foreach (Chinese.Character c in record.Chinese.Characters)
            {
                Run run = new Run();
                run.Text = first ? c.Pinyin.MarkedUp : " " + c.Pinyin.MarkedUp;
                Color color = colors[c.Pinyin.Tone];
                if (color != Colors.Black) // black is special
                    run.Foreground = new SolidColorBrush(color);
                block.Inlines.Add(run);
                first = false;
            }
        }
    }
}

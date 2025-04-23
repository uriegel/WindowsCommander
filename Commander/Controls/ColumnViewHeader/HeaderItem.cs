using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Commander.Controls.ColumnViewHeader;

public record HeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left)
{
    public int Index { get; set; }
};

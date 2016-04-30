﻿using GraphX.PCL.Common.Models;

namespace NugetUnicorn.Ui
{
    public class DataVertex : VertexBase
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
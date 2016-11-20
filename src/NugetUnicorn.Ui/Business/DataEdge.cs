using GraphX.PCL.Common.Models;

namespace NugetUnicorn.Ui.Business
{
    public class DataEdge : EdgeBase<DataVertex>
    {
        public string Text { get; set; }

        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public DataEdge()
            : base(null, null, 1)
        {
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
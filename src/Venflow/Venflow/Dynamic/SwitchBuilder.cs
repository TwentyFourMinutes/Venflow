using System.Reflection.Emit;

namespace Venflow.Dynamic
{
    internal class SwitchBuilder
    {
        private int _labelIndex;

        private readonly Label[] _labels;
        private readonly ILGenerator _iLGenerator;

        internal SwitchBuilder(ILGenerator iLGenerator, int labelCount)
        {
            _iLGenerator = iLGenerator;
            _labels = new Label[labelCount];

            for (int i = 0; i < labelCount; i++)
            {
                _labels[i] = iLGenerator.DefineLabel();
            }
        }

        internal void MarkCase()
        {
            _iLGenerator.MarkLabel(_labels[_labelIndex++]);
        }

        internal Label[] GetLabels()
        {
            return _labels;
        }
    }
}

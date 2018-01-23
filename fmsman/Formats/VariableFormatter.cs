using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace fmsman.Formats
{
    public class VariableFormatter : IValueConverter
    {
        #region Члены IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ve = value as VarEntry;

            const string dv = "------";

            if (ve == null)
                return dv;

            var vt = ve.VarType;
            var vac = VarEntry.Accessor;

            if (vt.StartsWith("B"))
                return vac.ReadBoolean(ve.ShOffset).ToString();                
                
            if (vt.StartsWith("T"))
                return vac.ReadBoolean(ve.ShOffset).ToString();

            if (vt.StartsWith("F"))
                return vac.ReadSingle(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("D"))
                return vac.ReadDouble(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("I"))
                return vac.ReadInt32(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("C"))
            {
                var c = vac.ReadChar(ve.ShOffset);
                return $"{c} ({(int)c})";
            }

            if (vt.StartsWith("S"))
            {
                var bl = vac.ReadUInt16(ve.ShOffset + sizeof(UInt32) + sizeof(UInt16)) * sizeof(char);
                var b = new byte[bl];
                vac.ReadArray((int)ve.ShOffset + sizeof(UInt32) + sizeof(Int16) * 2, b, 0, bl);
                return Encoding.Unicode.GetString(b);
            }

            if (vt.StartsWith("K"))
                return "<CMDVAR>";

            if (vt.StartsWith("A"))
            {
                var l = vac.ReadUInt16(ve.ShOffset + sizeof(UInt32));
                var b = new byte[l];
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                vac.ReadArray<Byte>(ve.ShOffset + sizeof(UInt32) + sizeof(UInt16) * 2, b, 0, l);
                return $"({l}) {string.Join(" ", b.Select(x => x.ToString("X2")))}";
            }

            return dv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    internal class FunctionCls
    {
        CultureInfo trCulture = new CultureInfo("tr-TR");
        public static string DefaultImgPath = Path.Combine(Application.StartupPath, "Images", "noproductimage.jpg");
        public void Textboxformatter(TextBox TextBox)
        {
            if (decimal.TryParse(TextBox.Text, NumberStyles.Any, trCulture, out decimal fiyat))
            {
                // Hem binlik hem virgül ondalıklı formatla
                TextBox.Text = fiyat.ToString("N2", trCulture); // Örn: 1522.25 → 1.522,25
            }
        }
        public decimal ConvertDecimal(string Incoming_Value)
        {
            CultureInfo trCulture = new CultureInfo("tr-TR");
            Incoming_Value = Incoming_Value.Trim();

            string Cleanedtext = Incoming_Value.Replace(".", "").Replace(",", ".").Replace("₺","");

            if (decimal.TryParse(Cleanedtext, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return decimal.Parse(Incoming_Value);
        }
        public string DecimalFormatter(object Incoming_value)
        {
            string raw_value = Incoming_value?.ToString();
            raw_value = raw_value.Replace(",", ".");

            if (decimal.TryParse(raw_value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal P_value))
            {
                return P_value.ToString("N2", trCulture);
            }
            return "0";
        }
        public void OnlyNumericCharacter(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Girişi engelle
            }
        }
    }
}

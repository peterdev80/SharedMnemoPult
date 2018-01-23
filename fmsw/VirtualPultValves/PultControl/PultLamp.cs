using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualPultValves.PultControl
{
    /// <summary>
    /// Выполните шаги 1a или 1b, а затем 2, чтобы использовать этот пользовательский элемент управления в файле XAML.
    ///
    /// Шаг 1a. Использование пользовательского элемента управления в файле XAML, существующем в текущем проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:VirtualPultValves.PultControl"
    ///
    ///
    /// Шаг 1б. Использование пользовательского элемента управления в файле XAML, существующем в другом проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:VirtualPultValves.PultControl;assembly=VirtualPultValves.PultControl"
    ///
    /// Потребуется также добавить ссылку из проекта, в котором находится файл XAML,
    /// на данный проект и пересобрать во избежание ошибок компиляции:
    ///
    ///     Щелкните правой кнопкой мыши нужный проект в обозревателе решений и выберите
    ///     "Добавить ссылку"->"Проекты"->[Поиск и выбор проекта]
    ///
    ///
    /// Шаг 2)
    /// Теперь можно использовать элемент управления в файле XAML.
    ///
    ///     <MyNamespace:PultLamp/>
    ///
    /// </summary>
    public class PultLamp : Control
    {
        static PultLamp()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PultLamp), new FrameworkPropertyMetadata(typeof(PultLamp)));
        }
        public static readonly DependencyProperty VklLampProperty = DependencyProperty.Register(
        "VklLamp", typeof(bool), typeof(PultLamp), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,null,corceValue));
        public bool VklLamp
        {
            get { return (bool)GetValue(VklLampProperty); }
            set { this.SetValue(VklLampProperty, value); }
        }

        private static object corceValue (DependencyObject d, object value)
        {
            Boolean intval;
            int val_i = 0;
            intval = int.TryParse(value.ToString(), out val_i);
            PultLamp mnComp=d as PultLamp;
            if (intval)
            {
                if (val_i == 1) return true;
            }
            else
                if ((bool)value) return true;
            
            
          
            return false;
        }
    }
}

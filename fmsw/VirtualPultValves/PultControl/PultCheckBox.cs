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
using System.Collections.ObjectModel;

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
    ///     <MyNamespace:PultCheckBox/>
    ///
    /// </summary>
   
    public class PultCheckBox : CheckBox
    {
        static PultCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PultCheckBox), new FrameworkPropertyMetadata(typeof(PultCheckBox)));
            
            
        }
      
     

        public static readonly DependencyProperty GeometryTextProperty = DependencyProperty.Register(
         "GeometryText", typeof(StreamGeometry), typeof(PultCheckBox), new FrameworkPropertyMetadata(new StreamGeometry(), FrameworkPropertyMetadataOptions.AffectsRender));
        public StreamGeometry GeometryText
        {
            get { return (StreamGeometry)GetValue(GeometryTextProperty); }
            set { this.SetValue(GeometryTextProperty, value); }
        }

        public static readonly DependencyProperty GeometryLineProperty = DependencyProperty.Register(
         "GeometryLine", typeof(StreamGeometry), typeof(PultCheckBox), new FrameworkPropertyMetadata(new StreamGeometry(), FrameworkPropertyMetadataOptions.AffectsRender));
        public StreamGeometry GeometryLine
        {
            get { return (StreamGeometry)GetValue(GeometryLineProperty); }
            set { this.SetValue(GeometryLineProperty, value); }

        }

     

        

    }
}

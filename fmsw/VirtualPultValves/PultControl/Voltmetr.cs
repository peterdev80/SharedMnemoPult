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
using System.Diagnostics;

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
    ///     <MyNamespace:Voltmetr/>
    ///
    /// </summary>
    public class Voltmetr : ProgressBar
    {
      
        static Voltmetr()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Voltmetr), new FrameworkPropertyMetadata(typeof(Voltmetr)));

            

        
        }
        
       /* public static readonly DependencyProperty PartValueProperty = DependencyProperty.Register(
           "PartValue", typeof(double), typeof(Voltmetr), new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsRender,  OnValue));
        public double PartValue
        {
            get { return (double)GetValue(PartValueProperty); }
            set { this.SetValue(PartValueProperty, value); }
        }
        Rectangle rng;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
           
            rng = GetTemplateChild("PART_Indicator") as Rectangle;
            BindingOperations.SetBinding(this,Voltmetr.PartValueProperty,new Binding{Source =rng,Path=new PropertyPath(Rectangle.ActualWidthProperty) });
        }


        private static void OnValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine(e.NewValue);

        }*/

    }
}

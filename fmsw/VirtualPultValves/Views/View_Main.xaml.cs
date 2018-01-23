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
using VirtualPultValves.ViewModel;
using System.Collections.ObjectModel;
using VirtualPultValves.Model;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_Main.xaml
    /// </summary>
    public partial class MainView : UserControl
    {

        public MainView()
        {
            var init = LinkInpu.Instance;
            InitializeComponent();

        }
        public ObservableCollection<GroupCommandViewModel> CommandsViewModel { get; set; }

        public static readonly DependencyProperty HeightRowProperty = DependencyProperty.Register(
          "HeightRow", typeof(GridLength), typeof(MainView), new FrameworkPropertyMetadata(new GridLength(60),
          FrameworkPropertyMetadataOptions.AffectsRender, OnPultOrientUpd));
        public GridLength HeightRow
        {
            get { return (GridLength)GetValue(HeightRowProperty); }
            set { this.SetValue(HeightRowProperty, value); }
        }


        public static readonly DependencyProperty WidthRowProperty = DependencyProperty.Register(
          "WidthRow", typeof(GridLength), typeof(MainView), new FrameworkPropertyMetadata(new GridLength(60),
          FrameworkPropertyMetadataOptions.AffectsRender, OnPultOrientUpd));
        public GridLength WidthRow
        {
            get { return (GridLength)GetValue(WidthRowProperty); }
            set { this.SetValue(WidthRowProperty, value); }
        }
        private static void OnPultOrientUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            MainView con = d as MainView;
            con.OrientUpdate();
        }

        private Orientation _MenuOrient = Orientation.Vertical;
        public Orientation MenuOrient
        {
            get { return _MenuOrient; }
            set
            {
                if (value == _MenuOrient) return;
                _MenuOrient = value;
                OrientUpdate();

            }
        }
        private void OrientUpdate()
        {
            if (_MenuOrient == Orientation.Vertical)
            {
                TopMenu.Visibility = Visibility.Hidden;
                LeftMenu.Visibility = Visibility.Visible;
                HeightOrient.Height = new GridLength(0);//GridLength.Auto;
                WidthOrient.Width = new GridLength(120);

            }
            if (_MenuOrient == Orientation.Horizontal)
            {
                TopMenu.Visibility = Visibility.Visible;
                LeftMenu.Visibility = Visibility.Hidden;
                HeightOrient.Height = HeightRow;// GridLength.Auto;
                WidthOrient.Width = WidthRow;

            }

        }




        /*   private MainViewModel viewmodel;







              public MainViewModel ViewModel
              {

                  get
                  {
                      return viewmodel;
                  }
              }*/


        public static readonly DependencyProperty TypePultProperty = DependencyProperty.Register(
         "TypePult", typeof(PultValvesType), typeof(MainView), new FrameworkPropertyMetadata(PultValvesType.None,
         FrameworkPropertyMetadataOptions.AffectsRender, OnTypePultUpd));
        public PultValvesType TypePult
        {
            get { return (PultValvesType)GetValue(TypePultProperty); }
            set { this.SetValue(TypePultProperty, value); }
        }


        private static void OnTypePultUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            /* View_Main con = d as View_Main;
             DataAccess.ValvesDictonarySigleton.Instance.valvesType = (PultValvesType)e.NewValue;


             con.viewmodel = new MainViewModel();
             con.CommandsViewModel = new ObservableCollection<GroupCommandViewModel>(con.viewmodel.GCommands.ToList());
             con.DataContext = con.viewmodel;*/
        }

        //выбор клапана по умолчанию
        public static readonly DependencyProperty SelectFormatProperty = DependencyProperty.Register(
        "SelectFormat", typeof(string), typeof(MainView), new FrameworkPropertyMetadata("",
        FrameworkPropertyMetadataOptions.AffectsRender, OnSelectFormatUpd));
        public string SelectFormat
        {
            get { return (string)GetValue(SelectFormatProperty); }
            set { this.SetValue(SelectFormatProperty, value); }
        }

        private static void OnSelectFormatUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vmcurent = d as MainView;
            var datacontext_source = vmcurent.FindResource("vMain") as ViewModel_Main;
            if (datacontext_source != null)
            {
                foreach (GroupCommandViewModel lst in datacontext_source.GCommands)
                {
                    foreach (CommandViewModel fcomm in lst.CommandsGroup)
                    {
                        if (fcomm.DisplayName == e.NewValue.ToString())
                            fcomm.Command.Execute(e.NewValue.ToString());
                    }
                }
            }

        }
    }
}

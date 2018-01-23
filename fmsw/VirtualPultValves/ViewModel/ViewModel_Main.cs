using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Views;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using VirtualPultValves.DataAccess;
using System.Windows;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_Main : ViewModelBase
    {
        const string FormatName = "pack://application:,,,/VirtualPultValves;component/Data/XMLValues.xml";

        private ReadOnlyCollection<GroupCommandViewModel> _gcommands;
        private Dictionary<string, string> ValvesLocate { get; set; }
        private InPUWin32View _inwin1;
        private InPUWin32View _inwin2;
        private View_InPU vInPu;
        private View_AllValves _allValves;
        private View_BAPD _bapd;
        private View_BR1 _br1;
        private View_BRUB _brub;
        private View_BRUS _brus;
        private View_DSD _dsd;
        private View_EPKP _epkp;
        private View_EPKPSA _epksa;
        private View_EPKRD _epkrd;
        private View_KR1 _kr1;
        private View_KR3 _kr3;
        private View_KSDBO _ksdbo;
        private View_KVDBO _kvdbo;
        private View_Lyk _lyk;
        private View_Manov _manov;
        private View_NeptunP2 _nprav;
        private View_RAP10 _rap10;
        private View_RAP7 _rap7;
        private View_RPV _rpv;
        private View_RUS _rus;
        private View_XSASA _xsasa;
        private View_ZDV _zdv;
        private View_ZGL _zgl;

        private List<string> GetLocate()
        {
            var lst = new List<string>();
            foreach (KeyValuePair<string, string> kvp in ValvesLocate)
                lst.Add(kvp.Value);
            return lst.Distinct().ToList();
        }
        public List<string> GetNames(string locate)
        {

            var lst = new List<string>();
            foreach (KeyValuePair<string, string> kvp in ValvesLocate)
            {
                if (kvp.Value == locate)
                    lst.Add(kvp.Key);
            }
            return lst;


        }
        private ModelVariableRepository repos;

        public BoolValue Inpu1ModelSelect { get; set; }
        public BoolValue Inpu2ModelSelect { get; set; }
        public ViewModel_Main()
        {
            var dr = new DataReader();
            ValvesLocate = dr.GetValvesShowCommand(FormatName);
            repos = ModelVariableRepository.Instance;
            Inpu1ModelSelect = repos.BoolFMSValues[0].ValState;
            Inpu2ModelSelect = repos.BoolFMSValues[1].ValState;
            Inpu1ModelSelect.PropertyChanged += Inpu1ModelSelect_PropertyChanged;
            Inpu2ModelSelect.PropertyChanged += Inpu1ModelSelect_PropertyChanged;
        }

        /// <summary>
        /// обновление состояния команды
        /// </summary>
        void Inpu1ModelSelect_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Создает и возвращает Win32 ИнПУ-1
        /// </summary>
        public InPUWin32View inwin1
        {
            get
            {
                if (_inwin1 == null)
                {
                    _inwin1 = new InPUWin32View();
                    _inwin1.InpuNum = 1;

                }
                return _inwin1;

            }
        }
        /// <summary>
        /// Создает и возвращает Win32 ИнПУ-2
        /// </summary>
        public InPUWin32View inwin2
        {
            get
            {
                if (_inwin2 == null)
                {
                    _inwin2 = new InPUWin32View();
                    _inwin2.InpuNum = 2;

                }
                return _inwin2;

            }
        }
        private UserControl _workspaces;
        public UserControl Workspaces
        {
            get { return _workspaces; }
            set
            {
                if (value == _workspaces) return;
                _workspaces = value;
                foreach (GroupCommandViewModel gcmd in GCommands)
                {
                    foreach (CommandViewModel cmd in gcmd.CommandsGroup)
                    {
                        cmd.StateCommand = false;
                        if (_workspaces != null)
                        {
                            if (notselectInpu == cmd.DisplayName) cmd.StateCommand = true;
                        }
                    }
                }
                base.OnPropertyChanged("Workspaces");

            }
        }

      /*  ///Работа с Wago
        private WagoIO wagoObmen;
        private void Start_WagoObmen()
        {
            wagoObmen = new WagoIO();
 
        }*/







        #region Commands
        /// <summary>
        /// выдает поллный сгрупированный перечень комманд для вызова клапана
        /// </summary>
        public ReadOnlyCollection<GroupCommandViewModel> GCommands
        {
            get
            {
                if (_gcommands == null)
                {
                    List<GroupCommandViewModel> lst = new List<GroupCommandViewModel>();
                    foreach (string str in GetLocate())
                    {
                        List<CommandViewModel> cmds = this.CreateCommands(str);
                        GroupCommandViewModel gcvm = new GroupCommandViewModel(cmds, str);
                        lst.Add(gcvm);
                    }

                    _gcommands = new ReadOnlyCollection<GroupCommandViewModel>(lst);

                }
                return _gcommands;

            }

        }


        List<CommandViewModel> CreateCommands(string locate)
        {
            List<CommandViewModel> lst = new List<CommandViewModel>();

            foreach (string str in GetNames(locate))
            {
                lst.Add(new CommandViewModel(str, new RelayCommand(param => SelectControl(param), param => CanExecute(param))));
            }

            return lst;
        }
        //  private string selectInpu = string.Empty;
        private string notselectInpu = String.Empty;
        private bool CanExecute(object param)
        {
            string selectInpu = InpuModelSelector.Instance.LoadedInpu;
            if (param == null) return false;

            if (selectInpu == string.Empty)
            {
                if ((param.ToString() == "ИнПУ-1") && (Inpu1ModelSelect.ValueState)) return false;
                if ((param.ToString() == "ИнПУ-2") && (Inpu2ModelSelect.ValueState)) return false;
            }



            if (selectInpu == param.ToString()) return false;
            return true;

        }


        private void SelectControl(object ControlName)
        {
            notselectInpu = ControlName.ToString();
            string selectInpu = InpuModelSelector.Instance.LoadedInpu;
            switch (ControlName.ToString())
            {
                case "ИнПУ-1":
                    {
                        //if ((selectInpu == string.Empty) && (!Inpu1ModelSelect.ValueState))
                        {
                            InpuModelSelector.Instance.LoadedInpu = "ИнПУ-2";
                            if (vInPu == null) vInPu = new View_InPU();
                            vInPu.DataContext = inwin1;
                            vInPu.RUSKlapan.Visibility = Visibility.Hidden;
                            Workspaces = vInPu;
                            WagoIO.Instance.TypePult = 1;
                        }
                    } break;
                case "ИнПУ-2":
                    {
                        //if ((selectInpu == string.Empty) && (!Inpu2ModelSelect.ValueState))
                        {
                            InpuModelSelector.Instance.LoadedInpu = "ИнПУ-1";
                            if (vInPu == null) vInPu = new View_InPU();
                            vInPu.DataContext = inwin2;
                            vInPu.RUSKlapan.Visibility = Visibility.Hidden; Workspaces = vInPu;
                            WagoIO.Instance.TypePult = 2;
                        }
                    } break;
                case "ЭПК-ПСА": { if (_epksa == null) _epksa = new View_EPKPSA(); Workspaces = _epksa; } break;
                case "РПВ-1,2": { if (_rpv == null) _rpv = new View_RPV(); Workspaces = _rpv; } break;
                case "ЭПК-РД": { if (_epkrd == null) _epkrd = new View_EPKRD(); Workspaces = _epkrd; } break;
                case "ЭПК-П": { if (_epkp == null) _epkp = new View_EPKP(); Workspaces = _epkp; } break;
                case "КР-3": { if (_kr3 == null) _kr3 = new View_KR3(); Workspaces = _kr3; } break;
                case "БРУС": { if (_brus == null) _brus = new View_BRUS(); Workspaces = _brus; } break;
                case "ЗДВ": { if (_zdv == null) _zdv = new View_ZDV(); Workspaces = _zdv; } break;
                case "ДСД": { if (_dsd == null) _dsd = new View_DSD(); Workspaces = _dsd; } break;
                case "Нептун Прав": { if (_nprav == null) _nprav = new View_NeptunP2(); Workspaces = _nprav; } break;
                case "КР-2": { if (_xsasa == null) _xsasa = new View_XSASA(); Workspaces = _xsasa; } break;
                case "Люк СА БО": { if (_lyk == null) _lyk = new View_Lyk(); Workspaces = _lyk; } break;
                case "БР1": { if (_br1 == null) _br1 = new View_BR1(); Workspaces = _br1; } break;
                case "РАП-10": { if (_rap10 == null) _rap10 = new View_RAP10(); Workspaces = _rap10; } break;
                case "РАП-7": { if (_rap7 == null) _rap7 = new View_RAP7(); Workspaces = _rap7; } break;
                case "КСД-БО": { if (_ksdbo == null) _ksdbo = new View_KSDBO(); Workspaces = _ksdbo; } break;
                case "КВД БО-СУ": { if (_kvdbo == null) _kvdbo = new View_KVDBO(); Workspaces = _kvdbo; } break;
                case "БРУБ": { if (_brub == null) _brub = new View_BRUB(); Workspaces = _brub; } break;
                case "Мановак.": { if (_manov == null) _manov = new View_Manov(); Workspaces = _manov; } break;
                case "КР1": { if (_kr1 == null) _kr1 = new View_KR1(); Workspaces = _kr1; } break;
                case "БАПД": { if (_bapd == null) _bapd = new View_BAPD(); Workspaces = _bapd; } break;
                case "БАПД ": { if (_bapd == null) _bapd = new View_BAPD(); Workspaces = _bapd; } break;
                case "Згл.КСД-СУ": { if (_zgl == null) _zgl = new View_ZGL(); Workspaces = _zgl; } break;
                case "ВСЕ": { if (_allValves == null) _allValves = new View_AllValves(); Workspaces = _allValves; } break;
                // case "РУС": { if (_rus == null) _rus = new View_RUS(); Workspaces = _rus; } break;
                case "РУС":
                    {

                        if ((Workspaces == vInPu) && (vInPu != null))
                        {
                            if (vInPu.RUSKlapan.Visibility == Visibility.Hidden)
                                vInPu.RUSKlapan.Visibility = Visibility.Visible;
                            else
                                vInPu.RUSKlapan.Visibility = Visibility.Hidden;

                        }
                        else
                        {

                            if (_rus == null) _rus = new View_RUS();
                            Workspaces = _rus;
                        }




                    } break;

            }
            //  base.OnPropertyChanged("Workspaces");
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fmslapi;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;
using System.IO.Compression;
using fmslapi.Channel;

namespace fmsproxy
{
    public class ModelVarProxy : ModelProxy
    {
        #region Частные данные
        private readonly Dictionary<int, IVariable> _vars = new Dictionary<int, IVariable>();
        private string _vmap;
        #endregion

        #region Наследуемые данные
        protected readonly Dictionary<IVariable, int> _pvars = new Dictionary<IVariable, int>();
        public readonly Dictionary<int, byte> VarLengths = new Dictionary<int, byte>();
        private readonly HashSet<IVariable> _changes = new HashSet<IVariable>();
        protected IVariablesChannel _varchan;
        #endregion

        public override void Load()
        {
            _vmap = _config["variables.map"];
            var varmap = Assembly.GetExecutingAssembly().GetManifestResourceStream(_config["map.stream"]);
            var channel = _config["fms.channel"];
            var canreceive = _config.GetBool("fms.receive");
            var endpoint = _config["name"];

            VariablesChanged ch = Changed;
            if (!canreceive)
                ch = null;

            if (!string.IsNullOrWhiteSpace(channel))
            {
                _varchan = _manager.JoinVariablesChannel(channel, endpoint, _vmap, ChanChanged, ch);
                LoadLocalMaps(varmap);
                CustomPostRegistration(_pvars.Keys);
            }

            base.Load();
        }

        private void LoadLocalMaps(Stream varmap)
        {
            var rdr = new BinaryReader(new GZipStream(varmap, CompressionMode.Decompress), Encoding.UTF8);

            var index = 1;

            while(true)
            {
                var vname = rdr.ReadString();
                if (string.IsNullOrEmpty(vname))
                    break;

                var type = rdr.ReadChar();

                IVariable v = null;
                byte len = 0;

                if (type == 'B') { v = _varchan.GetBoolVariable(vname); len = 1; }
                if (type == 'I') { v = _varchan.GetIntVariable(vname); len = 4; }
                if (type == 'F') { v = _varchan.GetFloatVariable(vname); len = 4; }
                if (type == 'C') { v = _varchan.GetCharVariable(vname); len = 1; }
                if (type == 'K') { v = _varchan.GetKVariable(vname); len = 1; }

                if (v != null)
                {
                    _vars.Add(index, v);
                    _pvars.Add(v, index);
                    VarLengths.Add(index, len);

                    v.VariableChanged += (sender, isinit) =>
                    {
                        if (isinit)
                            return;

                        lock (_changes) { _changes.Add(v); }
                    };
                }

                index++;
            }
        }

        protected override void ProcessIncomingUDP(ISenderChannel Sender, byte[] Data)
        {
            base.ProcessIncomingUDP(null, Data);

            try
            {
                UInt16 vindex = 0;
                var rdr = new BinaryReader(new MemoryStream(Data));
                var sz = rdr.ReadInt16();
                while (vindex != 2000)
                {
                    vindex = rdr.ReadUInt16();
                    if (vindex == 2000)
                        break;

                    if (!_vars.ContainsKey(vindex))
                        return;

                    var v = _vars[vindex];

                    byte ll = 0;
                    if (!VarLengths.TryGetValue(vindex, out ll))
                        return;

                    object val = null;

                    switch (v.VariableType)
                    {
                        case VariableType.Unknown:
                            rdr.ReadBytes(ll);                          // Пропускаем байты данных во входном потоке
                            break;

                        case VariableType.Boolean: val = rdr.ReadBoolean(); break;
                        case VariableType.Int32: val = rdr.ReadInt32(); break;
                        case VariableType.Single: val = rdr.ReadSingle(); break;
                        case VariableType.Char: val = (char)rdr.ReadByte(); break;
                        case VariableType.KMD: val = (byte)rdr.ReadByte(); break;
                        case VariableType.String: break; 

                        default:
                            break;
                    }

                    var undo = false;
                    IndividualVarProcess(vindex, v, val, ref undo);

                    if (undo)
                        continue;

                    switch (v.VariableType)
                    {
                        case VariableType.Unknown:
                            break;

                        case VariableType.Boolean: (v as IBoolVariable).Value = (bool)val; break;
                        case VariableType.Int32: (v as IIntVariable).Value = (Int32)val; break;
                        case VariableType.Single: (v as IFloatVariable).Value = (float)val; break;
                        case VariableType.Char: (v as ICharVariable).Value = (char)val; break;
                        case VariableType.KMD: 
                            if ((byte)val == 1)
                                (v as IKVariable).Set();
                            break;

                        case VariableType.String:
                            break;

                        default:
                            break;
                    }

                }
            }
            catch (Exception) { }
            finally
            {
                if (_varchan != null)
                    _varchan.SendChanges();
            }
        }

        /// <summary>
        /// Отправка переменных в сеть в формате обмена 200 серии
        /// </summary>
        /// <param name="IsInit"></param>
        private void Changed(bool IsInit, IVariable[] ChangedList)
        {
            // Формат
            // i2 - длина пакета
            // i2 - индекс переменной   -|
            // xx - значение переменной -| <- повторяется n раз
            // i2 = 2000 - маркер конца посылки

            if (IsInit)
                return;

            IVariable[] changes;

            lock (_changes)
            {
                changes = _changes.ToArray();
                _changes.Clear();
            }

            if (changes.Length == 0)
                return;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((Int16)0);

            foreach (var v in changes)
            {
                if (!_vars.ContainsValue(v)) 
                    continue;

                if (v.VariableType == VariableType.Unknown)
                    continue;

                IndividualUdpSendProcessPre(v, wr);

                wr.Write((Int16)_pvars[v]);

                switch (v.VariableType)
                {
                    case VariableType.Boolean:
                        wr.Write((byte)((v as IBoolVariable).Value ? 1 : 0));
                        break;

                    case VariableType.Int32:
                        wr.Write((v as IIntVariable).Value);
                        break;

                    case VariableType.Single:
                        wr.Write((v as IFloatVariable).Value);
                        break;

                    case VariableType.Char:
                        wr.Write((byte)((v as ICharVariable).Value));
                        break;

                    case VariableType.KMD:
                        wr.Write((byte)1);
                        break;

                    default:
                        break;
                }

                IndividualUdpSendProcess(v, wr);
            }

            // Маркер конца посылки
            wr.Write((Int16)2000);

            wr.Write((long)0);

            wr.Seek(0, SeekOrigin.Begin);
            wr.Write((Int16)ms.Length);

            base.ProcessIncomingUDP(null, ms.ToArray());
        }

        public int GetIndex200(string VariableName)
        {
            var z = (from v in _vars
                     where v.Value.VariableName == VariableName
                     select v.Key).FirstOrDefault();

            return z;
        }

        public override void CloseProxy()
        {
            if (_varchan != null)
                _varchan.Leave();

            base.CloseProxy();
        }

        #region Наследуемые методы
        protected virtual void CustomPostRegistration(IEnumerable<IVariable> Variables) { }
        protected virtual void IndividualUdpSendProcess(IVariable Variable, BinaryWriter Writer) { }
        protected virtual void IndividualUdpSendProcessPre(IVariable Variable, BinaryWriter Writer) { }
        #endregion
    }
}

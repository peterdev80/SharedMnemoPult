using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fmslstrap.Channel;
using fmslstrap.Variables;
using fmslstrap.Variables.VarTypes;
using System.Threading;
using fmslstrap.Configuration;

namespace fmslstrap.Pipe
{
    internal partial class PipeManager
    {
        /// <summary>
        /// Обработка административных команд
        /// </summary>
        /// <param name="stream"></param>
        private void ProceedAdministration(Stream stream)
        {
            var bmsgrdr = new BinaryReader(stream, Encoding.UTF8);

            var cmd = bmsgrdr.ReadChar();

            #region Команда GetLog
            if (cmd == 'A')
            {
                var fullog = Logger.Subscribe(AdmOnLog);

                OnPipeFinalize += p => Logger.Unsubscribe(AdmOnLog);

                foreach (var l in fullog)
                    AdmSendLogLine(l.LogTime, l.LogString, l.LogSender);
            }
            #endregion

            #region Команда Get Pipes
            if (cmd == 'B')
            {
                OnPipeFinalize += pf =>
                    {
                        OnCreatePipe -= AdmOnCreatePipe;

                        try
                        {
                            /*var apipes = (from p in ActivePipes
                                          where p != this && ( p._vartable != null || p.channelname != null )
                                          select p).ToArray();*/

                            ActivePipesLock.EnterReadLock();

                            foreach (var p in ActivePipes)
                                p.OnPipeFinalize -= AdmOnPipeFinalize;
                        }
                        finally
                        {
                            ActivePipesLock.ExitReadLock();
                        }
                    };

                OnCreatePipe += AdmOnCreatePipe;

                IEnumerable<PipeManager> pipes;
                try
                {
                    ActivePipesLock.EnterReadLock();

                    pipes = (from p in ActivePipes
                             where p != this
                             select p).ToArray();

                    foreach (var p in pipes)
                        p.OnPipeFinalize += AdmOnPipeFinalize;
                }
                finally
                {
                    ActivePipesLock.ExitReadLock();
                }

                foreach (var p in pipes)
                    AdmSendPipeClient(p);
            }

            #endregion

            #region Команда Get Pipes Statistics
            if (cmd == 'C')
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                wr.Write('C');

                PipeManager[] p;
                try
                {
                    ActivePipesLock.EnterReadLock();

                    p = (from pi in ActivePipes
                         where pi != this && (pi._vartable != null || pi._channelname != null)
                         select pi).ToArray();

                    wr.Write(p.Length);
                }
                finally
                {
                    ActivePipesLock.ExitReadLock();
                }

                foreach (var pi in p)
                {
                    wr.Write(pi._instanceid);
                    wr.Write(Interlocked.Read(ref pi.SendedAmount));
                    wr.Write(Interlocked.Read(ref pi.SendedCount));
                    wr.Write(Interlocked.Read(ref pi.ReceivedAmount));
                    wr.Write(Interlocked.Read(ref pi.ReceivedCount));
                    try
                    {
                        pi._registeredvariableslock.EnterReadLock();
                        wr.Write(pi._registeredvariables.Count);
                    }
                    finally
                    {
                        pi._registeredvariableslock.ExitReadLock();
                    }
                }

                MessageToClient(ms.ToArray());
            }
            #endregion

            #region Команда Get Hosts
            if (cmd == 'D')
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                wr.Write('D');

                var eps = EndPointsList.GetAll().Where(x => !x.IsCommandEndPoint).ToArray();

                wr.Write(eps.Length);

                foreach (var ep in eps)
                {
                    wr.Write(ep.UID);
                    wr.Write(ep.Host);
                    wr.Write(ep.Channel);
                    wr.Write(ep.EndPoint.ToString());
                    wr.Write(ep.Received);
                    wr.Write(ep.Sended);
                    wr.Write((UInt32)ep.SendSpeed);
                    wr.Write((UInt32)ep.ReceiveSpeed);
                    wr.Write(ep.DontSendTo);
                }

                MessageToClient(ms.ToArray());
            }
            #endregion

            #region Команда Get Pipe Vars
            if (cmd == 'E')
            {
                var subcmd = bmsgrdr.ReadChar();

                var token = bmsgrdr.ReadInt32();

                var instance = bmsgrdr.ReadUInt64();

                if (subcmd == 'A')
                {
                    PipeManager pipe;
                    try
                    {
                        ActivePipesLock.EnterReadLock();
                        pipe = ActivePipes.FirstOrDefault(x => x._instanceid == instance);
                    }
                    finally
                    {
                        ActivePipesLock.ExitReadLock();
                    }

                    if (pipe == null)
                        return;

                    Variable[] varpack;
                    try
                    {
                        pipe._registeredvariableslock.EnterReadLock();
                        varpack = pipe._registeredvariables.ToArray();
                    }
                    finally
                    {
                        pipe._registeredvariableslock.ExitReadLock();
                    }

                    if (_admvarlists.TryGetValue(token, out var hold))
                    {
                        _admvarlists[token] = varpack;
                        AdmSendVarPack(token, varpack.Except(hold).ToArray());
                    }
                    else
                    {
                        _admvarlists[token] = varpack;
                        AdmSendVarPack(token, varpack);
                    }
                }
            }
            #endregion

            #region Команда Subscribe to Global Var Changes
            if (cmd == 'F')
            {
                var subcmd = bmsgrdr.ReadChar();

                if (subcmd == 'A')
                {
                    OnPipeFinalize += p => VariablesManager.GlobalVarChanged -= GlobalVarChanged;

                    VariablesManager.GlobalVarChanged += GlobalVarChanged;

                }

                if (subcmd == 'B')
                {
                    uint[] lst;
                    lock (_admglobalchangedlist)
                    {
                        lst = _admglobalchangedlist.ToArray();
                        _admglobalchangedlist.Clear();
                    }

                    var ms = new MemoryStream();
                    var wr = new BinaryWriter(ms);

                    wr.Write('F');

                    wr.Write(lst.Length);

                    foreach (var vn in lst)
                    {
                        wr.Write(vn);
                    }

                    MessageToClient(ms.ToArray());
                }

            }
            #endregion

            #region Команда Send Var As Changed
            if (cmd == 'G')
            {
                var varindex = bmsgrdr.ReadUInt32();

                var incoming = new HashSet<Variable>();

                var var = VariablesTable.GlobalGetVariable(varindex, out var vartable);

                if (var == null)
                    return;

                (var as WVar)?.Reset();

                incoming.Add(var);

                PipeManager[] pipes;
                try
                {
                    ActivePipesLock.EnterReadLock();
                    pipes = ActivePipes.ToArray();
                }
                finally
                {
                    ActivePipesLock.ExitReadLock();
                }

                VariablesManager.RaiseGlobalVarChanged(incoming, false);

                vartable.SendChanges(incoming);

                foreach (var pm in pipes.Where(p => p._instanceid != _instanceid))
                {
                    Variable[] rv;
                    try
                    {
                        pm._registeredvariableslock.EnterReadLock();
                        rv = incoming.Intersect(pm._registeredvariables).ToArray();
                    }
                    finally
                    {
                        pm._registeredvariableslock.ExitReadLock();
                    }

                    pm.SendVarPackToPipe(rv, false);
                }
            }
            #endregion

            #region AdmLoc
            if (cmd == 'H')
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                AdmLocChannel.Send(ms.ToArray());
            }

            if (cmd == 'I')
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                wr.Write('I');

                var cfls = ConfigurationManager.GetConfigFileNames();

                wr.Write((UInt16)cfls.Count);

                foreach (var f in cfls)
                    wr.Write(f);

                ConfigurationManager.PackConfigToStream(ms);

                MessageToClient(ms.ToArray());
            }
            #endregion
        }

        #region Поддержка журнала
        private void AdmOnLog(string Sender, string Log, DateTime Time)
        {
            AdmSendLogLine(Time, Log, Sender);
        }

        /// <summary>
        /// Отсылает одну запись журнала административному инструменту
        /// </summary>
        /// <param name="Time">Временная отметка записи журнала</param>
        /// <param name="Log">Запись журнала</param>
        /// <param name="Sender">Отправитель записи</param>
        private void AdmSendLogLine(DateTime Time, string Log, string Sender)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('A');
            wr.Write(Time.Ticks);
            wr.Write(Log);
            wr.Write(Sender);

            MessageToClient(ms.ToArray());
        }
        #endregion

        #region Поддежка списка локальных клиентов
        private void AdmOnCreatePipe(PipeManager Pipe)
        {
            Pipe.OnPipeFinalize += AdmOnPipeFinalize;
            AdmSendPipeClient(Pipe);
        }

        private void AdmSendPipeClient(PipeManager Pipe)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('B');
            wr.Write('A');
            wr.Write(Pipe._instanceid);
            var ep = string.IsNullOrWhiteSpace(Pipe._endpointname) ? "" : Pipe._endpointname;
            wr.Write(ep);
            if (Pipe._channel != null)
            {
                wr.Write('R');
                wr.Write(Pipe._channel.Name);
            }

            if (Pipe._vartable != null)
            {
                wr.Write('V');
                wr.Write(Pipe._varchanname);
            }

            MessageToClient(ms.ToArray());
        }

        /// <summary>
        /// Отсылает уведомление об отключившемся клиенте
        /// </summary>
        /// <param name="Pipe"></param>
        private void AdmOnPipeFinalize(PipeManager Pipe)
        {
            Pipe.OnPipeFinalize -= AdmOnPipeFinalize;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('B');
            wr.Write('B');
            wr.Write(Pipe._instanceid);

            MessageToClient(ms.ToArray());
        }
        #endregion

        #region Поддержка списка переменных
        private readonly Dictionary<int, IEnumerable<Variable>> _admvarlists = new Dictionary<int, IEnumerable<Variable>>();

        private void AdmSendVarPack(int token, Variable[] vars)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('E');
            wr.Write('A');

            wr.Write(token);

            wr.Write(vars.Length);

            wr.Write(VariablesTable.ShMemoryName);

            foreach (var v in vars)
            {
                wr.Write(v.VarNum);
                wr.Write(v.Name);
                wr.Write(v.Type);
                wr.Write(0);
                wr.Write(v.SharedOffset);
                wr.Write(v.Comment);
            }

            MessageToClient(ms.ToArray());
        }
        #endregion

        #region Поддержка глобального списка изменений
        private readonly HashSet<uint> _admglobalchangedlist = new HashSet<uint>();

        private void GlobalVarChanged(IEnumerable<Variable> ChangeList, bool IsInit)
        {
            lock (_admglobalchangedlist)
            {
                foreach (var v in ChangeList)
                    _admglobalchangedlist.Add(v.VarNum);
            }
        }
        #endregion
    }
}

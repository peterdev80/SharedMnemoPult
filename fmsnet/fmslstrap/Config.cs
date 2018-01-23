using System;
using fmslstrap.Configuration;

namespace fmslstrap
{
    public class Config
    {
        #region Частные данные
        private static string _domain;
        private static string _name;
        private static bool _standalone;
        private static bool _copylocal;
        private static bool _silent;
        private static string _codebase;
        private static bool _verbose;

        private static ConfigSection _global;
        #endregion

        #region Конструкторы
        public static void Init()
        {
            _global = ConfigurationManager.GetSection("global");

            _name = GetString("name");
            _standalone = GetBool("standalone");
            _copylocal = GetBool("copylocal");
            _codebase = GetString("codebase");
            _silent = GetBool("silent");
            _verbose = GetBool("verbose");

            if (string.IsNullOrWhiteSpace(_codebase))
                _codebase = ".\\";

            _domain = _standalone ? Guid.NewGuid().ToString() : _global["domainname"].Value;

            if (_name == "*")
                _name = Environment.MachineName;
        }
        #endregion

        public static bool GetBool(string Value)
        {
            var s = _global[Value].Value;

            return s == "yes" || s == "true" || s == "on" || s == "1";
        }

        public static string GetString(string Value)
        {
            return _global[Value].Value ?? "";
        }

        #region Публичные свойств

        public static string WorkstationName 
        {
            get { return _name; }
        }

        /// <summary>
        /// Имя домена в конфигурации
        /// </summary>
        public static string DomainName 
        {
            get { return _domain; }
        }

        public static string CodeBase
        {
            get { return _codebase; } 
        }

        /// <summary>
        /// Разрешение сохранять принятые сборки локально в текущей папке
        /// </summary>
        public static bool CopyLocal
        {
            get { return _copylocal; }
        }

        /// <summary>
        /// Разрешение на распространение сборок
        /// </summary>
        /// <remarks>
        /// При наличии ключа Config и отсутствии EnableDeploy -> Распространение разрешено
        /// </remarks>
        public static bool EnableDeploy
        {
            get { return false; }
        }

        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        public static string ConfigFile
        {
            get { return ConfigurationManager.PrimaryConfigFile; }
        }

        /// <summary>
        /// Скрыть всплывающие сообщения в трее
        /// </summary>
        public static bool Silent
        {
            get { return _silent; }
        }

        public static bool Standalone
        {
            get { return _standalone; }
        }

        public static bool Verbose
        {
            get { return _verbose; }
        }
        #endregion
    }
}

using System;
using System.Windows.Markup;
using System.Xaml;

namespace fmslapi
{
    /// <summary>
    /// Обертка для расширения разметки Reference
    /// </summary>
    /// <remarks>
    /// Является копией стандартного расширения x:Reference с исправленной
    /// ошибкой, приводящей к исключению в дизайнере XAML в VS2010
    /// </remarks>
    [ContentProperty("Name")]
    public class Reference : MarkupExtension
    {
        #region Конструкторы
        public Reference()
        {
        }

        public Reference(string name)
        {
            Name = name;
        }
        #endregion

        #region Получения значения расширения разметки
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(IXamlNameResolver)) as IXamlNameResolver;
            if (service == null)
                return null;

            if (string.IsNullOrEmpty(Name))
                return null;
            
            var fixupToken = service.Resolve(Name);
            if (fixupToken == null)
            {
                var names = new[] { Name };
                fixupToken = service.GetFixupToken(names, true);
            }

            return fixupToken;
        }
        #endregion

        #region Публичные свойства
        [ConstructorArgument("name")]
        public string Name { get; set; }
        #endregion
    }
}

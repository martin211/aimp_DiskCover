using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace AIMP.DiskCover.Resources
{
    public class LocalizerExtension: MarkupExtension
    {
        private static readonly String _localizedDataFullName = typeof(LocalizedData).FullName;

        public LocalizerExtension()
        {
        }

        public LocalizerExtension(String key)
        {
            Key = key;
        }

        [ConstructorArgument("key")] 
        public String Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            String typeName = _localizedDataFullName;

            String[] parts = Key.Split(new[] {'.'});
            
            // If resource name is composite (is not on the top of hierarchy)
            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    typeName += "+" + parts[i];
                }
            }

            Type containingClassType = Type.GetType(typeName);

            if (containingClassType == null)
            {
                throw new LocalizerException("Containing class is not found: " + typeName);
            }
            
            // Search for the requested static property.
            PropertyInfo property = containingClassType.GetProperty(parts.Last());

            if (property == null)
            {
                throw new LocalizerException("Property is not found: " + parts.Last());
            }

            // Return its value.
            return property.GetValue(null, null);
        }
    }

    /// <summary>
    /// An exception that is raised when some error happens 
    /// in <see cref="LocalizerExtension"/>.
    /// </summary>
    public class LocalizerException : ApplicationException
    {
        public LocalizerException(String message)
            :base(message)
        {
            
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;

namespace Assets.Utils
{
    static class ConfigFileReader 
    {
        public static Dictionary<string, float> GetPropsFromConfig(string scriptName, string[] props) // TODO (refactor): remove this when no one use it any more
        {
            Dictionary<string, float> propsFromConfig = new Dictionary<string, float>();
            XPathDocument docNav = new(@"Assets/BusinessLogic/Configuration/PropsConfig.xml");
            XPathNavigator nav = docNav.CreateNavigator();

            foreach (string prop in props)
            {
                string xPath = "/scripts/" + scriptName + "/" + prop;
                propsFromConfig.Add(prop, float.Parse(nav.SelectSingleNode(xPath).Value));
            }
            
            return propsFromConfig;
        }

        public static void LoadMembersFromConfigFile(object instance)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            foreach (FieldInfo field in instance.GetType().GetFields(bindingFlags))
            {
                float? fieldValue = GetPropertyFromConfig(instance.GetType().Name, field.Name);

                if (fieldValue != null)
                {
                    field.SetValue(instance, fieldValue);
                }
            }
        }

        public static float? GetPropertyFromConfig(string scriptName, string propertyName)
        {
            if (IsBackingField(propertyName)) 
            {
                return null;
            }

            XPathDocument documentPath = new(@"Assets/BusinessLogic/Configuration/PropsConfig.xml");
            XPathNavigator pathNavigator = documentPath.CreateNavigator();
            string xPath = "/scripts/" + scriptName + "/" + propertyName;

            XPathNavigator configNode = pathNavigator.SelectSingleNode(xPath);

            if (configNode != null)
            {
                return float.Parse(configNode.Value);
            }

            return null;
        }

        private static bool IsBackingField(string propertyName)
        {
            return propertyName.Contains("BackingField");
        }
    }
}

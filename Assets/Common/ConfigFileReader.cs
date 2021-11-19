using System.Collections.Generic;
using System.Xml.XPath;

namespace Assets.Common
{
    static class ConfigFileReader 
    {
        public static Dictionary<string, float> GetPropsFromConfig(string scriptName, string[] props)
        {
            Dictionary<string, float> propsFromConfig = new Dictionary<string, float>();
            XPathDocument docNav = new XPathDocument(@"Assets/Configuration/PropsConfig.xml"); ;
            XPathNavigator nav = docNav.CreateNavigator();

            foreach (string prop in props)
            {
                string xPath = "/scripts/" + scriptName + "/" + prop;
                propsFromConfig.Add(prop, float.Parse(nav.SelectSingleNode(xPath).Value));
            }
            
               
            return propsFromConfig;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.Api
{
    public class AppUrl
    {
        /// <summary>
        /// App的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Web应用的程序路径
        /// </summary>
        public string BaseUrl { get; set; }
    }

    public class WebConnectorSettings
    {
        public List<AppUrl> Applications { get; set; }
    }
}

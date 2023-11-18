using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;

namespace prog_p1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string connectionString = ConfigurationManager.AppSettings["Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"];
   
    }
}

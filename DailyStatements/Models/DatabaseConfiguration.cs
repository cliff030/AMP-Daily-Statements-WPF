using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMPStatements.Models
{
    public class DatabaseConfiguration
    {
        private EntityConnectionStringBuilder _eb;
        private string _ConnectionStringName = "Entities";
        private string _AppConnectionString;

        private string _DatabaseName;
        public DatabaseConfiguration() { }

        public DatabaseConfiguration(EntityConnectionStringBuilder eb)
        {
            _eb = eb;
        }

        public DatabaseConfiguration(string DatabaseName)
        {
            _DatabaseName = DatabaseName;

            try
            {
                _AppConnectionString = ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString;

                _eb = new EntityConnectionStringBuilder(_AppConnectionString);
                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(_eb.ProviderConnectionString);

                sb.InitialCatalog = _DatabaseName;

                _eb.ProviderConnectionString = sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CompanyList> GetCompanyList()
        {
            List<CompanyList> CompanyList = new List<CompanyList>();

            CompanyList SelectFinancial = new CompanyList();
            SelectFinancial.CompanyName = "Select Financial";
            SelectFinancial.DatabaseName = "CSDATA8";
            SelectFinancial.Order = 0;

            CompanyList LibertyFinancial = new CompanyList();
            LibertyFinancial.CompanyName = "Liberty Financial";
            LibertyFinancial.DatabaseName = "CSDATA8_INC";
            LibertyFinancial.Order = 1;

            CompanyList FirstFinancial = new CompanyList();
            FirstFinancial.CompanyName = "First Financial";
            FirstFinancial.DatabaseName = "CSDATA8_FFN";
            FirstFinancial.Order = 2;

            CompanyList KarmaCapital = new CompanyList();
            KarmaCapital.CompanyName = "Karma Capital";
            KarmaCapital.DatabaseName = "CSDATA8_KAR";
            KarmaCapital.Order = 3;
            
            CompanyList.Add(SelectFinancial);
            CompanyList.Add(LibertyFinancial);
            CompanyList.Add(FirstFinancial);
            CompanyList.Add(KarmaCapital);

            return CompanyList;
        }

        public struct CompanyList
        {
            public string CompanyName { get; set; }
            public string DatabaseName { get; set; }
            public int Order { get; set; }


            // Override the == and != operators so we can compare these objects
            // This is useful for set operators on ViewModel public properties
            public static bool operator ==(CompanyList c1, CompanyList c2)
            {
                return c1.Equals(c2);
            }

            public static bool operator !=(CompanyList c1, CompanyList c2)
            {
                return !c1.Equals(c2);
            }
        }

        public string ConnectionString
        {
            get { return _eb.ToString(); }
        }

        public EntityConnectionStringBuilder eb
        {
            get { return _eb; }
            set { _eb = value; }
        }

        public string DatabaseName
        {
            get { return _DatabaseName; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMPStatements.Models;

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

        public static List<Company> GetCompanyList()
        {
            List<Company> CompanyList = new List<Company>();

            using(var cxt = new CreditsoftCompaniesEntities())
            {
                var q = from c in cxt.Companies
                        where c.Active == true
                        orderby c.MenuOption ascending
                        select c;

                CompanyList = q.ToList();
            }

            return CompanyList;
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

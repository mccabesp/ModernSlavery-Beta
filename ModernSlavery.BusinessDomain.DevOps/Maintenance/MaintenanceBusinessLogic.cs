namespace ModernSlavery.BusinessDomain.DevOps.Maintenance
{
    //Includes debugging, logging
    public interface IMaintenanceBusinessLogic
    {
        void RunWebjob();

        void CopyDatabase(bool anonymised = true);

    }

    public class MaintenanceBusinessLogic : IMaintenanceBusinessLogic
    {
        public void CopyDatabase(bool anonymised = true)
        {
            throw new System.NotImplementedException();
        }

        public void RunWebjob()
        {
            throw new System.NotImplementedException();
        }
    }
}
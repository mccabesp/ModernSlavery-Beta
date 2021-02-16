namespace ModernSlavery.BusinessDomain.DevOps.Performance
{
    public interface IScalabilityBusinessLogic
    {
        void ScaleUpDatabase();
        void ScaleOutDatabase();
        void ScaleDatabase();

        void ScaleUpWebService();
        void ScaleOutWebService();
        void ScaleWebService();

    }

    public class ScalabilityBusinessLogic : IScalabilityBusinessLogic
    {
        public void ScaleDatabase()
        {
            throw new System.NotImplementedException();
        }

        public void ScaleOutDatabase()
        {
            throw new System.NotImplementedException();
        }

        public void ScaleOutWebService()
        {
            throw new System.NotImplementedException();
        }

        public void ScaleUpDatabase()
        {
            throw new System.NotImplementedException();
        }

        public void ScaleUpWebService()
        {
            throw new System.NotImplementedException();
        }

        public void ScaleWebService()
        {
            throw new System.NotImplementedException();
        }
    }
}
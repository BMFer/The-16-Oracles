namespace The16Oracles.domain.Services
{
    public interface IDataModel
    {
        string Name { get; }
    }

    public class DataModel : IDataModel
    {
        public DataModel() { }

        public string Name => this.GetType().Name;
    }

    public interface ITestService
    {
        Task<string> GetDataModelName();

    }

    public class TestService : ITestService
    {
        private readonly IDataModel _dataModel;

        public TestService(IDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public async Task<string> GetDataModelName()
        {
            return await Task.Run(() => this._dataModel.Name);
        }
    }
}

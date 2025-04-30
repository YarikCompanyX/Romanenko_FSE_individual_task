namespace Romanenko_FSE_individual_task
{
    public class TxtFactory : IFileHandlerFactory
    {
        public ILoader CreateLoader() => new TxtLoader();
        public ISaver CreateSaver() => new TxtSaver();
    }

    public class BinFactory : IFileHandlerFactory
    {
        public ILoader CreateLoader() => new BinLoader();
        public ISaver CreateSaver() => new BinSaver();
    }

    public class HtmlFactory : IFileHandlerFactory
    {
        public ILoader CreateLoader() => new HtmlLoader();
        public ISaver CreateSaver() => new HtmlSaver();
    }
}
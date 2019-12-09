using Ninject;
using SymbolRecognitionLib.ViewModels;

namespace SymbolRecognitionLib.InversionOfControl
{
    public static class IoCController
    {
        public static IKernel Kernel { get; private set; } = new StandardKernel();

        public static void SetUp()
        {
            bind();
        }

        static void bind()
        {
            Kernel.Bind<SymbolsRecognitionViewModel>().ToConstant(new SymbolsRecognitionViewModel());
            Kernel.Bind<TrainNetworkViewModel>().ToConstant(new TrainNetworkViewModel());
        }
    }
}

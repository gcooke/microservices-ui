using Gateway.Web.Models.Interrogation;

namespace Gateway.Web.Services
{
    public interface IRiskBatchInterrogationService
    {
        void PopulateLookups(InterrogationModel model);

        void Analyze(InterrogationModel model);
    }
}
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.DataAccess
{
    public interface ISettingsRepository
    {
        SystemSettings GetSettings();
        void UpdateAlarmThreshold(int threshold);
    }
}

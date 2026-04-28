using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.DataAccess
{
    public interface IAdminRepository
    {
        AdminUser Authenticate(string username, string password);
        void UpdatePassword(string username, string newPasswordHash);
    }
}

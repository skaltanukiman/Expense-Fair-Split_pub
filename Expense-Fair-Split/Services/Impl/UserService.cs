using Expense_Fair_Split.Data;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Expense_Fair_Split.Services.Impl
{
    public class UserService :IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;

        public UserService(IUserRepository userRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEMailAsync(string email)
        {
            return await _userRepository.GetByEMailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                await _userRepository.AddAsync(user);
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                await _userRepository.UpdateAsync(user);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }
    }
}

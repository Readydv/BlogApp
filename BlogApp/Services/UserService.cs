//using BlogApp.InterfaceServices;
//using BlogApp.Models;
//using Microsoft.EntityFrameworkCore;
//using System.Runtime.InteropServices.Marshalling;

//namespace BlogApp.Services
//{
//    public class UserService : IUserService
//    {
//        //private readonly BlogContext _context;

//        //public UserService(BlogContext context)
//        //{
//         //   _context = context;
//        //}

//        //public async Task<User> CreateAsync(User user)
//        //{
//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();
//            return user;
//        }

//        public async Task<IEnumerable<User>> GetAllAsync()
//        {
//            return await _context.Users
//                .Include(u => u.Posts)
//                .Include(u => u.Comments)
//                .ToListAsync();
//        }

//        public async Task<User> GetByIdAsync(Guid id)
//        {
//            return await _context.Users
//                .Include(u => u.Posts)
//                .Include(u => u.Comments)
//                .FirstOrDefaultAsync(u => u.Id == id);
//        }

//        public async Task UpdateAsync(User user)
//        {
//            _context.Users.Update(user);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteAsync(Guid id)
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user != null)
//            {
//                _context.Users.Remove(user);
//                await _context.SaveChangesAsync();
//            }
//        }
//    }
//}

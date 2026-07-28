using mau.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.ViewModel
{
    public class SidebarViewModel : BaseViewModel
    {
        private readonly DbConnect _context;
        private string _fullName = string.Empty;
        private string _groupName = string.Empty;

        public SidebarViewModel(DbConnect context) : base(context)
        {
            _context = context;
            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync();
                FullName = user?.FullName ?? string.Empty;
                GroupName = user?.GroupName ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                FullName = string.Empty;
                GroupName = string.Empty;
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value;
                OnPropertyChanged();
            }
        }
    }
}

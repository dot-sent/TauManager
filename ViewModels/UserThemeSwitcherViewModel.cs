using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    public class UserThemeSwitcherViewModel
    {
        public Dictionary<int, string> AllThemes {get; set;}
        public UserCSSTheme CurrentTheme { get; set; }
    }
}
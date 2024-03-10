using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Linear_Algebra_Calculator.Core
{
    public class Change : ObservableObjects
    {
        public static string setb = "3";
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertychanged();
            }
        }

        public Change()
        {
            Name = setb;
        }
    }
}

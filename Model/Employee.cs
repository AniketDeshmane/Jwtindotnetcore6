using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace APIwithSQLLite.Model
{
    public class Employee
    {
        [Key]
        public int EmpID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DeptID { get; set; }
        public string DeptName { get; set; } = string.Empty;    
    }
}

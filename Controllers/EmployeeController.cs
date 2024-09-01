using APIwithSQLLite.Data;
using APIwithSQLLite.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIwithSQLLite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public EmployeeController(DataContext dataContext) {
            _dataContext = dataContext;
        }
        [HttpGet]
        [Route("getEmp")]
        public async Task<ActionResult<List<Employee>>> getEmp()
        {
            return Ok(await _dataContext.Employees.ToListAsync());

        }

        [HttpPost]
        [Route("addEmp")]
        public async Task<ActionResult> addEmp([FromBody] Employee emp) {
            _dataContext.Employees.Add(emp);
            await _dataContext.SaveChangesAsync();
            return Ok();    

        }
        [HttpPost]
        [Route("addEmps")]
        public async Task<ActionResult> addEmps([FromBody] List<Employee> lstemployee)
        {
            foreach (var emp in lstemployee)
            {
                _dataContext.Employees.Add(emp);
                await _dataContext.SaveChangesAsync();
            }
            return Ok();
        }
        [HttpDelete]
        [Route("deleteEmp")]
        public async Task<ActionResult> deleteEmp(int empid)
        {
            var employee = await _dataContext.Employees.FirstOrDefaultAsync(x => x.EmpID == empid);
            if (employee == null)
            {
                return NotFound(); // Return 404 if the employee is not found
            }

            _dataContext.Employees.Remove(employee); // Remove the employee from the DbSet
            await _dataContext.SaveChangesAsync(); // Save changes to the database
            return Ok(); // Return 200 OK status
        }

    }
}

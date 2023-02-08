using Microsoft.AspNetCore.Mvc;
using WebApplication7.ModelViews;
using WebApplication7.Requests;
using WebApplication7.Services;
using System.Data;
using WebApplication7.Data;
using Microsoft.EntityFrameworkCore;
using WebApplication7.Models;

namespace WebApplication7.Controllers
{
    public class StudentController : Controller
    {
        private IStudentService _studentService;
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _studentService = new StudentService(context);
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public JsonResult List(string keyword, int? page)
        {
            

            var studentDb = _context.Students.Where(x => x.IsDeleted == 0);

            
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                studentDb = studentDb.Where(x => x.FullName.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            // paging
            int pageSize = 2;
            int pageIndex = page ?? 1;
            int start = (pageIndex - 1) * pageSize;
            int totalCount = studentDb.Count();
            int totalPage = totalCount / pageSize;
            if (totalCount % pageSize > 0)
            {
                totalPage += 1;
            }

            var students = studentDb.Skip(start).Take(pageSize).ToList();
            int totalItem = students.Count;

            var studentIds = students.Select(x => x.Id).ToList();
            var courseStudents = _context.CourseStudents.Where(x => studentIds.Contains(x.StudentId) && x.IsDeleted == 0)
                .Join(_context.Courses, cs => cs.CourseId, c => c.Id, (cs, c) => new { cs, c })
                .Where(x => x.c.IsDeleted == 0)
                .ToList();

            var data = new List<StudentModel>();
            foreach (var student in students)
            {
                data.Add(new StudentModel
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Code = student.Code,
                    Age = student.Age,
                    Address = student.Address,
                });
            }

            return Json(new PagedStudentModel
            {
                Students = data,
                TotalCount = totalCount,
                TotalItem = totalItem,
                TotalPage = totalPage,
                PageSize = pageSize,
                PageCurrent = pageIndex
            });
        }

        public ActionResult LoadData(DatatableRequest request)
        {
            try
            {
                // Datatable params
                var draw = request.Draw;
                var start = request.Start;
                var length = request.Length;
                var orderColumn = request.Order.First().Column;
                var sortColumn = request.Columns.ElementAt(orderColumn).Name;
                var sortColumnDir = request.Order.First().Dir;
                var searchValue = request.Search.Value;

                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all data    
                var studentData = _context.Students.Where(x => x.IsDeleted == 0);

                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "fullName":
                                studentData = studentData.OrderBy(x => x.FullName);
                                break;
                            case "age":
                                studentData = studentData.OrderBy(x => x.Age);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "fullName":
                                studentData = studentData.OrderByDescending(x => x.FullName);
                                break;
                            case "age":
                                studentData = studentData.OrderByDescending(x => x.Age);
                                break;
                            default:
                                break;
                        }
                    }
                    //studentData = studentData.OrderBy(sortColumn, sortColumnDir);
                }

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    studentData = studentData.Where(x => x.FullName.ToLower().Contains(searchValue) || x.Code.ToLower().Contains(searchValue));
                }

                //total number of rows count     
                recordsTotal = studentData.Count();

                //Paging     
                var students = studentData.Skip(skip).Take(pageSize).ToList();

                var data = studentData.Include(x => x.CourseStudents)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new StudentModel
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        Code = x.Code,
                        Age = x.Age,
                        Address = x.Address,
                        Courses = x.CourseStudents != null ? x.CourseStudents.Where(a => a.IsDeleted == 0 && a.Course.IsDeleted == 0).Select(a => new SimpleCourseModel
                        {
                            Id = a.Course.Id,
                            Name = a.Course.Name
                        }).ToList() : new List<SimpleCourseModel>()
                    })
                    .ToList();

                //Returning Json Data    
                return Json(new DatatableModel<StudentModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyCode(string code)
        {
            if (!_studentService.VerifyCode(code))
            {
                return Json($"Code {code} is already in use.");
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult<ResponseModel> Create([FromBody] AddStudentRequest request)
        {
            // validate
            if (!ModelState.IsValid)
            {
                var message = string.Join(" </br> ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return new ResponseModel
                {
                    Status = 0,
                    Message = message
                };
            }

            var result = _studentService.Store(request);
            if (result.Status == 0)
            {
                return new ResponseModel
                {
                    Status = 0,
                    Message = result.Message
                };
            }

            return new ResponseModel
            {
                Status = 1,
                Message = "Create student successfully"
            };
        }

        public ActionResult<ResponseModel<StudentModel>> GetById(int id)
        {
            

            var student = _context.Students.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
            if (student == null)
            {
                return new ResponseModel<StudentModel>
                {
                    Status = 0,
                    Message = "Student does not exist"
                };
            }

            return new ResponseModel<StudentModel>
            {
                Status = 1,
                Data = new StudentModel
                {
                    Id = id,
                    FullName = student.FullName,
                    Code = student.Code,
                    Age = student.Age,
                    Address = student.Address
                }
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> Update([FromBody] UpdateStudentRequest request)
        {
            // validate
            if (!ModelState.IsValid)
            {
                var message = string.Join(" </br> ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            var result = _studentService.Update(request);
            if (result.Status == 0)
            {
                return new ResponseModel
                {
                    Status = 0,
                    Message = result.Message
                };
            }

            return new ResponseModel
            {
                Status = 1,
                Message = "Update student successfully"
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> Delete(int id)
        {
            var result = _studentService.Delete(id);
            if (result.Status == 0)
            {
                return new ResponseModel
                {
                    Status = 0,
                    Message = result.Message
                };
            }

            return new ResponseModel
            {
                Status = 1,
                Message = "Delete student successfully"
            };
        }

        public ActionResult<ResponseModel<CourseModel>> GetCourseInfo(int id)
        {
            

            var course = _context.Courses.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
            if (course == null)
            {
                return new ResponseModel<CourseModel>
                {
                    Status = 0,
                    Message = "Course does not exist"
                };
            }

            return new ResponseModel<CourseModel>
            {
                Status = 1,
                Data = new CourseModel
                {
                    Id = id,
                    Name = course.Name,
                    Code = course.Code,
                    MaxStudentNum = course.MaxStudentNum
                }
            };
        }
    }
}

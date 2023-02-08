using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Xml.Linq;
using WebApplication7.Data;
using WebApplication7.Models;
using WebApplication7.ModelViews;
using WebApplication7.Requests;
using WebApplication7.Services;

namespace WebApplication7.Controllers
{
    public class CourseController : Controller
    {
        private ICourseService _CourseService;
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _CourseService = new CourseService(context);
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public JsonResult List(string keyword)
        {
            

            var courseDb = _context.Courses.Where(x => x.IsDeleted == 0);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                courseDb = courseDb.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            var courseIds = courseDb.Select(x => x.Id).ToList();
            var courseStudents = _context.CourseStudents.Where(x => courseIds.Contains(x.CourseId) && x.IsDeleted == 0)
                .Join(_context.Students, c => c.StudentId, s => s.Id, (c, s) => new { c, s })
                .Where(x => x.s.IsDeleted == 0)
                .ToList();

            var courses = courseDb.ToList();
            var data = new List<CourseModel>();
            foreach (var course in courses)
            {
                data.Add(new CourseModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Code = course.Code,
                    MaxStudentNum = course.MaxStudentNum,
                    CurrentStudentNum = courseStudents.Count(x => x.c.CourseId == course.Id)
                });
            }

            return Json(data);
        }

        public ActionResult LoadData(DatatableRequest request)
        {
            try
            {
                //Creating instance of DatabaseContext class  
                //Request.Form.TryGetValue("draw", out var draws);
                //Request.Form.TryGetValue("start", out var starts);
                //Request.Form.TryGetValue("length", out var lengths);
                //Request.Form.TryGetValue("order[0][column]", out var orderColumns);
                //Request.Form.TryGetValue("columns[" + orderColumn + "][name]", out var sortColumns);
                //Request.Form.TryGetValue("order[0][dir]", out var sortColumnDirs);
                //Request.Form.TryGetValue("search[value]", out var searchValues);

                // Datatable params
                var draw = request.Draw;
                var start = request.Start;             
                var length = request.Length;              
                var orderColumn = request.Order.First().Column;
                var sortColumn = request.Columns.ElementAt(orderColumn).Name;                
                var sortColumnDir = request.Order.First().Dir;
                var searchValue = request.Search.Value;

                // Paging Size
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all data    
                var courseData = _context.Courses.Where(x => x.IsDeleted == 0);

                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumnDir == "asc")
                    {
                        switch (sortColumn)
                        {
                            case "name":
                                courseData = courseData.OrderBy(x => x.Name);
                                break;
                            case "code":
                                courseData = courseData.OrderBy(x => x.Code);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (sortColumn)
                        {
                            case "name":
                                courseData = courseData.OrderByDescending(x => x.Name);
                                break;
                            case "code":
                                courseData = courseData.OrderByDescending(x => x.Code);
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    courseData = courseData.Where(x => x.Name.ToLower().Contains(searchValue) || x.Code.ToLower().Contains(searchValue));
                }

                //total number of rows count     
                recordsTotal = courseData.Count();

                //Paging     
                var data = courseData.Include(x => x.CourseStudents)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new CourseModel {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code,
                        MaxStudentNum = x.MaxStudentNum,
                        CurrentStudentNum = x.CourseStudents != null ? x.CourseStudents.Where(a => a.IsDeleted == 0 && a.Student.IsDeleted == 0).Count() : 0
                    })
                    .ToList();

                //Returning Json Data    
                return Json(new DatatableModel<CourseModel>
                { 
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                });
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyCode(string code)
        {
            if (!_CourseService.VerifyCode(code))
            {
                return Json($"Code {code} is already in use.");
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult<ResponseModel> Create([FromBody] AddCourseRequest request)
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

            var result = _CourseService.Store(request);
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
                Message = "Create course successfully"
            };
        }

        public ActionResult<ResponseModel<CourseModel>> GetById(int id)
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

        [HttpPost]
        public ActionResult<ResponseModel> Update([FromBody] UpdateCourseRequest request)
        {
            // validate
            if (!ModelState.IsValid)
            {
                var message = string.Join(" </br> ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            var result = _CourseService.Update(request);
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
                Message = "Update course successfully"
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> Delete(int id)
        {
            var result = _CourseService.Delete(id);
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
                Message = "Delete course successfully"
            };
        }

        [HttpGet]
        public ActionResult<ResponseModel<List<StudentCourseModel>>> AllStudentsOfCourse(int id, string keyword)
        {
            

            var course = _context.Courses.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
            if (course == null)
            {
                return new ResponseModel<List<StudentCourseModel>>
                {
                    Status = 0,
                    Message = "Course does not exist"
                };
            }

            var enrolledStudentIds = _context.CourseStudents.Where(x => x.CourseId == id && x.IsDeleted == 0)
                .Select(x => x.StudentId)
                .ToList();

            var studentDb = _context.Students.Where(x => x.IsDeleted == 0);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                studentDb = studentDb.Where(x => x.FullName.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            var data = new List<StudentCourseModel>();
            foreach (var student in studentDb)
            {
                data.Add(new StudentCourseModel
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Code = student.Code,
                    Age = student.Age,
                    Address = student.Address,
                    IsEnrolled = enrolledStudentIds.Any(x => x == student.Id) ? 1 : 0
                });
            }

            return new ResponseModel<List<StudentCourseModel>>
            {
                Status = 1,
                Data = data
            };
        }

        [HttpGet]
        public ActionResult<ResponseModel<List<StudentModel>>> StudentsOfCourse(int id, string keyword)
        {
            

            var course = _context.Courses.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
            if (course == null)
            {
                return new ResponseModel<List<StudentModel>>
                {
                    Status = 0,
                    Message = "Course does not exist"
                };
            }

            var studentIds = _context.CourseStudents.Where(x => x.CourseId == id && x.IsDeleted == 0)
                .Select(x => x.StudentId)
                .ToList();

            var studentDb = _context.Students.Where(x => x.IsDeleted == 0 && studentIds.Contains(x.Id));

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                studentDb = studentDb.Where(x => x.FullName.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            var data = studentDb.Select(x => new StudentModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Code = x.Code,
                Age = x.Age,
                Address = x.Address
            })
            .ToList();

            return new ResponseModel<List<StudentModel>>
            {
                Status = 1,
                Data = data
            };
        }

        [HttpGet]
        public ActionResult<ResponseModel<List<StudentModel>>> StudentsToEnroll(int id, string keyword)
        {
            

            var course = _context.Courses.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
            if (course == null)
            {
                return new ResponseModel<List<StudentModel>>
                {
                    Status = 0,
                    Message = "Course does not exist"
                };
            }

            var studentIds = _context.CourseStudents.Where(x => x.CourseId == id && x.IsDeleted == 0)
                .Select(x => x.StudentId)
                .ToList();

            if (studentIds.Count >= course.MaxStudentNum)
            {
                return new ResponseModel<List<StudentModel>>
                {
                    Status = 0,
                    Message = "Course is full"
                };
            }

            var studentDb = _context.Students.Where(x => x.IsDeleted == 0 && !studentIds.Contains(x.Id));

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                studentDb = studentDb.Where(x => x.FullName.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            var data = studentDb.Select(x => new StudentModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Code = x.Code,
                    Age = x.Age,
                    Address = x.Address
                })
                .ToList();

            return new ResponseModel<List<StudentModel>>
            {
                Status = 1,
                Data = data
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> EnrollStudentToCourse([FromBody] EnrollStudentToCourseRequest request)
        {
            var result = _CourseService.EnrollStudent(request);
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
                Message = "Enroll student to course successfully"
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> RemoveStudentFromCourse([FromBody] RemoveStudentFromCourseRequest request)
        {
            var result = _CourseService.RemoveStudent(request);
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
                Message = "Remove student from course successfully"
            };
        }

        [HttpPost]
        public ActionResult<ResponseModel> UpdateEnrolls([FromBody] UpdateEnrollsRequest request)
        {
            var result = _CourseService.UpdateEnrolls(request);
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
                Message = "Update enrolls successfully"
            };
        }
    }
}

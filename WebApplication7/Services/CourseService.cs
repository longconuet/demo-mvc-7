using Microsoft.EntityFrameworkCore;
using WebApplication7.Data;
using WebApplication7.ModalViews;
using WebApplication7.Models;
using WebApplication7.Requests;

namespace WebApplication7.Services
{
    public interface ICourseService
    {
        bool VerifyCode(string code);
        ServiceResponse Store(AddCourseRequest request);
        ServiceResponse Update(UpdateCourseRequest request);
        ServiceResponse Delete(int id);
        ServiceResponse EnrollStudent(EnrollStudentToCourseRequest request);
        ServiceResponse RemoveStudent(RemoveStudentFromCourseRequest request);
        ServiceResponse UpdateEnrolls(UpdateEnrollsRequest request);
    }

    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool VerifyCode(string code)
        {
            

            return _context.Courses.Any(x => x.Code == code && x.IsDeleted == 0);
        }

        public ServiceResponse Store(AddCourseRequest request)
        {
            try
            {
                

                var checkCode = _context.Courses.Any(x => x.Code == request.Code && x.IsDeleted == 0);
                if (checkCode)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Code is used"
                    };
                }

                _context.Courses.Add(new Course
                {
                    Code = request.Code,
                    Name = request.Name,
                    MaxStudentNum = request.MaxStudentNum
                });
                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Create course failed"
                };
            }
        }

        public ServiceResponse Update(UpdateCourseRequest request)
        {
            try
            {
                

                var course = _context.Courses.FirstOrDefault(x => x.Id == request.Id && x.IsDeleted == 0);
                if (course == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course does not exist"
                    };
                }

                course.Name = request.Name;
                course.MaxStudentNum = request.MaxStudentNum;
                course.UpdatedAt = DateTime.Now;

                _context.Courses.Update(course);
                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Update course failed"
                };
            }
        }

        public ServiceResponse Delete(int id)
        {
            try
            {
                

                var course = _context.Courses.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
                if (course == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course does not exist"
                    };
                }
                    
                course.IsDeleted = 1;
                course.UpdatedAt = DateTime.Now;

                _context.Courses.Update(course);
                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Delete course failed"
                };
            }
        }

        public ServiceResponse EnrollStudent(EnrollStudentToCourseRequest request)
        {
            try
            {
                

                if (!request.StudentIds.Any())
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Please select student"
                    };
                }

                var course = _context.Courses.FirstOrDefault(x => x.Id == request.CourseId && x.IsDeleted == 0);
                if (course == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course does not exist"
                    };
                }

                var enrolledStudentIds = _context.CourseStudents.Where(x => x.CourseId == request.CourseId && x.IsDeleted == 0)
                    .Join(_context.Students, c => c.StudentId, s => s.Id, (c, s) => new { c, s })
                    .Where(x => x.s.IsDeleted == 0)
                    .Select(x => x.c.StudentId)
                    .ToList();
                if (enrolledStudentIds.Count + request.StudentIds.Count > course.MaxStudentNum)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course is full"
                    };
                }

                var studentIds = request.StudentIds.Except(enrolledStudentIds).ToList();

                var insertCourseStudents = studentIds.Select(x => new CourseStudent
                {
                    CourseId = request.CourseId,
                    StudentId = x
                }).ToList();

                _context.CourseStudents.AddRange(insertCourseStudents);
                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Enroll student to course failed"
                };
            }
        }

        public ServiceResponse RemoveStudent(RemoveStudentFromCourseRequest request)
        {
            try
            {
                

                var course = _context.Courses.FirstOrDefault(x => x.Id == request.CourseId && x.IsDeleted == 0);
                if (course == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course does not exist"
                    };
                }

                var courseStudent = _context.CourseStudents.FirstOrDefault(x => x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.IsDeleted == 0);
                if (courseStudent == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Student is not in course"
                    };
                }

                courseStudent.IsDeleted = 1;
                courseStudent.UpdatedAt = DateTime.Now;

                _context.CourseStudents.Update(courseStudent);
                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Remove student from course failed"
                };
            }
        }

        public ServiceResponse UpdateEnrolls(UpdateEnrollsRequest request)
        {
            try
            {

                List<int> insertStudentIds = new();
                List<int> deleteStudentIds = new();

                var course = _context.Courses.FirstOrDefault(x => x.Id == request.CourseId && x.IsDeleted == 0);
                if (course == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course does not exist"
                    };
                }

                var enrolledStudentIds = _context.Students.Where(x => x.IsDeleted == 0 && request.EnrolledStudentIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToList();

                var oldEnrolledStudents = _context.CourseStudents.Where(x => x.CourseId == request.CourseId && x.IsDeleted == 0).ToList();
                var oldEnrolledStudentIds = oldEnrolledStudents.Select(x => x.StudentId).ToList();
                if (enrolledStudentIds.Count > course.MaxStudentNum)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Course is full"
                    };
                }

                // insert
                insertStudentIds = enrolledStudentIds.Except(oldEnrolledStudentIds).ToList();
                if (insertStudentIds.Any())
                {
                    var insertCourseStudents = insertStudentIds.Select(x => new CourseStudent
                    {
                        CourseId = request.CourseId,
                        StudentId = x
                    }).ToList();

                    _context.CourseStudents.AddRange(insertCourseStudents);
                }

                // delete
                deleteStudentIds = oldEnrolledStudentIds.Except(enrolledStudentIds).ToList();
                if (deleteStudentIds.Any())
                {
                    var deleteCourseStudents = oldEnrolledStudents.Where(x => deleteStudentIds.Contains(x.StudentId)).ToList();
                    deleteCourseStudents.ForEach(x => x.IsDeleted = 1);

                    _context.CourseStudents.UpdateRange(deleteCourseStudents);
                }

                _context.SaveChanges();

                return new ServiceResponse
                {
                    Status = 1
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse
                {
                    Status = 0,
                    Message = "Update enrolls failed"
                };
            }
        }

        //public ServiceResponse UpdateEnrolls(UpdateEnrollsRequest request)
        //{
        //    try
        //    {

        //        List<int> insertStudentIds = new();
        //        List<int> deleteStudentIds = new();

        //        var course = _context.Courses.FirstOrDefault(x => x.Id == request.CourseId && x.IsDeleted == 0);
        //        if (course == null)
        //        {
        //            return new ServiceResponse
        //            {
        //                Status = 0,
        //                Message = "Course does not exist"
        //            };
        //        }

        //        var enrolledStudentIds = _context.Students.Where(x => x.IsDeleted == 0 && request.EnrolledStudentIds.Contains(x.Id))
        //            .Select(x => x.Id)
        //            .ToList();

        //        var oldEnrolledStudents = _context.CourseStudents.Where(x => x.CourseId == request.CourseId && x.IsDeleted == 0).ToList();
        //        var oldEnrolledStudentIds = oldEnrolledStudents.Select(x => x.StudentId).ToList();
        //        if (enrolledStudentIds.Count > course.MaxStudentNum)
        //        {
        //            return new ServiceResponse
        //            {
        //                Status = 0,
        //                Message = "Course is full"
        //            };
        //        }

        //        // insert
        //        insertStudentIds = enrolledStudentIds.Except(oldEnrolledStudentIds).ToList();
        //        if (insertStudentIds.Any())
        //        {
        //            var insertCourseStudents = insertStudentIds.Select(x => new CourseStudent
        //            {
        //                CourseId = request.CourseId,
        //                StudentId = x
        //            }).ToList();

        //            _context.CourseStudents.AddRange(insertCourseStudents);
        //        }

        //        // delete
        //        deleteStudentIds = oldEnrolledStudentIds.Except(enrolledStudentIds).ToList();
        //        if (deleteStudentIds.Any())
        //        {
        //            var deleteCourseStudents = oldEnrolledStudents.Where(x => deleteStudentIds.Contains(x.StudentId)).ToList();
        //            deleteCourseStudents.ForEach(x => x.IsDeleted = 1);

        //            _context.CourseStudents.UpdateRange(deleteCourseStudents);
        //        }

        //        _context.SaveChanges();

        //        return new ServiceResponse
        //        {
        //            Status = 1
        //        };
        //    }
        //    catch (Exception e)
        //    {
        //        return new ServiceResponse
        //        {
        //            Status = 0,
        //            Message = "Update enrolls failed"
        //        };
        //    }
        //}
    }
}

using WebApplication7.Data;
using WebApplication7.ModalViews;
using WebApplication7.Models;
using WebApplication7.Requests;

namespace WebApplication7.Services
{
    public interface IStudentService
    {
        bool VerifyCode(string code);
        ServiceResponse Store(AddStudentRequest student);
        ServiceResponse Update(UpdateStudentRequest request);
        ServiceResponse Delete(int id);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool VerifyCode(string code)
        {
            

            return _context.Students.Any(x => x.Code == code && x.IsDeleted == 0);
        }

        public ServiceResponse Store(AddStudentRequest request)
        {
            try
            {
                

                var checkCode = _context.Students.Any(x => x.Code == request.Code && x.IsDeleted == 0);
                if (checkCode)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Code is used"
                    };
                }

                _context.Students.Add(new Student
                {
                    Code = request.Code,
                    FullName = request.FullName,
                    Age = request.Age,
                    Address = request.Address
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
                    Message = "Create student failed"
                };
            }
        }

        public ServiceResponse Update(UpdateStudentRequest request)
        {
            try
            {
                

                var student = _context.Students.FirstOrDefault(x => x.Id == request.Id && x.IsDeleted == 0);
                if (student == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Student is not exist"
                    };
                }

                student.FullName = request.FullName;
                student.Age = request.Age;
                student.Address = request.Address;
                student.UpdatedAt = DateTime.Now;

                _context.Students.Update(student);
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
                    Message = "Update student failed"
                };
            }
        }

        public ServiceResponse Delete(int id)
        {
            try
            {
                

                var student = _context.Students.FirstOrDefault(x => x.Id == id && x.IsDeleted == 0);
                if (student == null)
                {
                    return new ServiceResponse
                    {
                        Status = 0,
                        Message = "Student does not exist"
                    };
                }

                student.IsDeleted = 1;
                student.UpdatedAt = DateTime.Now;

                _context.Students.Update(student);
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
                    Message = "Delete student failed"
                };
            }
        }
    }
}

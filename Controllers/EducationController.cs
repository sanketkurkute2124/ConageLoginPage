using LoginRegistration.Data;
using LoginRegistration.DTOs.EducationDTOs;
using LoginRegistration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EducationController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("AddEducation")]
        public async Task<IActionResult> AddEducation([FromForm] EducationDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);

                if (customer == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Customer not found."
                    });
                }

                string fileName = string.Empty;

                if (dto.Certificate != null && dto.Certificate.Length > 0)
                {
                    string webRootPath = Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot");

                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                    }

                    string certificateFolder = Path.Combine(
                        webRootPath,
                        "Certificates");

                    if (!Directory.Exists(certificateFolder))
                    {
                        Directory.CreateDirectory(certificateFolder);
                    }

                    fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(dto.Certificate.FileName);

                    string filePath = Path.Combine(
                        certificateFolder,
                        fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Certificate.CopyToAsync(stream);
                    }
                }

                var education = new Education
                {
                    CustomerId = dto.CustomerId,
                    Qualification = dto.Qualification,
                    CollegeName = dto.CollegeName,
                    University = dto.University,
                    PassingYear = dto.PassingYear,
                    Percentage = dto.Percentage,
                    CertificatePath = fileName
                };

                _context.Educations.Add(education);
                await _context.SaveChangesAsync();

                var response = new EducationResponseDto
                {
                    Id = education.Id,
                    CustomerId = education.CustomerId,
                    Qualification = education.Qualification,
                    CollegeName = education.CollegeName,
                    University = education.University,
                    PassingYear = education.PassingYear,
                    Percentage = education.Percentage,
                    CertificatePath = education.CertificatePath
                };

                return Ok(new
                {
                    Success = true,
                    Message = "Education details saved successfully.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetAllEducation")]
        public async Task<IActionResult> GetAllEducation()
        {
            try
            {
                var data = await _context.Educations
                    .Select(x => new EducationResponseDto
                    {
                        Id = x.Id,
                        CustomerId = x.CustomerId,
                        Qualification = x.Qualification,
                        CollegeName = x.CollegeName,
                        University = x.University,
                        PassingYear = x.PassingYear,
                        Percentage = x.Percentage,
                        CertificatePath = x.CertificatePath
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetEducationById/{id}")]
        public async Task<IActionResult> GetEducationById(int id)
        {
            try
            {
                var education = await _context.Educations
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (education == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Education not found."
                    });
                }

                var response = new EducationResponseDto
                {
                    Id = education.Id,
                    CustomerId = education.CustomerId,
                    Qualification = education.Qualification,
                    CollegeName = education.CollegeName,
                    University = education.University,
                    PassingYear = education.PassingYear,
                    Percentage = education.Percentage,
                    CertificatePath = education.CertificatePath
                };

                return Ok(new
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [HttpDelete("DeleteEducation/{id}")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            try
            {
                var education = await _context.Educations.FindAsync(id);

                if (education == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Education not found."
                    });
                }

                _context.Educations.Remove(education);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Education deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        //for customer id wise data
        [HttpGet("GetAllCustomerEducation")]
        public async Task<IActionResult> GetAllCustomerEducation()
        {
            var result = await (from customer in _context.Customers
                                join education in _context.Educations
                                on customer.Id equals education.CustomerId into eduGroup
                                from education in eduGroup.DefaultIfEmpty()
                                
                                select new
                                {
                                    CustomerId = customer.Id,
                                    customer.FirstName,
                                    customer.LastName,
                                    customer.Email,
                                    //customer.MobileNumber,

                                    EducationId = education != null ? education.Id : (int?)null,
                                    Qualification = education != null ? education.Qualification : null,
                                    University = education != null ? education.University : null,
                                    College = education != null ? education.CollegeName : null,
                                    PassingYear = education != null ? education.PassingYear : (int?)null,
                                    Percentage = education != null ? education.Percentage : (decimal?)null
                                }).ToListAsync();

            if (!result.Any())
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No customer records found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Customer education details retrieved successfully.",
                Count = result.Count,
                Data = result
            });
        }

        [HttpGet("GetAllCustomerEducationByCustomerId/{customerId}")]
        public async Task<IActionResult> GetAllCustomerEducationByCustomerId(int customerId)
        {
            var result = await (
              from education in _context.Educations
              join customer in _context.Customers
                  on education.CustomerId equals customer.Id
              where education.CustomerId == customerId
              select new
                {
                    CustomerId = customer.Id,
                    customer.FirstName,
                    customer.LastName,
                    customer.Email,

                    EducationId = education != null ? education.Id : (int?)null,
                    Qualification = education != null ? education.Qualification : null,
                    University = education != null ? education.University : null,
                    College = education != null ? education.CollegeName : null,
                    PassingYear = education != null ? education.PassingYear : (int?)null,
                    Percentage = education != null ? education.Percentage : (decimal?)null,
                    certificate = education != null ? education.CertificatePath : (string?)null
                }
            ).ToListAsync();

            if (!result.Any())
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No education records found for this customer."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Customer education details retrieved successfully.",
                Count = result.Count,
                Data = result
            });
        }

        //Update the education and the customer details  from table
        [HttpPut("UpdateCustomerEducation/{educationId}")]
        public async Task<IActionResult> UpdateCustomerEducation(int educationId,[FromForm] UpdateCustomerEducationDTO model,IFormFile? Certificate)
        {
            //var education = await _context.Educations.FindAsync(educationId);
            var education = await _context.Educations.FirstOrDefaultAsync(x => x.Id == educationId);
            if (education == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Education record not found."
                });
            }

            //var customer = await _context.Customers
            //    .FirstOrDefaultAsync(x => x.Id == education.CustomerId);

            //if (customer == null)
            //{
            //    return NotFound(new
            //    {
            //        Success = false,
            //        Message = "Customer not found."
            //    });
            //}

            // Update Customer
            //customer.FirstName = model.FirstName;
            //customer.LastName = model.LastName;
            //customer.Email = model.Email;

            // Update Education
            education.Qualification = model.Qualification;
            education.University = model.University;
            education.CollegeName = model.CollegeName;
            education.PassingYear = model.PassingYear;
            education.Percentage = model.Percentage;

            // Update Certificate
            if (Certificate != null && Certificate.Length > 0)
            {
                if (!string.IsNullOrEmpty(education.CertificatePath))
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Certificates",
                        education.CertificatePath);

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(Certificate.FileName);

                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Certificates");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Certificate.CopyToAsync(stream);
                }

                education.CertificatePath = fileName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Customer and Education updated successfully."
            });
        }


    }
}
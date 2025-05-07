using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using userPanelOMR.Context;
using userPanelOMR.model;
using userPanelOMR.Service;
using static System.Net.WebRequestMethods;

namespace userPanelOMR.Controllers.userAuth
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignupController : ControllerBase
    {
        private readonly JWTContext _context;
        private readonly otpMail _otpmail;
        private readonly HashEncption _HashPwd;
        private readonly jwtTokenGen _JwtToken;


        public SignupController(JWTContext jwt, otpMail otpMail, HashEncption hashPwd, jwtTokenGen JwtToken)
        {
            _context = jwt;
            _otpmail = otpMail;
            _HashPwd = hashPwd;
            _JwtToken = JwtToken;
        }

        // Done
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> signup(string name, string email, int contact, string pwd, string role, string otp)
        {
            dynamic res;
            string querry;
            if(!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email) || !int.IsNegative(contact) || !string.IsNullOrEmpty(pwd) || !string.IsNullOrEmpty(role))
            {
                try
                {
                    var userNames = _context.singUps.Select(x => x.sEmail).ToList();

                    foreach(var userName in userNames){
                        if(userName == email)
                        {
                            return res = new
                            {
                                state = false,
                                message = @$"{email} is Already Exist, Try Diffrenct.",
                            };
                        }
                    }

                    DateTime expiryTimeUtc = DateTime.UtcNow.AddMinutes(10);
                    // Convert UTC to India Time (IST)
                    TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    DateTime expiryTimeIndia = TimeZoneInfo.ConvertTimeFromUtc(expiryTimeUtc, indiaTimeZone);

                    pwd = _HashPwd.ComputeSha256Hash(pwd);

                    IActionResult UserOTP = await _otpmail.sendOtp(email);
                    if (UserOTP is OkObjectResult GetRespon)
                    {
                        otp = GetRespon.Value.ToString();
                    }
                    var saveRes = _context.Add(new SingUps
                    {
                        sName = name,
                        sEmail = email,
                        sContact = contact,
                        sPassword = pwd,
                        sOtp = otp,
                        ExpiryDate = expiryTimeIndia,
                        IsVerified = false,
                        role = role
                    });
                    _context.SaveChanges();
                    res = new
                    {
                        otp = otp,
                        massege = "Record Has been saved",
                        status = true
                    };

                }
                catch (Exception ex)
                {
                    res = new
                    {
                        state = false,
                        message = ex.Message
                    };
                }
            }
            return Ok("res");
        }

        // Done
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> deleteUser(string email)
        {
            dynamic res;

            try
            {
                var userDel = _context.singUps.FirstOrDefault(a=>a.sEmail == email);
                if (userDel!=null)
                {
                    _context.singUps.Remove(userDel);
                    await _context.SaveChangesAsync();
                    return Ok(new { state = true, message = "List Deleted" });
                }
                else
                {
                    return BadRequest("Record not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Done
        [HttpPost]
        [Route("ForgetGen")]
        public async Task<IActionResult> forgetPwd1(string email)
        {
            dynamic res;
            if (!string.IsNullOrEmpty(email))
            {
                try {
                    var isfound = _context.singUps.FirstOrDefault(a => a.sEmail == email);
                    if (isfound != null)
                    {
                        DateTime expiryTimeUtc = DateTime.UtcNow.AddMinutes(10);
                        // Convert UTC to India Time (IST)
                        TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        DateTime expiryTimeIndia = TimeZoneInfo.ConvertTimeFromUtc(expiryTimeUtc, indiaTimeZone);
                        IActionResult UserOTP = await _otpmail.sendOtp(email);
                        if (UserOTP is OkObjectResult GetRespon)
                        {
                            var otp = GetRespon.Value.ToString();
                            isfound.sOtp = otp;
                            _context.SaveChanges();
                        }
                        isfound.IsVerified = false;
                        isfound.ExpiryDate = expiryTimeIndia;
                        _context.SaveChanges();
                        res = new
                        {
                            state = true,
                            massage = @$"OTP Sent on {email}"
                        };
                    }
                    else
                    {
                        res = new
                        {
                            state = false,
                            massage = @$"Record Not found"
                        };
                    }
                } 
                catch (Exception ex) {
                    res = new
                    {
                        state = false,
                        massage = ex.Message
                    };
                }
            }
            else
            {
                res = new
                {
                    state = false,
                    massage = "Format's Not Match"
                };
            }
            return Ok(res);
        }


        [HttpPost]
        [Route("ForgetPwd")]
        public async Task<IActionResult> forgetpwd2(string eamil, string otp, string pwd)
        {
            dynamic res;
            if(!string.IsNullOrEmpty(eamil) || !string.IsNullOrEmpty(otp) || !string.IsNullOrEmpty(pwd))
            {
                try
                {
                    DateTime TimeUtc = DateTime.UtcNow;
                    TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    DateTime TimeIndia = TimeZoneInfo.ConvertTimeFromUtc(TimeUtc, indiaTimeZone);

                    pwd = _HashPwd.ComputeSha256Hash(pwd);

                    var isVAlids = _context.singUps.FirstOrDefault(s => s.sEmail == eamil);
                    if (isVAlids != null)
                    {
                        if (isVAlids.sOtp == otp)
                        {
                            if (isVAlids.ExpiryDate < TimeIndia)
                            {
                                res = "OTP Expaired";
                            }
                            else
                            {
                                isVAlids.IsVerified = true;
                                isVAlids.sPassword = pwd;
                                _context.SaveChanges();
                                return res = new
                                {
                                    message = $"Password Has been updated",
                                    state = true,
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return res = new
                    {
                        message = ex.Message,
                        state = false,
                    };
                }
            }
            else
            {
                return res = new
                {
                    status = false,
                    result = "Value is Invalid"
                };
            }
            return Ok("res");
        }

        // Email, OTP, Password RePwd are matched then PWD will be set as per user id


        [HttpGet]
        [Route("Varify")]
        public async Task<IActionResult> verify(string user, string pwd, string otp)
        {
            dynamic res;
            if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(pwd) || !string.IsNullOrEmpty(otp))
            {
                pwd = _HashPwd.ComputeSha256Hash(pwd);
                DateTime TimeUtc = DateTime.UtcNow;
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime TimeIndia = TimeZoneInfo.ConvertTimeFromUtc(TimeUtc, indiaTimeZone);
                var otpRecord = _context.singUps.FirstOrDefault(o => o.sEmail == user && o.sPassword ==pwd && o.sOtp == otp);
                if (otpRecord!=null)
                {
                    if (otpRecord.ExpiryDate < TimeIndia)
                    {
                        res = "OTP Expaired";
                    }
                    else
                    {
                       // OTP Validate Succesfully
                        otpRecord.IsVerified = true;
                        var token = _JwtToken.GenerateJwtToken(otpRecord);
                        _context.SaveChanges();
                        // Save into UserTable with Requried details To Genrate Token
                        return Ok(new
                        {
                            message = $"Login Success",
                            token = token,
                        });
                    }
                }
                else
                {
                    res = new
                    {
                        massege = "Not Found in DB",
                        status = false
                    };
                    
                }
            }
            else
            {
                res = new
                {
                    massege = "Please Add Valid Details",
                    status = false
                };
            }
            return Ok("res");
        }


        [HttpGet]
        [Route("Signin")]
        public async Task<IActionResult> signin(string userName, string pwd) {
            dynamic res;
            string querry = string.Empty;

            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(pwd))
            {
                
                try
                {
           
                    pwd = _HashPwd.ComputeSha256Hash(pwd);

                    var otpRecord = _context.singUps.FirstOrDefault(o => o.sEmail == userName && o.sPassword == pwd);
                    if(otpRecord == null)
                    {
                        return res = new
                        {
                            message = "User Not Found",
                            status = false
                        };
                    }
                    else
                    {
                        var token = _JwtToken.GenerateJwtToken(otpRecord);
                        res = new
                        {
                            message = "Login Success",
                            token = token,
                            status = true
                        };
                    }
                }
                catch(Exception ex)
                {
                    res = new
                    {
                        message = ex.Message,
                        status = false
                    };
                }
            }
            else
            {
                res = new
                {
                    status = false,
                    result = "Value is Invalid"
                };
            }
            return Ok(res);
        }



        // Done
        [HttpGet]
        [Route("GetList")]
        public IActionResult GetList()
        {

            // Get all data from EmpClass
            var empList = _context.singUps.ToList();
            if (empList == null)
            {
                BadRequest("No Data Found");
            }
            else if (empList.Any())
            {
                //var fs = empList.First();
                var fs = empList;
                //var fs = empList.Last();
                return Ok(fs);
            }
            return Ok(empList);
        }


    }
       
     public class signin
    {
        public string uname { get; set; } = "";
        public string pasword { get; set; } = "";
        public string otp { get; set; } = "";
    }
   
}

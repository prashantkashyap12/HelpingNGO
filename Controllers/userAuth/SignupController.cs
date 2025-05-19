using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using userPanelOMR.Context;
using userPanelOMR.model;
using userPanelOMR.Service;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // Done -- creating user details to database and send otp to reg. email just 10 min.
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> signup(string name, string email, int contact, string role)
        {
            dynamic res;
            string querry;
            var otp="";
            if(!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email) || !int.IsNegative(contact) || !string.IsNullOrEmpty(role))
            {
                try
                {
                    var userNames = _context.singUps.Select(x => x.sEmail).ToList();
                    foreach(var userName in userNames){
                        if(userName == email)
                        {
                            res = new
                            {
                                state = false,
                                message = @$"{email} is Already Exist, Try Diffrent Email.",
                            };
                        }
                    }
                    DateTime expiryTimeUtc = DateTime.UtcNow.AddMinutes(10);
                    TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    DateTime expiryTimeIndia = TimeZoneInfo.ConvertTimeFromUtc(expiryTimeUtc, indiaTimeZone);

                    IActionResult UserOTP = await _otpmail.sendOtp(email);   // send OTP to email
                    if (UserOTP is OkObjectResult GetRespon)
                    {
                        otp = GetRespon.Value.ToString();
                    }
                    var saveRes = _context.Add(new SingUps
                    {
                        sName = name,
                        sEmail = email,
                        sContact = contact,
                        sPassword = "waiting",
                        sOtp = otp,
                        ExpiryDate = expiryTimeIndia,
                        IsVerified = false,
                        role = role
                    });
                    _context.SaveChanges();
                    res = new
                    {
                        otp = otp,
                        status = true,
                        massege = "Record Has been saved"
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

        // Done -- Check email isExist then remove from db.
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
                     res = new { 
                         state = true, 
                         message = @$"{email} is Deleted from record." 
                     };
                }
                else
                {
                    res = new
                    {
                        state = false,
                        message = $@"{email} not found."
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    massage = ex.Message
                };
            }
            return Ok(res);
        }

        // Done -- does exist then send me OTP
        [HttpPost]
        [Route("ForgetGen1")]
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
                            massage = @$"{email} Not found."
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

        // Done -- does exist then Pwd, re-Pwd update.
        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> forgetpwd2(string eamil, string pwd, string otp)
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
                                res = new
                                {
                                    state = false,
                                    massage = "OTP Expaired"
                                };
                            }
                            else
                            {
                                isVAlids.IsVerified = true;
                                isVAlids.sPassword = pwd;
                                _context.SaveChanges();
                                res = new
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
                    res = new
                    {
                        message = ex.Message,
                        state = false,
                    };
                }
            }
            else
            {
                res = new
                {
                    status = false,
                    result = "Enter valid Information"
                };
            }
            return Ok("res");
        }

        // Done -- return Token isAuthrized
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
                            message = "Login Success, Return Token",
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

        // Done -- return All User List is Auth
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
                var fs = empList;
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



//# signup api : 
//input field<name, email, contact, pwd, role>
// - check Email isUnique
// - Min +10 into IST.
// - Send OTP to Email.
//table field <name, email, contact, pwd, role, otp>   
//Return {otp, status(T/F), massege}

//# verify api :
//input field<email, pwd, otp>
// - getCurrentTime
// - hashing pwd
// - check isEmail Exitst
//    - Time Check OTP Expire <return>
//    - save status, pwd, email.
//    - 

//# Forget api :
//  input field <email>
//  - isExitst
//  - Min +10 into IST  | Save.
//  - Send OTP to Email.


//# Delete api :
//input<email>
// - check exist then delete


//# SignIn Api :
//  input field <user, pwd>
//  - isVarified
//  - make Token(Data) and return to front End<save Local>.

//# All User List :
//  - return All user List

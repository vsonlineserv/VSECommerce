using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class TokenController : VSControllerBase
    {
        private UserService _userService;

        public TokenController(UserService userService, IOptions<AppSettings> _appSettings) : base(_appSettings)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Seller/{StoreId}/token")]
        public IActionResult GetAccessToken(int StoreId, [FromForm] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (loginDTO.grant_type == "password")
                {
                    var validUser = _userService.ValidateUser(StoreId, loginDTO.username, loginDTO.password);
                    if (validUser)
                    {
                        var token = _userService.GetUserToken(StoreId, loginDTO.username);
                        if (token != null)
                        {
                            return Ok(token);
                        }
                        return StatusCode(STATUSCODE_FAILURE, "Error while Login, Please try again.");
                    }
                    else
                    {
                        return StatusCode(STATUSCODE_FAILURE, "You have entered an invalid username or password");
                    }
                }
                else
                {
                    return StatusCode(STATUSCODE_FAILURE, "Invalid Grant Type");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(STATUSCODE_ERROR, "There is some error occured");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("token/vsoauth")]
        public IActionResult GetAccessTokenWithVSOAuth([FromForm] OAuthDTO oAuthDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string userRole = Enums.Role.Guests.ToString();
                if (oAuthDTO.grant_type == "vs-license" && oAuthDTO.tokenOrigin == "vsecommerce")
                {
                    int userId = _userService.ValidateVSUsers(oAuthDTO.username, oAuthDTO.password);
                    if (userId == 0)
                    {
                        return BadRequest("invalid_grant : Invalid credintial received");
                    }
                    userRole = _userService.GetUserRoleForId(userId).ToString();
                    var token = _userService.GenerateJwtToken(userId, userRole);
                    if (token != null)
                    {
                        TokenResult tokenResult = new TokenResult();
                        tokenResult.AccessToken = token.Item1;
                        tokenResult.ValidDateUTC = token.Item2;
                        return Ok(tokenResult);
                    }
                    return StatusCode(STATUSCODE_FAILURE, "User not Exist");

                }
                return BadRequest("invalid_grant : Invalid Grant Type");
            }
            catch (Exception ex)
            {
                return StatusCode(STATUSCODE_ERROR, "There is some error occured");
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("vbuy/token")]
        public IActionResult GetVbuyAccessToken([FromForm] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (loginDTO.grant_type == "password")
                {
                    var validUser = _userService.ValidateVbuyUser(loginDTO.username, loginDTO.password);
                    if (validUser)
                    {
                        var token = _userService.GetVbuyUserToken(loginDTO.username);
                        if (token != null)
                        {
                            return Ok(token);
                        }
                        return StatusCode(STATUSCODE_FAILURE, "Error while Login, Please try again.");
                    }
                    else
                    {
                        return StatusCode(STATUSCODE_FAILURE, "You have entered an invalid username or password");

                    }
                }
                else
                {
                    return StatusCode(STATUSCODE_FAILURE, "Invalid Grant Type");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(STATUSCODE_ERROR, "There is some error occured");
            }
        }


    
    }
}

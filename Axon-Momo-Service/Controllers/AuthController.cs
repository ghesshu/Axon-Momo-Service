// using Axon_Momo_Service.Data;
// using Axon_Momo_Service.Features.Auth;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace Axon_Momo_Service.Controllers
// {
//     [Route("api/bc-authorize")]
//     [ApiController]
//     public class AuthController(DataContext db) : ControllerBase
//     {
        
//         [HttpPost("authorize")]
//         public async Task<IActionResult> Authorize([FromBody] AuthRequest request)
//         {
//             var user = await db.Users.FirstOrDefaultAsync(u => u.Tel == request.Tel);
//             if (user == null)
//             {
//                 return NotFound("User not found.");
//             }
            
//             var interval = 5;
//             var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeMilliseconds();

//             var token = new AuthToken
//             {
//                 UserId = user.Id,
//                 Interval = interval,
//                 Expires_in = expiry
//             };

//             db.AuthTokens.Add(token);
//             await db.SaveChangesAsync();

//             var response = new AuthResponse
//             {
//                 Auth_req_id = token.Id.ToString(),
//                 Interval = interval,
//                 Expires_in = expiry
//             };

//             return Ok(response);
//         }
//     }
// }

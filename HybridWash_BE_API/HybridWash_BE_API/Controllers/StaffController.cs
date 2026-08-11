using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet("today-bookings")]
        public async Task<IActionResult> GetTodayBookings()
        {
            try
            {
                var result = await _staffService.GetTodayBookingsAsync();
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetDailyHistory([FromQuery] string? dateStr)
        {
            try
            {
                // Nếu không truyền ngày thì mặc định lấy hôm nay
                DateOnly date = string.IsNullOrEmpty(dateStr) 
                    ? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)) // VN time
                    : DateOnly.Parse(dateStr);

                var result = await _staffService.GetDailyHistoryAsync(date);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBooking([FromBody] BookingIdRequestDTO request)
        {
            try
            {
                var result = await _staffService.ConfirmBookingAsync(request);
                return Ok(new { Success = result, Message = "Xác nhận lịch thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] BookingIdRequestDTO request)
        {
            try
            {
                var result = await _staffService.CheckInAsync(request);
                return Ok(new { Success = result, Message = "Check-in thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("scan-qr/{qrCode}")]
        public async Task<IActionResult> ScanQrCode(string qrCode)
        {
            try
            {
                var result = await _staffService.GetBookingByQrCodeAsync(qrCode);
                if (result == null)
                    return NotFound(new { Message = "Không tìm thấy lịch đặt xe đang hoạt động với mã QR này." });
                
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("issue-receipt")]
        public async Task<IActionResult> IssueReceipt([FromBody] IssueReceiptRequestDTO request)
        {
            try
            {
                var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(staffIdStr) || !int.TryParse(staffIdStr, out int staffId))
                {
                    return Unauthorized(new { Message = "Staff ID không hợp lệ." });
                }

                var result = await _staffService.IssueReceiptAsync(request, staffId);
                return Ok(new { Success = result, Message = "Phát hành biên bản gửi xe thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("verify-receipt")]
        public async Task<IActionResult> VerifyReceipt([FromBody] VerifyReceiptRequestDTO request)
        {
            try
            {
                var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(staffIdStr) || !int.TryParse(staffIdStr, out int staffId))
                {
                    return Unauthorized(new { Message = "Staff ID không hợp lệ." });
                }

                var result = await _staffService.VerifyReceiptAsync(request, staffId);
                return Ok(new { Success = result, Message = "Xác nhận biên bản gửi xe thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] BookingIdRequestDTO request)
        {
            try
            {
                var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(staffIdStr) || !int.TryParse(staffIdStr, out int staffId))
                {
                    return Unauthorized(new { Message = "Staff ID không hợp lệ." });
                }

                var result = await _staffService.CheckOutAsync(request, staffId);
                return Ok(new { Success = result, Message = "Giao xe thành công. Đơn hàng đã hoàn tất (Completed)." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}

using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.DTOs.Auth;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Gender Gender,
    BloodType BloodType = BloodType.Unknown
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string UserId,
    string Email,
    string FullName,
    string Role,
    string Token,
    DateTime ExpiresAt
);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

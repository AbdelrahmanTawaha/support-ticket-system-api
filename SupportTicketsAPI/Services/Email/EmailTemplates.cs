namespace SupportTicketsAPI.Services.Email
{
    public static class EmailTemplates
    {
        public static string ResetCode(string code, DateTime expiresAt)
        {
            //Verbatim Interpolated String
            return $@"
<div style='font-family: Arial, sans-serif; background:#0b1220; padding:24px;'>
  <div style='max-width:520px; margin:auto; background:#ffffff; border-radius:14px; overflow:hidden;'>
    <div style='background:linear-gradient(90deg,#0f172a,#111827,#1f2937); padding:18px 22px; color:#fff;'>
      <h2 style='margin:0; font-size:20px;'>Password Reset</h2>
      <p style='margin:6px 0 0; font-size:12px; opacity:.8;'>Support Ticket System</p>
    </div>

    <div style='padding:22px; color:#0f172a;'>
      <p style='margin:0 0 12px;'>We received a request to reset your password.</p>

      <div style='background:#f8fafc; border:1px solid #e5e7eb; padding:14px; border-radius:10px; text-align:center;'>
        <div style='font-size:12px; color:#6b7280;'>Your reset code</div>
        <div style='font-size:28px; font-weight:800; letter-spacing:6px; margin-top:6px;'>{code}</div>
      </div>

      <p style='margin:14px 0 0; font-size:12px; color:#6b7280;'>
        This code expires at: <strong>{expiresAt:yyyy-MM-dd HH:mm} UTC</strong>
      </p>

      <hr style='border:none; border-top:1px solid #eee; margin:18px 0;' />

      <p style='margin:0; font-size:11px; color:#9ca3af;'>
        If you didn't request this, you can ignore this email.
      </p>
    </div>
  </div>
</div>";
        }
    }
}

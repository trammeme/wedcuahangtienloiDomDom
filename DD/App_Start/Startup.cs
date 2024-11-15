using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DD.Startup))]  // Đây là nơi bạn tham chiếu đến lớp Startup

namespace DD  // Đảm bảo namespace này trùng với tên bạn sử dụng trong OwinStartup
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cấu hình các middleware của bạn ở đây (ví dụ: Hangfire, bảo mật, v.v.)
            // Ví dụ:
            // app.UseHangfireDashboard(); // Nếu bạn muốn sử dụng Hangfire
            // app.UseHangfireServer();    // Nếu bạn muốn sử dụng Hangfire Server
        }
    }
}

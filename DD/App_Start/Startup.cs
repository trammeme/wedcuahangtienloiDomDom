using Microsoft.Owin;
using Hangfire;
using Hangfire.SqlServer; // nếu bạn dùng SQL để lưu job
using Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Hangfire;
using Hangfire.SqlServer;

[assembly: OwinStartup(typeof(DD.Startup))]  // Đây là nơi bạn tham chiếu đến lớp Startup

namespace DD  // Đảm bảo namespace này trùng với tên bạn sử dụng trong OwinStartup
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          //  Cấu hình các middleware của bạn ở đây (ví dụ: Hangfire, bảo mật, v.v.)
            //Ví dụ:
           //  app.UseHangfireDashboard(); // Nếu bạn muốn sử dụng Hangfire
           //  app.UseHangfireServer();    // Nếu bạn muốn sử dụng Hangfire Server
            app.UseStaticFiles();
        }
    }
}

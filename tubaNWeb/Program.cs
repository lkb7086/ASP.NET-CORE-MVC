using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// TODO: Session
builder.Services.AddSession(options => {
	options.IdleTimeout = TimeSpan.FromMinutes(3000000);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// TODO
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(Path.Combine(@"D:\", @"VideoManagerData")),
	RequestPath = new PathString("/VideoManagerData"),

	// TODO: 브라우저 캐시제거
	//OnPrepareResponse = context =>
	//{
	//	context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
	//	context.Context.Response.Headers.Add("Expires", "-1");
	//}
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

// TODO: Session
app.UseSession();

app.Run();
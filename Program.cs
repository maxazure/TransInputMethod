using TransInputMethod.Services;

namespace TransInputMethod;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // Ensure only one instance is running
        using var mutex = new Mutex(true, "TransInputMethodApp", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("应用程序已在运行中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var appController = new AppController();
            Application.Run(appController);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用程序启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }    
}